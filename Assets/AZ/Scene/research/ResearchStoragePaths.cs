using System;
using System.IO;

/// <summary>Research data follows the capture script in the Unity editor.</summary>
public static class ResearchStoragePaths
{
    // Pure path conversion, also exercised by the relocation regression check.
    public static string EditorSamplesDirectory(string assetsDirectory, string captureScriptAssetPath)
    {
        if (string.IsNullOrWhiteSpace(assetsDirectory))
            throw new ArgumentException("The project's Assets directory is required.");
        string assetPath = (captureScriptAssetPath ?? "").Replace('\\', '/');
        if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal) ||
            !assetPath.EndsWith("/ResearchCapture.cs", StringComparison.Ordinal))
            throw new ArgumentException("ResearchCapture.cs must be inside this project's Assets folder.");
        foreach (string segment in assetPath.Split('/'))
            if (segment == "." || segment == ".." || segment.Length == 0 || segment.Contains(":"))
                throw new ArgumentException("Invalid capture script asset path.");

        string relativeDirectory = Path.GetDirectoryName(assetPath.Substring("Assets/".Length));
        return Path.GetFullPath(Path.Combine(assetsDirectory, relativeDirectory ?? "", "Samples"));
    }

    public static string PlayerSamplesDirectory(string persistentDataDirectory)
    {
        return Path.Combine(persistentDataDirectory, "research", "Samples");
    }

#if UNITY_EDITOR
    public static string EditorSamplesDirectory(ResearchCapture capture)
    {
        UnityEditor.MonoScript script = UnityEditor.MonoScript.FromMonoBehaviour(capture);
        return EditorSamplesDirectory(UnityEngine.Application.dataPath, UnityEditor.AssetDatabase.GetAssetPath(script));
    }

    // Menu tools have no scene instance. Resolve the unique script by its type,
    // not by an AZ/AQY path or a machine-specific absolute directory. Do not cache:
    // the user can move the folder between invocations without recompiling.
    public static string EditorAssetDirectory
    {
        get
        {
            string directory = null;
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("ResearchCapture t:MonoScript", new[] { "Assets" }))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                UnityEditor.MonoScript script = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(path);
                if (script == null || script.GetClass() != typeof(ResearchCapture)) continue;
                if (directory != null)
                    throw new InvalidOperationException("Multiple ResearchCapture scripts found. Move the research folder instead of duplicating it.");
                directory = Path.GetDirectoryName(path).Replace('\\', '/');
            }
            if (directory == null)
                throw new InvalidOperationException("Cannot locate ResearchCapture.cs under Assets. Wait for Unity to finish importing the scripts.");
            return directory;
        }
    }

    public static string EditorAssetPath(string fileName)
    {
        return EditorAssetDirectory + "/" + fileName;
    }

    public static string CurrentEditorSamplesDirectory
    {
        get { return EditorSamplesDirectory(UnityEngine.Application.dataPath, EditorAssetPath("ResearchCapture.cs")); }
    }
#endif
}
