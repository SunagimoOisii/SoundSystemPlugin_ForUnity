#if UNITY_EDITOR
namespace SoundSystem
{
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(SoundPresetProperty))]
    internal sealed class SoundPresetPropertyEditor : Editor
    {
        //BGM
        private SerializedProperty bgmPresets;
        private SerializedProperty bgmMixerG;
        private SerializedProperty bgmPresetList;

        //SE
        private SerializedProperty sePresets;
        private SerializedProperty seMixerG;
        private SerializedProperty sePresetList;

        //検索欄
        private ToolbarSearchField searchField;

        //SoundLoader設定
        private SerializedProperty loaderType;

        //SoundCache設定
        private SerializedProperty cacheType;
        private SerializedProperty param;
        private SerializedProperty enableAutoEvict;
        private SerializedProperty autoEvictInterval;

        //AudioSourcePool設定
        private SerializedProperty poolType;
        private SerializedProperty initSize;
        private SerializedProperty maxSize;
        private SerializedProperty persistentGameObjects;

        private void BuildPresetList(SerializedProperty list, VisualElement container, string label)
        {
            container.Clear();

            if (list == null) return;

            var title = new Label(label)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
            container.Add(title);

            string filter = searchField?.value ?? string.Empty;
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = element.FindPropertyRelative("presetName");
                if (nameProp != null && 
                    string.IsNullOrEmpty(filter) == false)
                {
                    string name = nameProp.stringValue;
                    if (name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }
                }

                var field = new PropertyField(element);
                field.BindProperty(element);
                container.Add(field);
            }
        }

        private void OnEnable()
        {
            //BGM
            bgmMixerG     = serializedObject.FindProperty("bgmMixerG");
            bgmPresets    = serializedObject.FindProperty("bgmPresets");
            bgmPresetList = bgmPresets.FindPropertyRelative("presetList");

            //SE
            seMixerG     = serializedObject.FindProperty("seMixerG");
            sePresets    = serializedObject.FindProperty("sePresets");
            sePresetList = sePresets.FindPropertyRelative("presetList");

            //SoundLoader設定
            loaderType = serializedObject.FindProperty("loaderType");

            //SoundCache設定
            cacheType          = serializedObject.FindProperty("cacheType");
            param              = serializedObject.FindProperty("param");
            enableAutoEvict    = serializedObject.FindProperty("enableAutoEvict");
            autoEvictInterval  = serializedObject.FindProperty("autoEvictInterval");

            //AudioSourcePool設定
            poolType              = serializedObject.FindProperty("poolType");
            initSize              = serializedObject.FindProperty("initSize");
            maxSize               = serializedObject.FindProperty("maxSize");
            persistentGameObjects = serializedObject.FindProperty("persistentGameObjects");
        }

        public override VisualElement CreateInspectorGUI()
        {
            searchField                = new ToolbarSearchField();
            var bgmContainer           = new VisualElement();
            var seContainer            = new VisualElement();
            var loaderField            = new PropertyField(loaderType);
            var cacheField             = new PropertyField(cacheType);
            var paramField             = new PropertyField(param);
            var autoEvictField         = new PropertyField(enableAutoEvict);
            var autoEvictIntervalField = new PropertyField(autoEvictInterval);
            var poolTypeField          = new PropertyField(poolType);
            var initSizeField          = new PropertyField(initSize);
            var maxSizeField           = new PropertyField(maxSize);
            var persistentField        = new PropertyField(persistentGameObjects);

            var root = new VisualElement();
            root.Add(searchField);
            root.Add(new PropertyField(bgmMixerG));
            root.Add(bgmContainer);
            root.Add(new PropertyField(seMixerG));
            root.Add(seContainer);
            root.Add(loaderField);
            root.Add(cacheField);
            root.Add(paramField);
            root.Add(autoEvictField);
            root.Add(autoEvictIntervalField);
            root.Add(poolTypeField);
            root.Add(initSizeField);
            root.Add(maxSizeField);
            root.Add(persistentField);

            root.Bind(serializedObject);

            void Refresh()
            {
                BuildPresetList(bgmPresetList, bgmContainer, "BGM Presets");
                BuildPresetList(sePresetList, seContainer, "SE Presets");

                var type = (SoundCacheFactory.Type)cacheType.enumValueIndex;
                switch (type)
                {
                    case SoundCacheFactory.Type.LRU:
                        paramField.label = "idleTimeThreshold";
                        paramField.style.display = DisplayStyle.Flex;
                        break;

                    case SoundCacheFactory.Type.TTL:
                        paramField.label = "ttlSeconds";
                        paramField.style.display = DisplayStyle.Flex;
                        break;

                    case SoundCacheFactory.Type.Random:
                        paramField.label = "maxCacheCount";
                        paramField.style.display = DisplayStyle.Flex;
                        break;

                    default:
                        paramField.style.display = DisplayStyle.None;
                        break;
                }

                autoEvictIntervalField.SetEnabled(enableAutoEvict.boolValue);
            }

            searchField.RegisterValueChangedCallback(_ => Refresh());
            cacheField.RegisterValueChangeCallback(_ => Refresh());
            autoEvictField.RegisterValueChangeCallback(_ => Refresh());

            Refresh();
            return root;
        }
    }
}
#endif
