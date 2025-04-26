using UnityEditor;
using UnityEngine;

namespace Febucci.UI.Core.Editors
{
#if UNITY_EDITOR

    [CustomEditor(typeof(TAnimGlobalDataScriptable))]
    internal class TAnimGlobalDataScriptableDrawer : Editor
    {
        private TextAnimatorDrawer.UserPresetDrawer[] appearancesDrawers = new TextAnimatorDrawer.UserPresetDrawer[0];
        private SerializedProperty appearancesPresets;

        private TextAnimatorDrawer.UserPresetDrawer[] behaviorDrawers = new TextAnimatorDrawer.UserPresetDrawer[0];

        private SerializedProperty behaviorPresets;
        private SerializedProperty customActionsArray;

        private SerializedProperty customTagsFormatting;
        private TAnimGlobalDataScriptable script;
        private bool showAppearances;

        private bool showBehaviors;
        private SerializedProperty tagInfo_appearances;
        private SerializedProperty tagInfo_behaviors;

        protected virtual void OnEnable()
        {
            behaviorPresets = serializedObject.FindProperty("globalBehaviorPresets");
            appearancesPresets = serializedObject.FindProperty("globalAppearancePresets");
            customActionsArray = serializedObject.FindProperty("customActions");

            tagInfo_behaviors = serializedObject.FindProperty("tagInfo_behaviors");
            tagInfo_appearances = serializedObject.FindProperty("tagInfo_appearances");
            customTagsFormatting = serializedObject.FindProperty("customTagsFormatting");

            script = (TAnimGlobalDataScriptable)target;


            Undo.undoRedoPerformed += Redo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Redo;
        }

        private void Redo()
        {
            serializedObject.UpdateIfRequiredOrScript(); //I have spent too much searching this method... :(
            Repaint();
            TryResettingTextAnimators();
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
                EditorGUILayout.LabelField("[!!] Remember: Saves are applied in play mode.");

            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

                TextAnimatorDrawer.ShowPresets(ref behaviorDrawers, ref showBehaviors, ref behaviorPresets, false,
                    true);

                TextAnimatorDrawer.ShowPresets(ref appearancesDrawers, ref showAppearances, ref appearancesPresets,
                    true, true);


                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(customActionsArray, true);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Tags Info", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(customTagsFormatting, true);
                if (customTagsFormatting.boolValue)
                {
                    EditorGUILayout.PropertyField(tagInfo_behaviors, true);
                    EditorGUILayout.PropertyField(tagInfo_appearances, true);
                }

                EditorGUI.indentLevel--;
            }


            if (serializedObject.hasModifiedProperties)
            {
                //Repaint();

                //Undo.RecordObject(serializedObject.targetObject, "Changed TextAnimator Global Data Scriptable");
                Undo.RecordObject(script, "Changed TextAnimator Global Data Scriptable");
                EditorUtility.SetDirty(script);

                //Undo.RegisterCompleteObjectUndo(script, "Changed TextAnimator Global Data Scriptable");
                serializedObject.ApplyModifiedProperties();

                Repaint();
                TryResettingTextAnimators();
            }
        }

        private void TryResettingTextAnimators()
        {
            if (EditorApplication.isPlaying) TAnim_EditorHelper.TriggerEvent();
        }
    }

#endif
}