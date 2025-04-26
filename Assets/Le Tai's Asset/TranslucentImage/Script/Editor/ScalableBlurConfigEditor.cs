using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace LeTai.Asset.TranslucentImage.Editor
{
    [CustomEditor(typeof(ScalableBlurConfig))]
    [CanEditMultipleObjects]
    public class ScalableBlurConfigEditor : UnityEditor.Editor
    {
        private readonly AnimBool useAdvancedControl = new(false);
        private EditorProperty iteration;

        private EditorProperty radius;
        private EditorProperty strength;

        private int tab, previousTab;

        public void Awake()
        {
            LoadTabSelection();
            useAdvancedControl.value = tab > 0;
        }

        public void OnEnable()
        {
            radius = new EditorProperty(serializedObject, nameof(ScalableBlurConfig.Radius));
            iteration = new EditorProperty(serializedObject, nameof(ScalableBlurConfig.Iteration));
            strength = new EditorProperty(serializedObject, nameof(ScalableBlurConfig.Strength));

            // Without this editor will not Repaint automatically when animating
            useAdvancedControl.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            Draw();
        }

        public void Draw()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                DrawTabBar();

                using (var changes = new EditorGUI.ChangeCheckScope())
                {
                    serializedObject.Update();
                    DrawTabsContent();
                    if (changes.changed) serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawTabBar()
        {
            using (var h = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                tab = GUILayout.Toolbar(
                    tab,
                    new[] { "Simple", "Advanced" },
                    GUILayout.MinWidth(0),
                    GUILayout.MaxWidth(EditorGUIUtility.pixelsPerPoint * 192)
                );

                GUILayout.FlexibleSpace();
            }

            if (tab != previousTab)
            {
                GUI.FocusControl(""); // Defocus
                SaveTabSelection();
                previousTab = tab;
            }

            useAdvancedControl.target = tab == 1;
        }

        private void DrawTabsContent()
        {
            if (EditorGUILayout.BeginFadeGroup(1 - useAdvancedControl.faded))
            {
                // EditorProperty dooesn't invoke getter. Not needed anywhere else.
                _ = ((ScalableBlurConfig)target).Strength;
                strength.Draw();
            }

            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(useAdvancedControl.faded))
            {
                radius.Draw();
                iteration.Draw();
            }

            EditorGUILayout.EndFadeGroup();
        }

        //Persist selected tab between sessions and instances
        private void SaveTabSelection()
        {
            EditorPrefs.SetInt("LETAI_TRANSLUCENTIMAGE_TIS_TAB", tab);
        }

        private void LoadTabSelection()
        {
            if (EditorPrefs.HasKey("LETAI_TRANSLUCENTIMAGE_TIS_TAB"))
                tab = EditorPrefs.GetInt("LETAI_TRANSLUCENTIMAGE_TIS_TAB");
        }
    }
}