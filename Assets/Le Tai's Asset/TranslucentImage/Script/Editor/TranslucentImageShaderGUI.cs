using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LeTai.Asset.TranslucentImage.Editor
{
    public class TranslucentImageShaderGUI : ShaderGUI
    {
        private static readonly GUIContent LABEL = new();

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            DrawProperties(materialEditor, properties, false);
        }

        public static void DrawProperties(MaterialEditor materialEditor, MaterialProperty[] properties,
            bool skipUnimportants)
        {
            materialEditor.SetDefaultGUIWidths();

            for (var i = skipUnimportants ? 1 : 0; i < properties.Length; i++)
            {
                var prop = properties[i];

                if (skipUnimportants && prop.name == "_StencilComp")
                    break;

                if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0)
                    continue;

                switch (prop.name)
                {
                    default:
                        var h = materialEditor.GetPropertyHeight(prop, prop.displayName);
                        var r = EditorGUILayout.GetControlRect(true, h);
                        materialEditor.ShaderProperty(r, prop, GetPropGUIContent(prop));
                        break;
                }
            }

            EditorGUILayout.Space();

            if (!skipUnimportants)
            {
                EditorGUILayout.Space();
                if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
                    materialEditor.RenderQueueField();
                materialEditor.EnableInstancingField();
                materialEditor.DoubleSidedGIField();
            }
        }

        private static GUIContent GetPropGUIContent(MaterialProperty prop)
        {
            switch (prop.name)
            {
                case "_Vibrancy":
                    LABEL.tooltip =
                        "(De)Saturate the image, 1 is normal, 0 is black and white, below zero make the image negative";
                    break;
                case "_Brightness":
                    LABEL.tooltip = "Brighten/darken the image";
                    break;
                case "_Flatten":
                    LABEL.tooltip = "Flatten the color behind to help keep contrast on varying background";
                    break;
                default:
                    LABEL.tooltip = "";
                    break;
            }

            LABEL.text = prop.displayName;

            return LABEL;
        }
    }
}