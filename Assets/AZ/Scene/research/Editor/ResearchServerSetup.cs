using System.IO;
using UnityEditor;
using UnityEngine;

public static class ResearchServerSetup
{
    [MenuItem("Tools/Research/Open Server Config Folder")]
    public static void OpenConfigFolder()
    {
        string folder = Path.GetDirectoryName(ResearchCapture.ServerConfigurationPath);
        Directory.CreateDirectory(folder);
        EditorUtility.RevealInFinder(folder);
        Debug.Log("Place the private research_server.json inside: " + folder + ". Do not put it inside Assets or upload it to Git.");
    }
}
