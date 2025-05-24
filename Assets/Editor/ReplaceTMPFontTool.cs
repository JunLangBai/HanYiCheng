using UnityEditor;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ReplaceTMPFontTool : EditorWindow
{
    private TMP_FontAsset targetFont;

    [MenuItem("Tools/TMP/批量替换TMP字体")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceTMPFontTool>("替换TMP字体");
    }

    void OnGUI()
    {
        GUILayout.Label("批量替换TMP字体设置", EditorStyles.boldLabel);
        
        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "目标字体", 
            targetFont, 
            typeof(TMP_FontAsset), 
            false);

        if (GUILayout.Button("开始替换"))
        {
            if (targetFont == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择目标字体！", "确定");
                return;
            }

            if (EditorUtility.DisplayDialog("警告", 
                "即将修改所有TMP文本的字体，此操作不可逆！\n\n建议先保存场景。\n是否继续？", 
                "继续", 
                "取消"))
            {
                ReplaceAllTMPFonts();
            }
        }
    }

    private void ReplaceAllTMPFonts()
    {
        // 处理当前场景中的对象
        ReplaceInSceneObjects();

        // 处理所有预制体
        ReplaceInPrefabs();

        EditorUtility.DisplayDialog("完成", "字体替换已完成！", "确定");
    }

    private void ReplaceInSceneObjects()
    {
        TextMeshProUGUI[] tmpComponents = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        int count = 0;

        foreach (TextMeshProUGUI tmp in tmpComponents)
        {
            // 跳过预制体实例（后面单独处理）
            if (PrefabUtility.IsPartOfPrefabInstance(tmp))
                continue;

            Undo.RecordObject(tmp, "Change TMP Font");
            tmp.font = targetFont;
            count++;
        }

        Debug.Log($"已修改 {count} 个场景中的TMP组件");
    }

    private void ReplaceInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int modifiedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);

            bool modified = false;
            TextMeshProUGUI[] tmpComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI tmp in tmpComponents)
            {
                if (tmp.font != targetFont)
                {
                    tmp.font = targetFont;
                    modified = true;
                    modifiedCount++;
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        Debug.Log($"已修改 {modifiedCount} 个预制体中的TMP组件");
    }
}