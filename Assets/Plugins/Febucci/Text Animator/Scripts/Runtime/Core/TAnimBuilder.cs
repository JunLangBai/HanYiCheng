using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Febucci.UI.Core
{
#if UNITY_EDITOR
    public static class TAnim_EditorHelper
    {
        internal static event VoidCallback onChangesApplied;

        public static void TriggerEvent()
        {
            if (Application.isPlaying) onChangesApplied?.Invoke();
        }

        internal delegate void VoidCallback();
    }
#endif


    public static class TAnimBuilder
    {
        internal static TagFormatting tag_behaviors = new('<', '>');
        internal static TagFormatting tag_appearances = new('{', '}');

        private static bool hasData;

        internal static TAnimGlobalDataScriptable data { get; private set; }

        [Serializable]
        internal struct TagFormatting
        {
            public TagFormatting(char openingChar, char closingChar)
            {
                charOpeningTag = openingChar;
                charClosingTag = closingChar;
            }

            public char charOpeningTag;
            public char charClosingTag;
        }


        #region Static Controller

        private static Dictionary<string, Type> behaviorsData = new();
        private static Dictionary<string, Type> appearancesData = new();

        private static readonly HashSet<string> globalDefaultActions = new();
        private static readonly HashSet<string> globalCustomActions = new();

        private static bool globalDatabaseInitialized;

        public static string[] GetAllBehaviorsTags()
        {
            var tags = new List<string>();
            for (var i = 0; i < behaviorsData.Count; i++) tags.Add(behaviorsData.Keys.ElementAt(i));

            return tags.ToArray();
        }

        public static string[] GetAllApppearancesTags()
        {
            var tags = new List<string>();
            for (var i = 0; i < appearancesData.Count; i++) tags.Add(appearancesData.Keys.ElementAt(i));

            return tags.ToArray();
        }

        /// <summary>
        ///     Initializes and Load TextAnimator's effects and global settings, in case it has not been loaded already.
        /// </summary>
        public static void InitializeGlobalDatabase()
        {
            if (globalDatabaseInitialized)
                return;

            globalDatabaseInitialized = true;

            TextUtilities.Initialize();


            #region Local Methods

            void PopulateEffectsFromAssembly<T>(ref Dictionary<string, Type> effectsList) where T : EffectsBase
            {
                List<Type> GetAssemblyClasses()
                {
                    return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                        from assemblyType in domainAssembly.GetTypes()
                        where assemblyType.IsSubclassOf(typeof(T))
                        where !assemblyType.IsAbstract
                        select assemblyType).ToList();
                }

                var effectsInAssembly = GetAssemblyClasses();
                EffectInfoAttribute attribute;
                string effectTag;

                for (var i = 0; i < effectsInAssembly.Count; i++)
                {
                    effectTag = string.Empty;
                    attribute = effectsInAssembly[i].GetCustomAttribute<EffectInfoAttribute>();

                    if (attribute != null)
                    {
                        effectTag = attribute.tag;
                    }
                    else
                    {
                        Debug.LogError(
                            $"TextAnimator: skipping class {effectsInAssembly[i].Name}. Please add a 'EffectInfoAttribute' on top of it.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(effectTag)) continue;

                    if (!effectsList.ContainsKey(effectTag))
                        effectsList.Add(effectTag, effectsInAssembly[i]);
                    else
                        Debug.LogError(
                            $"TextAnimator: not adding effect <{effectTag}> (from class '{effectsInAssembly[i].Name}') to the database because an effect with the same tag has already been added (by class '{effectsList[effectTag].Name}')");
                }
            }

            #endregion

            PopulateEffectsFromAssembly<BehaviorBase>(ref behaviorsData);
            PopulateEffectsFromAssembly<AppearanceBase>(ref appearancesData);

            #region Default Actions

            globalDefaultActions.Add("waitfor");
            globalDefaultActions.Add("waitinput");
            globalDefaultActions.Add("speed");

            #endregion

            hasData = false;
            data = Resources.Load(TAnimGlobalDataScriptable.resourcesPath) as TAnimGlobalDataScriptable;

            if (data != null)
            {
                hasData = true;

                #region Settings

                if (data.customTagsFormatting)
                {
                    if (data.tagInfo_behaviors.charOpeningTag != data.tagInfo_appearances.charOpeningTag
                        && data.tagInfo_behaviors.charClosingTag != data.tagInfo_appearances.charClosingTag)
                    {
                        tag_behaviors = data.tagInfo_behaviors;
                        tag_appearances = data.tagInfo_appearances;
                    }
                    else
                    {
                        Debug.LogError("Not valid"); //todo error
                    }
                }

                #endregion

                #region Global Effects

                //Adds global effects
                for (var i = 0; i < data.globalBehaviorPresets.Length; i++)
                    TryAddingPresetToDictionary(ref behaviorsData, data.globalBehaviorPresets[i].effectTag,
                        typeof(PresetBehavior));

                //Adds global effects
                for (var i = 0; i < data.globalAppearancePresets.Length; i++)
                    TryAddingPresetToDictionary(ref appearancesData, data.globalAppearancePresets[i].effectTag,
                        typeof(PresetAppearance));

                #endregion

                #region Custom Actions

                if (data.customActions != null && data.customActions.Length > 0)
                    for (var i = 0; i < data.customActions.Length; i++)
                    {
                        if (data.customActions[i].Length <= 0)
                        {
                            Debug.LogError($"TextAnimator: Custom action {i} has an empty tag!");
                            continue;
                        }

                        if (globalCustomActions.Contains(data.customActions[i]))
                        {
                            Debug.LogError(
                                $"TextAnimator: Custom feature with tag '{data.customActions[i]}' is already present, it won't be added to the database.");
                            continue;
                        }

                        globalCustomActions.Add(data.customActions[i]);
                    }

                #endregion
            }
        }


        internal static bool TryGetGlobalPresetBehavior(string tag, out PresetBehaviorValues result)
        {
            if (!hasData) //avoids searching if data is null
            {
                result = default;
                return false;
            }

            return GetPresetFromArray(tag, data.globalBehaviorPresets, out result);
        }

        internal static bool TryGetGlobalPresetAppearance(string tag, out PresetAppearanceValues result)
        {
            if (!hasData) //avoids searching if data is null
            {
                result = default;
                return false;
            }

            return GetPresetFromArray(tag, data.globalAppearancePresets, out result);
        }

        internal static bool GetPresetFromArray<T>(string tag, T[] presets, out T result) where T : PresetBaseValues
        {
            if (presets.Length > 0)
                for (var i = 0; i < presets.Length; i++)
                    if (tag.Equals(presets[i].effectTag))
                    {
                        result = presets[i];
                        return true;
                    }

            result = default;
            return false;
        }


        internal static bool IsDefaultAction(string tag)
        {
            if (globalDefaultActions.Count > 0 && globalDefaultActions.Contains(tag)) return true;

            return false;
        }

        internal static bool IsCustomAction(string tag)
        {
            if (globalCustomActions.Count > 0 && globalCustomActions.Contains(tag)) return true;

            return false;
        }

        internal static bool TryGetGlobalBehaviorFromTag(string effectTag, string entireRichTextTag,
            out BehaviorBase effectClass)
        {
            return TryGetEffectClassFromTag(behaviorsData, effectTag, entireRichTextTag, out effectClass);
        }

        internal static bool TryGetGlobalAppearanceFromTag(string effectTag, string entireRichTextTag,
            out AppearanceBase effectClass)
        {
            return TryGetEffectClassFromTag(appearancesData, effectTag, entireRichTextTag, out effectClass);
        }

        internal static bool TryGetEffectClassFromTag<T>(Dictionary<string, Type> dictionary, string effectTag,
            string entireRichTextTag, out T effectClass) where T : EffectsBase
        {
            if (dictionary.ContainsKey(effectTag))
            {
                effectClass = Activator.CreateInstance(dictionary[effectTag]) as T;
                effectClass._Initialize(effectTag, entireRichTextTag);
                return true;
            }

            effectClass = default;
            return false;
        }

        internal static void TryAddingPresetToDictionary(ref Dictionary<string, Type> database, string tag, Type type)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogWarning($"TextAnimator: Preset has a null or empty tag '{tag}'");
                return;
            }

            if (!TextUtilities.IsTagLongEnough(tag))
            {
                Debug.LogWarning($"TextAnimator: Preset has tag '{tag}' shorter than three characters.");
                return;
            }

            if (database.ContainsKey(tag))
            {
                Debug.LogWarning(
                    $"TextAnimator: A Preset has tag '{tag}' that's already present, it won't be added to the database.");
                return;
            }

            database.Add(tag, type);
        }

        #endregion
    }
}