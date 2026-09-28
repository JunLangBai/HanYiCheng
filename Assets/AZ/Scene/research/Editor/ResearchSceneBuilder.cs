using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Copies the writing UI layout from Level1-1 into an independent research
/// scene. Only the capture board, finish/clear buttons and status remain.
/// All source-level OCR, question and completion scripts stay in Level1-1.
/// </summary>
public static class ResearchSceneBuilder
{
    private const string SourcePath = "Assets/AZ/Scene/关卡/Level1-1.unity";
    private const string TargetPath = "Assets/AZ/Scene/research/ResearchCapture.unity";

    [MenuItem("Tools/Research/Build Capture Scene")]
    public static void Build()
    {
        Scene source = SceneManager.GetSceneByPath(SourcePath);
        bool openedSource = !source.isLoaded;
        if (openedSource)
            source = EditorSceneManager.OpenScene(SourcePath, OpenSceneMode.Additive);

        Scene target = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            if (Root(source, "PointableUI") == null)
                throw new InvalidOperationException("Level1-1 has no PointableUI root.");

            EditorSceneManager.SetActiveScene(target);
            CloneRoot(source, target, "[RKInput]");
            CloneRoot(source, target, "RKCameraRig");
            GameObject ui = CloneRoot(source, target, "PointableUI");

            Transform canvas = FindDeep(ui.transform, "Canvas");
            Transform board = FindDeep(ui.transform, "RawImage");
            Transform clear = FindDeep(ui.transform, "clearButton");
            Transform finish = FindDeep(ui.transform, "识别");
            Transform status = FindDeep(ui.transform, "结果");
            if (canvas == null || board == null || clear == null || finish == null || status == null)
                throw new InvalidOperationException(
                    "The source layout has changed. Required: Canvas, RawImage, clearButton, 识别, 结果.");

            // Keep the original relative layout but center it in the research
            // view. The source scene's -0.11 m vertical offset clips the lower
            // half of the board in the standalone Game view.
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector3 canvasPosition = canvasRect.anchoredPosition3D;
            canvasPosition.y = 0f;
            canvasRect.anchoredPosition3D = canvasPosition;

            // Preserve the original positions, sizes and background graphics
            // on the ancestors. The copybook and question branches go away.
            HashSet<Transform> keep = new HashSet<Transform>();
            KeepAncestors(board, canvas, keep);
            KeepAncestors(clear, canvas, keep);
            KeepAncestors(finish, canvas, keep);
            KeepAncestors(status, canvas, keep);
            KeepDescendants(board, keep);
            KeepDescendants(clear, keep);
            KeepDescendants(finish, keep);
            KeepDescendants(status, keep);
            Prune(canvas, keep);

            // The copied Canvas must not execute level/question/OCR scripts.
            MonoBehaviour[] behaviours = canvas.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null) continue;
                if (behaviour.GetType().Assembly.GetName().Name == "Assembly-CSharp")
                    UnityEngine.Object.DestroyImmediate(behaviour);
            }

            RawImage drawing = board.GetComponent<RawImage>();
            Button clearButton = clear.GetComponent<Button>();
            Button finishButton = finish.GetComponent<Button>();
            TMP_Text statusText = status.GetComponent<TMP_Text>();
            if (drawing == null || clearButton == null || finishButton == null || statusText == null)
                throw new InvalidOperationException("The source board/button/text components are missing.");

            clearButton.onClick.RemoveAllListeners();
            finishButton.onClick.RemoveAllListeners();
            TMP_Text finishLabel = finish.GetComponentInChildren<TMP_Text>(true);
            if (finishLabel != null) finishLabel.text = "完成";
            statusText.text = "请在框内写字";
            statusText.raycastTarget = false;

            ResearchCapture capture = board.gameObject.AddComponent<ResearchCapture>();
            capture.drawingImage = drawing;
            capture.clearButton = clearButton;
            capture.finishButton = finishButton;
            capture.statusText = statusText;
            capture.guidedCollection = true;
            capture.labelFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AZ/Scene/research/ResearchLabels.txt");
            capture.promptFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/AZ/Font/NanumMyeongjoBold SDF.asset");
            if (capture.labelFile == null || capture.promptFont == null)
                throw new InvalidOperationException("ResearchLabels.txt and the Korean prompt font are required.");
            ResearchLabelCatalog.Parse(capture.labelFile.text);

            // Retained background Images must not intercept handwriting.
            foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
                if (image.GetComponent<Button>() == null) image.raycastTarget = false;

            EditorSceneManager.MarkSceneDirty(target);
            if (!EditorSceneManager.SaveScene(target, TargetPath))
                throw new InvalidOperationException("Unity could not save " + TargetPath);
            AssetDatabase.Refresh();
            Debug.Log("Research scene created: " + TargetPath);
        }
        catch
        {
            EditorSceneManager.CloseScene(target, true);
            throw;
        }
        finally
        {
            if (openedSource && source.isLoaded)
                EditorSceneManager.CloseScene(source, true);
        }
    }

    private static GameObject Root(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == name) return root;
        return null;
    }

    private static GameObject CloneRoot(Scene source, Scene target, string name)
    {
        GameObject original = Root(source, name);
        if (original == null)
            throw new InvalidOperationException("Level1-1 has no " + name + " root.");
        GameObject copy = UnityEngine.Object.Instantiate(original);
        copy.name = original.name;
        copy.transform.SetParent(null);
        SceneManager.MoveGameObjectToScene(copy, target);
        return copy;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    private static void KeepAncestors(Transform leaf, Transform canvas, HashSet<Transform> keep)
    {
        for (Transform current = leaf; current != null; current = current.parent)
        {
            keep.Add(current);
            if (current == canvas) return;
        }
        throw new InvalidOperationException(leaf.name + " is not inside the Canvas.");
    }

    private static void KeepDescendants(Transform root, HashSet<Transform> keep)
    {
        keep.Add(root);
        for (int i = 0; i < root.childCount; i++)
            KeepDescendants(root.GetChild(i), keep);
    }

    private static void Prune(Transform parent, HashSet<Transform> keep)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (!keep.Contains(child))
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            else
                Prune(child, keep);
        }
    }
}
