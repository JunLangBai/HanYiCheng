using UnityEditor;
using UnityEngine;

namespace Febucci.UI.Core.Editors
{
    [CustomEditor(typeof(TAnimPlayerBase), true)]
    internal class TAnimPlayerBaseDrawer : Editor
    {
        private SerializedProperty canSkipTypewriter;
        private SerializedProperty disappearanceOrientation;
        private SerializedProperty hideAppearancesOnSkip;
        private SerializedProperty onCharacterVisible;
        private SerializedProperty onTextDisappeared;

        private SerializedProperty onTextShowed;
        private SerializedProperty onTypewriterStart;

        private string[] propertiesToExclude = new string[0];

        private SerializedProperty resetTypingSpeedAtStartup;
        private SerializedProperty showLettersDinamically;
        private SerializedProperty startTypewriterMode;
        private SerializedProperty triggerEventsOnSkip;

        protected virtual void OnEnable()
        {
            showLettersDinamically = serializedObject.FindProperty("useTypeWriter");
            startTypewriterMode = serializedObject.FindProperty("startTypewriterMode");
            canSkipTypewriter = serializedObject.FindProperty("canSkipTypewriter");
            hideAppearancesOnSkip = serializedObject.FindProperty("hideAppearancesOnSkip");
            triggerEventsOnSkip = serializedObject.FindProperty("triggerEventsOnSkip");
            disappearanceOrientation = serializedObject.FindProperty("disappearanceOrientation");


            onTextShowed = serializedObject.FindProperty("onTextShowed");
            onTypewriterStart = serializedObject.FindProperty("onTypewriterStart");
            onCharacterVisible = serializedObject.FindProperty("onCharacterVisible");
            onTextDisappeared = serializedObject.FindProperty("onTextDisappeared");

            resetTypingSpeedAtStartup = serializedObject.FindProperty("resetTypingSpeedAtStartup");

            propertiesToExclude = GetPropertiesToExclude();
        }

        protected virtual string[] GetPropertiesToExclude()
        {
            return new[]
            {
                "m_Script",
                "useTypeWriter",
                "startTypewriterMode",
                "canSkipTypewriter",
                "hideAppearancesOnSkip",
                "triggerEventsOnSkip",
                "onTextShowed",
                "onTypewriterStart",
                "onCharacterVisible",
                "resetTypingSpeedAtStartup",
                "onTextDisappeared",
                "disappearanceOrientation"
            };
        }

        private bool ButtonPlaymode(string label)
        {
            var prevGUI = GUI.enabled;
            GUI.enabled = Application.isPlaying;

            var value = GUILayout.Button(label, EditorStyles.miniButton, GUILayout.MaxWidth(70));

            GUI.enabled = prevGUI;
            return value;
        }

        public override void OnInspectorGUI()
        {
            {
                EditorGUILayout.LabelField("Main Settings", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(showLettersDinamically);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            //Typewriter settings

            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Typewriter", EditorStyles.boldLabel);

                if (showLettersDinamically.boolValue)
                {
                    if (ButtonPlaymode("Start")) ((TAnimPlayerBase)target).StartShowingText();
                    if (ButtonPlaymode("Stop")) ((TAnimPlayerBase)target).StopShowingText();
                }

                EditorGUILayout.EndHorizontal();
            }

            if (showLettersDinamically.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(startTypewriterMode);

                EditorGUILayout.PropertyField(resetTypingSpeedAtStartup);

                EditorGUILayout.LabelField("Typewriter Skip", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(canSkipTypewriter);

                if (canSkipTypewriter.boolValue && ButtonPlaymode("Skip")) ((TAnimPlayerBase)target).SkipTypewriter();
                EditorGUILayout.EndHorizontal();


                GUI.enabled = canSkipTypewriter.boolValue;
                EditorGUILayout.PropertyField(hideAppearancesOnSkip);
                EditorGUILayout.PropertyField(triggerEventsOnSkip);
                GUI.enabled = true;

                EditorGUI.indentLevel--;
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField("The typewriter is disabled");
                GUI.enabled = true;
            }

            EditorGUILayout.Space();

            //Events
            {
                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

                // foldoutEvents = EditorGUILayout.Foldout(foldoutEvents, "Events");

                //if (foldoutEvents)
                {
                    EditorGUILayout.PropertyField(onTextShowed);
                    EditorGUILayout.PropertyField(onTextDisappeared);

                    //GUI.enabled = showLettersDinamically.boolValue;

                    if (showLettersDinamically.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(onTypewriterStart);
                        EditorGUILayout.PropertyField(onCharacterVisible);

                        EditorGUI.indentLevel--;
                    }

                    //GUI.enabled = true;
                }
            }

            EditorGUILayout.Space();

            //Typewriter
            {
                EditorGUILayout.LabelField("Typewriter Wait", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                OnTypewriterSectionGUI();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            //Disappearance
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Disappearances", EditorStyles.boldLabel);

                if (ButtonPlaymode("Start")) ((TAnimPlayerBase)target).StartDisappearingText();
                if (ButtonPlaymode("Stop")) ((TAnimPlayerBase)target).StopDisappearingText();

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                GUI.enabled = false;
                EditorGUILayout.LabelField(
                    "To start disappearances, please call the 'StartDisappearingText()' method. See the docs for more.",
                    EditorStyles.wordWrappedMiniLabel);
                GUI.enabled = true;

                EditorGUILayout.PropertyField(disappearanceOrientation);

                OnDisappearanceSectionGUI();

                EditorGUI.indentLevel--;
            }

            //Draws parent without the children (so, TanimPlayerBase can have a custom inspector)
            DrawPropertiesExcluding(serializedObject, propertiesToExclude);


            if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnTypewriterSectionGUI()
        {
        }

        protected virtual void OnDisappearanceSectionGUI()
        {
        }
    }


    [CustomEditor(typeof(TextAnimatorPlayer), true)]
    internal class TAnimPlayerDrawer : TAnimPlayerBaseDrawer
    {
        private SerializedProperty avoidMultiplePunctuactionWait;
        private PropertyWithDifferentLabel disappearanceSpeedMultiplier;
        private PropertyWithDifferentLabel disappearanceWaitTime;

        private PropertyWithDifferentLabel useTypewriterWaitForDisappearances;
        private SerializedProperty waitForLastCharacter;
        private SerializedProperty waitForNewLines;
        private SerializedProperty waitForNormalChars;
        private SerializedProperty waitLong;
        private SerializedProperty waitMiddle;

        protected override void OnEnable()
        {
            base.OnEnable();

            waitForNormalChars = serializedObject.FindProperty("waitForNormalChars");
            waitLong = serializedObject.FindProperty("waitLong");
            waitMiddle = serializedObject.FindProperty("waitMiddle");
            avoidMultiplePunctuactionWait = serializedObject.FindProperty("avoidMultiplePunctuactionWait");
            waitForNewLines = serializedObject.FindProperty("waitForNewLines");
            waitForLastCharacter = serializedObject.FindProperty("waitForLastCharacter");
            useTypewriterWaitForDisappearances = new PropertyWithDifferentLabel(serializedObject,
                "useTypewriterWaitForDisappearances", "Use Typewriter Wait Times");
            disappearanceSpeedMultiplier = new PropertyWithDifferentLabel(serializedObject,
                "disappearanceSpeedMultiplier", "Typewriter Speed Multiplier");
            disappearanceWaitTime =
                new PropertyWithDifferentLabel(serializedObject, "disappearanceWaitTime", "Disappearances Wait");
        }

        protected override string[] GetPropertiesToExclude()
        {
            var newProperties = new[]
            {
                "script",
                "waitForNormalChars",
                "waitLong",
                "waitMiddle",
                "avoidMultiplePunctuactionWait",
                "waitForNewLines",
                "waitForLastCharacter",
                "useTypewriterWaitForDisappearances",
                "disappearanceSpeedMultiplier",
                "disappearanceWaitTime"
            };

            var baseProperties = base.GetPropertiesToExclude();

            var mergedArray = new string[newProperties.Length + baseProperties.Length];

            for (var i = 0; i < baseProperties.Length; i++) mergedArray[i] = baseProperties[i];

            for (var i = 0; i < newProperties.Length; i++) mergedArray[i + baseProperties.Length] = newProperties[i];

            return mergedArray;
        }

        protected override void OnTypewriterSectionGUI()
        {
            EditorGUILayout.PropertyField(waitForNormalChars);
            EditorGUILayout.PropertyField(waitLong);
            EditorGUILayout.PropertyField(waitMiddle);

            EditorGUILayout.PropertyField(avoidMultiplePunctuactionWait);
            EditorGUILayout.PropertyField(waitForNewLines);
            EditorGUILayout.PropertyField(waitForLastCharacter);
        }

        protected override void OnDisappearanceSectionGUI()
        {
            useTypewriterWaitForDisappearances.PropertyField();

            if (useTypewriterWaitForDisappearances.property.boolValue)
                disappearanceSpeedMultiplier.PropertyField();
            else
                disappearanceWaitTime.PropertyField();
        }

        private struct PropertyWithDifferentLabel
        {
            public readonly SerializedProperty property;
            public readonly GUIContent label;

            public PropertyWithDifferentLabel(SerializedObject obj, string property, string label)
            {
                this.property = obj.FindProperty(property);
                this.label = new GUIContent(label);
            }

            public void PropertyField()
            {
                EditorGUILayout.PropertyField(property, label);
            }
        }
    }
}