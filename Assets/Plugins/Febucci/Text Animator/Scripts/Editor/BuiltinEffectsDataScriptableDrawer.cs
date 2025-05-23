﻿using UnityEditor;
using UnityEngine;

namespace Febucci.UI.Core.Editors
{
    internal abstract class BuiltinDataScriptableDrawer : Editor
    {
        private TextAnimatorDrawer.BuiltinVariablesDrawer effectsDrawer;
        private SerializedProperty scriptable;

        protected virtual void OnEnable()
        {
            scriptable = serializedObject.FindProperty("effectValues");

            effectsDrawer = InitializeDrawer(scriptable);
        }

        protected abstract TextAnimatorDrawer.BuiltinVariablesDrawer InitializeDrawer(SerializedProperty property);

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Editing shared built-in values", EditorStyles.boldLabel);

            GUI.enabled = false;

            EditorGUILayout.LabelField(
                "TextAnimators that reference this asset will use and share these built-in effect values.",
                EditorStyles.wordWrappedLabel);
            GUI.enabled = true;

            if (Application.isPlaying)
                EditorGUILayout.LabelField(
                    "[!] Remember: Changes will be saved when you exit playmode (since you are editing a Scriptable Object).",
                    EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();

            effectsDrawer.DrawBody();

            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(BuiltinAppearancesDataScriptable))]
    internal class BuiltinAppearancesDrawer : BuiltinDataScriptableDrawer
    {
        protected override TextAnimatorDrawer.BuiltinVariablesDrawer InitializeDrawer(SerializedProperty property)
        {
            return new TextAnimatorDrawer.AppearanceDefaultEffects(property);
        }
    }

    [CustomEditor(typeof(BuiltinBehaviorsDataScriptable))]
    internal class BuiltinBehaviorsDrawer : BuiltinDataScriptableDrawer
    {
        protected override TextAnimatorDrawer.BuiltinVariablesDrawer InitializeDrawer(SerializedProperty property)
        {
            return new TextAnimatorDrawer.BehaviorDefaultEffects(property);
        }
    }
}