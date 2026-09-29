using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

/// <summary>The model vocabulary is ordered, not a set. IDs must never be re-sorted.</summary>
public static class ResearchLabelCatalog
{
    public const int ClassCount = 2350;
    // SHA-256 of the training labels joined by LF, without a trailing LF.
    public const string ExpectedSha256 = "5c7b367099e48f200c3c060a0b938d70bc070ec50e93ec4f670a178007281fc5";
    public const string ManifestHeader = "filename,target_label,target_class_id,verified_label,verified_class_id,label_status,participant_id,session_id,utc_time,width,height,format,labels_sha256";

    public static string[] Parse(string text)
    {
        if (text == null) throw new ArgumentException("Training label asset is missing.");
        var labels = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string line in text.TrimStart('\uFEFF').Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string label = line.Trim();
            if (label.Length == 0) continue;
            if (label.Length != 1 || label[0] < '\uAC00' || label[0] > '\uD7A3' || !seen.Add(label))
                throw new ArgumentException("Label table contains a duplicate or non-Hangul label: " + label);
            labels.Add(label);
        }
        if (labels.Count != ClassCount)
            throw new ArgumentException("Expected 2350 model labels, received " + labels.Count + ". Do not include <rare>.");
        string[] result = labels.ToArray();
        if (!string.Equals(Hash(result), ExpectedSha256, StringComparison.Ordinal))
            throw new ArgumentException("Label order differs from the trained model. Restore ResearchLabels.txt.");
        return result;
    }

    public static string Hash(string[] labels)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", labels)));
            return BitConverter.ToString(digest).Replace("-", "").ToLowerInvariant();
        }
    }

    // The UI uses a 1-based number in the full vocabulary. Model IDs and CSVs
    // remain 0-based, even when the active collection plan is a reordered subset.
    public static bool TryResolveCharacterNumber(string input, int[] collectionIds, out int position, out string error)
    {
        position = -1;
        int number;
        if (!int.TryParse((input ?? "").Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out number) ||
            number < 1 || number > ClassCount)
        {
            error = "请输入 1–" + ClassCount + " 的整数序号";
            return false;
        }
        if (collectionIds == null || collectionIds.Length == 0)
        {
            error = "采集字表尚未就绪";
            return false;
        }
        position = Array.IndexOf(collectionIds, number - 1);
        if (position < 0)
        {
            error = "该字不在当前采集字表中";
            return false;
        }
        error = "";
        return true;
    }

    public static int[] BuildPlan(string[] labels, string characters)
    {
        if (string.IsNullOrWhiteSpace(characters))
        {
            int[] all = new int[labels.Length];
            for (int i = 0; i < all.Length; i++) all[i] = i;
            return all;
        }
        var selected = new List<int>();
        var seen = new HashSet<int>();
        foreach (char ch in characters.Normalize(NormalizationForm.FormC))
        {
            if (char.IsWhiteSpace(ch) || ch == ',' || ch == '，' || ch == ';' || ch == '；' || ch == '、') continue;
            int id = Array.IndexOf(labels, ch.ToString());
            if (id < 0) throw new ArgumentException("Character is outside the model vocabulary: " + ch);
            if (seen.Add(id)) selected.Add(id);
        }
        if (selected.Count == 0) throw new ArgumentException("Collection character list contains no Hangul characters.");
        return selected.ToArray();
    }
}
