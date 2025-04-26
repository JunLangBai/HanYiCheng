using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace LeTai.Asset.TranslucentImage.Editor
{
    internal class ScenceGizmoAutoDisable : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!importedAssets.Any(p => p.Contains("TranslucentImage")))
                return;

            var structAnnotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            if (structAnnotation == null) return;

            var fieldClassId = structAnnotation.GetField("classID");
            var fieldScriptClass = structAnnotation.GetField("scriptClass");
            var fieldFlags = structAnnotation.GetField("flags");
            var fieldIconEnabled = structAnnotation.GetField("iconEnabled");

            var classAnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            if (classAnnotationUtility == null) return;

            var methodGetAnnotations =
                classAnnotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Static);
            if (methodGetAnnotations == null) return;
            var methodSetIconEnabled =
                classAnnotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Static);
            if (methodSetIconEnabled == null) return;

            var annotations = (Array)methodGetAnnotations.Invoke(null, null);
            foreach (var a in annotations)
            {
                var scriptClass = (string)fieldScriptClass.GetValue(a);

                // built in types
                if (string.IsNullOrEmpty(scriptClass)) continue;

                var classId = (int)fieldClassId.GetValue(a);
                var flags = (int)fieldFlags.GetValue(a);
                var iconEnabled = (int)fieldIconEnabled.GetValue(a);

                const int maskHasIcon = 1;
                var hasIconFlag = (flags & maskHasIcon) == maskHasIcon;

                if (hasIconFlag
                    && iconEnabled != 0
                    && scriptClass.Contains("TranslucentImage"))
                    methodSetIconEnabled.Invoke(null, new object[] { classId, scriptClass, 0 });
            }
        }
    }
}