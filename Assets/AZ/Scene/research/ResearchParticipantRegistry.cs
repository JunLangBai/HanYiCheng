using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>Local anonymous ID allocation. Never infers identity from handwriting.</summary>
public sealed class ResearchParticipantRegistry
{
    public const string Header = "participant_id,session_id,created_utc";
    private readonly string directory;
    private readonly HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> sessions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private int highestNumber;
    public string LastParticipant { get; private set; }
    public int SampledParticipantCount { get { return counts.Count; } }

    public ResearchParticipantRegistry(string directory) { this.directory = directory; }

    public static bool IsValidId(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 32 ||
            string.Equals(value, "anonymous", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "unknown", StringComparison.OrdinalIgnoreCase)) return false;
        foreach (char c in value)
            if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z') &&
                !(c >= '0' && c <= '9') && c != '_' && c != '-') return false;
        return true;
    }

    public int CountFor(string id)
    {
        int count;
        return id != null && counts.TryGetValue(id, out count) ? count : 0;
    }

    public string SessionFor(string id)
    {
        string session;
        return id != null && sessions.TryGetValue(id, out session) ? session : "S01";
    }

    private void Remember(string id, string session)
    {
        if (!IsValidId(id)) return; // anonymous legacy samples are not participants.
        ids.Add(id);
        sessions[id] = IsValidId(session) ? session : "S01";
        int number;
        if (id.Length > 1 && (id[0] == 'P' || id[0] == 'p') &&
            int.TryParse(id.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out number))
            highestNumber = Math.Max(highestNumber, number);
    }

    // Read-only. Existing samples/registrations are never renumbered or rewritten.
    public void Reload(string configuredId = null)
    {
        ids.Clear(); counts.Clear(); sessions.Clear(); highestNumber = 0; LastParticipant = "";
        string registrations = Path.Combine(directory, "participants.csv");
        if (File.Exists(registrations))
        {
            List<string[]> rows = ReadCsv(registrations);
            if (rows.Count == 0 || string.Join(",", rows[0]) != Header)
                throw new IOException("Unexpected participant registry header: " + registrations);
            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                if (row.Length != 3 || !IsValidId(row[0]) || !IsValidId(row[1]))
                    throw new IOException("Invalid participant registry row " + (i + 1));
                Remember(row[0], row[1]);
                LastParticipant = row[0];
            }
        }
        string lastRegistered = LastParticipant;
        var seenImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string name in new[] { "samples.csv", "samples_guided_v2.csv" })
        {
            string manifest = Path.Combine(directory, name);
            if (!File.Exists(manifest)) continue;
            List<string[]> rows = ReadCsv(manifest);
            if (rows.Count == 0) throw new IOException("Empty sample manifest: " + manifest);
            int participant = Array.IndexOf(rows[0], "participant_id");
            int filename = Array.IndexOf(rows[0], "filename");
            int session = Array.IndexOf(rows[0], "session_id");
            if (participant < 0 || filename < 0) throw new IOException("Missing participant/filename columns: " + manifest);
            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                if (row.Length != rows[0].Length) throw new IOException("Invalid sample row " + (i + 1) + ": " + manifest);
                string id = row[participant].Trim();
                if (!IsValidId(id)) continue;
                Remember(id, session >= 0 ? row[session].Trim() : "S01");
                LastParticipant = id;
                // Count unique images that still exist, not empty registrations or missing files.
                string image = row[filename];
                if (string.IsNullOrEmpty(image) || image != Path.GetFileName(image)) continue;
                if (File.Exists(Path.Combine(directory, image)) && seenImages.Add(image))
                    counts[id] = CountFor(id) + 1;
            }
        }
        if (!string.IsNullOrEmpty(lastRegistered)) LastParticipant = lastRegistered;
        if (IsValidId(configuredId) && !ids.Contains(configuredId)) Remember(configuredId, "S01");
    }

    public string NextId
    {
        get
        {
            if (highestNumber == int.MaxValue) throw new IOException("Participant number limit reached.");
            return "P" + (highestNumber + 1).ToString("D3", CultureInfo.InvariantCulture);
        }
    }

    public string Create(string expectedId, string configuredId)
    {
        Reload(configuredId);
        string id = NextId;
        if (id != expectedId) throw new IOException("Participant list changed. Reopen the confirmation dialog.");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "participants.csv");
        // A zero-sample registration still reserves its ID across restarts.
        bool hasFile = File.Exists(path);
        using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None))
        using (var writer = new StreamWriter(stream, new UTF8Encoding(!hasFile)))
        {
            // Also handles a CSV editor removing the final newline from an existing file.
            if (hasFile) writer.WriteLine();
            else writer.WriteLine(Header);
            writer.WriteLine(id + ",S01," + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        }
        Remember(id, "S01");
        LastParticipant = id;
        return id;
    }

    public void RecordSaved(string id, string session)
    {
        Remember(id, session);
        counts[id] = CountFor(id) + 1;
    }

    // Supports quoted commas, quotes and newlines from CSV editors, including UTF-8 BOM.
    private static List<string[]> ReadCsv(string path)
    {
        var rows = new List<string[]>();
        var row = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;
        using (var reader = new StreamReader(path, Encoding.UTF8, true))
        {
            int value;
            while ((value = reader.Read()) >= 0)
            {
                char c = (char)value;
                if (c == '"')
                {
                    if (quoted && reader.Peek() == '"') { reader.Read(); field.Append('"'); }
                    else quoted = !quoted;
                }
                else if (!quoted && c == ',') { row.Add(field.ToString()); field.Length = 0; }
                else if (!quoted && (c == '\r' || c == '\n'))
                {
                    if (c == '\r' && reader.Peek() == '\n') reader.Read();
                    row.Add(field.ToString()); field.Length = 0;
                    if (row.Count != 1 || row[0].Length != 0) rows.Add(row.ToArray());
                    row.Clear();
                }
                else field.Append(c);
            }
        }
        if (quoted) throw new IOException("Unclosed CSV quote: " + path);
        if (row.Count > 0 || field.Length > 0) { row.Add(field.ToString()); rows.Add(row.ToArray()); }
        return rows;
    }
}
