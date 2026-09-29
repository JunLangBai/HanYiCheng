using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Batch-only smoke checks. Never stores test images in the research Samples folder.</summary>
public static class ResearchCaptureValidation
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Research smoke check failed: " + message);
    }
    private static void Set(ResearchCapture capture, string field, object value)
    {
        typeof(ResearchCapture).GetField(field, PrivateInstance).SetValue(capture, value);
    }
    private static void Call(ResearchCapture capture, string method, params object[] args)
    {
        typeof(ResearchCapture).GetMethod(method, PrivateInstance).Invoke(capture, args);
    }
    private static void Ink(ResearchCapture capture)
    {
        Call(capture, "DrawDisk", 128, 128, 8);
        Call(capture, "ApplyPixels");
        Set(capture, "hasInk", true);
    }

    // Unity.exe -batchmode -nographics -projectPath ... -executeMethod
    // ResearchCaptureValidation.Run -researchCheckOutput <empty test folder> -quit
    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("This validation is batch-only; use a separate editor process.");
        string[] args = Environment.GetCommandLineArgs();
        int argIndex = Array.IndexOf(args, "-researchCheckOutput");
        if (argIndex < 0 || argIndex + 1 >= args.Length) throw new ArgumentException("Explicit test output directory is required.");
        string output = Path.GetFullPath(args[argIndex + 1]);
        if (Directory.Exists(output)) throw new IOException("Use a new test directory; existing files are never replaced.");
        string assets = Path.GetFullPath(Application.dataPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (output.StartsWith(assets, StringComparison.OrdinalIgnoreCase)) throw new IOException("Test output must be outside Assets.");
        Scene scene = EditorSceneManager.OpenScene(ResearchStoragePaths.EditorAssetPath("ResearchCapture.unity"), OpenSceneMode.Additive);
        try
        {
            ResearchCapture capture = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (capture == null) capture = root.GetComponentInChildren<ResearchCapture>(true);
            Require(capture != null && capture.labelFile != null && capture.promptFont != null, "serialized scene references");
            Require(capture.guidedCollection && !capture.uploadAfterSave, "safe scene defaults");
            Call(capture, "Awake");
            Require(capture.OutputDirectory == ResearchStoragePaths.CurrentEditorSamplesDirectory, "samples directory follows capture script");
            // Never send test dots to a live endpoint even if a private config exists.
            capture.uploadAfterSave = false;
            Set(capture, "outputDirectory", output);
            capture.participantId = "anonymous";
            Call(capture, "InitializeParticipants");
            Require(capture.CurrentTargetCharacter == "가" && capture.CurrentTargetId == 0, "default target");
            Require(!capture.finishButton.interactable && capture.ActiveParticipantId == "", "anonymous cannot save");
            Ink(capture);
            capture.SaveSample();
            Require(!Directory.Exists(output), "anonymous sample is not saved");
            capture.ClearBoard();
            capture.BeginNewParticipant();
            capture.CancelNewParticipant();
            Require(!Directory.Exists(output), "cancel does not register a participant");
            capture.BeginNewParticipant();
            capture.NextTarget();
            Require(capture.CurrentTargetId == 0, "confirmation dialog blocks background navigation");
            Require(!capture.JumpToCharacterNumber("201") && capture.CurrentTargetId == 0, "confirmation dialog blocks numeric jump");
            capture.ConfirmNewParticipant();
            capture.ConfirmNewParticipant();
            Require(capture.ActiveParticipantId == "P001" && capture.finishButton.interactable, "confirmed participant enables save; double confirm is harmless");
            Require(capture.SampledParticipantCount == 0, "zero-image participant not counted as collected");
            capture.NextTarget();
            Require(capture.CurrentTargetId == 1 && capture.CurrentTargetCharacter == "각", "next target");
            capture.PreviousTarget();
            Require(capture.CurrentTargetId == 0, "previous target");
            Transform panel = capture.transform.parent;
            TMP_Text target = panel.Find("ResearchTargetCharacter").GetComponent<TMP_Text>();
            Require(target.text == "가" && target.font.HasCharacter((int)'가'), "visible Hangul prompt");
            TMP_InputField number = panel.Find("ResearchTargetNumberInput").GetComponent<TMP_InputField>();
            Require(number.text == "1" && number.lineType == TMP_InputField.LineType.SingleLine, "number field starts at vocabulary number one");
            number.text = "201";
            panel.Find("ResearchJumpTarget").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Require(capture.CurrentTargetId == 200 && target.text == ResearchLabelCatalog.Parse(capture.labelFile.text)[200], "button jumps to vocabulary position 201 / ID 200");
            number.text = "2350";
            number.onSubmit.Invoke(number.text);
            Require(capture.CurrentTargetId == 2349 && target.text == "힝", "Enter-submit jumps to last class");
            Require(!capture.JumpToCharacterNumber("2351") && capture.CurrentTargetId == 2349, "out-of-range input leaves target unchanged");
            Require(!capture.JumpToCharacterNumber("") && capture.CurrentTargetId == 2349, "empty input leaves target unchanged");
            Require(capture.JumpToCharacterNumber("1") && target.text == "가", "jump back to first class");
            float boardRight = capture.drawingImage.rectTransform.anchoredPosition.x + capture.drawingImage.rectTransform.rect.width / 2;
            foreach (string name in new[] { "ResearchTargetHeading", "ResearchTargetCharacter", "ResearchTargetInfo", "ResearchPreviousTarget", "ResearchNextTarget", "ResearchJumpHint", "ResearchTargetNumberInput", "ResearchJumpTarget" })
            {
                RectTransform rect = panel.Find(name).GetComponent<RectTransform>();
                Require(rect.anchoredPosition.x - rect.rect.width / 2 > boardRight, "prompt outside board: " + name);
            }
            float boardTop = capture.drawingImage.rectTransform.anchoredPosition.y + capture.drawingImage.rectTransform.rect.height / 2;
            foreach (string name in new[] { "ResearchParticipantInfo", "ResearchNewParticipant", "ResearchResumeParticipant" })
            {
                RectTransform rect = panel.Find(name).GetComponent<RectTransform>();
                Require(rect.anchoredPosition.y - rect.rect.height / 2 > boardTop, "participant UI above board: " + name);
            }
            capture.SaveSample();
            Require(Directory.GetFiles(output, "*.jpg").Length == 0, "blank board is not saved");
            string[] labels = ResearchLabelCatalog.Parse(capture.labelFile.text);
            Set(capture, "collectionIds", ResearchLabelCatalog.BuildPlan(labels, "한 글 가"));
            Set(capture, "targetPosition", 0);
            Call(capture, "UpdateTargetUI");
            Require(capture.CurrentTargetId == 2210 && target.text == "한", "subset uses model ID");
            Require(number.text == "2211", "number field shows vocabulary number, not subset position");
            Require(!capture.JumpToCharacterNumber("201") && capture.CurrentTargetId == 2210, "jump cannot leave configured collection subset");
            Require(capture.JumpToCharacterNumber("153") && capture.CurrentTargetId == 152, "subset jump resolves global number");
            Require(capture.JumpToCharacterNumber("2211") && capture.CurrentTargetId == 2210, "subset jump back before save");
            Ink(capture);
            capture.NextTarget();
            Require(capture.CurrentTargetId == 2210, "unsaved ink blocks relabeling");
            Require(!capture.JumpToCharacterNumber("153") && capture.CurrentTargetId == 2210, "numeric jump cannot relabel unsaved ink");
            // Inspector edits mid-stroke cannot relabel the active sample.
            capture.participantId = "P777";
            capture.SaveSample();
            Require(Directory.GetFiles(output, "*.jpg").Length == 1, "one successful image save");
            string csv = File.ReadAllText(Path.Combine(output, "samples_guided_v2.csv"));
            Require(csv.Contains(",\"한\",2210,,,pending_review,\"P001\",\"S01\""), "target and active participant snapshotted independently of Inspector");
            Require(capture.CurrentParticipantSampleCount == 1 && capture.SampledParticipantCount == 1, "successful save updates participant counts");
            Require(capture.CurrentTargetId == 2210, "default repeats same target");
            Require(!File.Exists(Path.Combine(output, "samples.csv")), "old schema not modified");
            capture.NextTarget();
            Require(capture.CurrentTargetId == 152, "navigation after clear");
            capture.advanceAfterSave = true;
            Ink(capture);
            capture.SaveSample();
            Require(capture.CurrentTargetId == 0, "optional automatic advance");
            Require(Directory.GetFiles(output, "*.jpg").Length == 2, "second save");
            capture.clearAfterSave = false;
            Ink(capture);
            capture.SaveSample();
            Require(capture.CurrentTargetId == 0, "no advance without clearing");
            capture.NextTarget();
            Require(capture.CurrentTargetId == 0, "retained ink still blocks navigation");
            Require(Directory.GetFiles(output, "*.jpg").Length == 3, "third save");
            capture.BeginNewParticipant();
            capture.ConfirmNewParticipant();
            Require(capture.ActiveParticipantId == "P001", "retained ink blocks participant switch");
            capture.ClearBoard();
            capture.BeginNewParticipant();
            capture.CancelNewParticipant();
            Require(capture.ActiveParticipantId == "P001" && capture.CurrentParticipantSampleCount == 3, "cancel preserves current participant and counts");
            capture.BeginNewParticipant();
            capture.ConfirmNewParticipant();
            Require(capture.ActiveParticipantId == "P002" && capture.CurrentParticipantSampleCount == 0, "next writer uses new ID and zero count");
            Require(capture.CurrentTargetId == 2210 && capture.SampledParticipantCount == 1, "new writer starts at first character; registration not counted as data");
            capture.clearAfterSave = true;
            Ink(capture);
            capture.SaveSample();
            Require(capture.CurrentParticipantSampleCount == 1 && capture.SampledParticipantCount == 2, "two writers have data");
            capture.participantId = "anonymous";
            Call(capture, "InitializeParticipants");
            Require(capture.ActiveParticipantId == "" && !capture.finishButton.interactable, "restart requires explicit participant selection");
            capture.ResumeLastParticipant();
            Require(capture.ActiveParticipantId == "P002" && capture.CurrentParticipantSampleCount == 1 && capture.SampledParticipantCount == 2, "resume restores ID/count without adding person");
            capture.BeginNewParticipant();
            capture.ConfirmNewParticipant();
            Require(capture.ActiveParticipantId == "P003" && capture.SampledParticipantCount == 2, "restart does not reuse allocated IDs");
            Require(Directory.GetFiles(output, "*.jpg").Length == 4, "only four deliberate saves");
            File.WriteAllText(Path.Combine(output, "validation.txt"), "PASS: scene bindings, Hangul prompt, UI outside board, button/Enter numeric jumps, range/subset checks, navigation, blank prevention, target lock, JPEG save, v2 metadata, repeat/auto-advance, new participant confirm/cancel, participant lock, resume/restart and counts. Test dots are not real handwriting.\n");
            Debug.Log("RESEARCH_GUIDED_CAPTURE_SMOKE_PASS " + output);
        }
        finally
        {
            // Close the transient loaded scene without persisting any test/UI changes.
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
