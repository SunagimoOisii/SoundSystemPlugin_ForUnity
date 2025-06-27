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
        private SerializedProperty bgmMixerG;
        private ToolbarSearchField bgmSearchField;
        private Label              bgmSearchState;
        private SerializedProperty bgmPresetList;
        private ScrollView         bgmScrollView;

        //SE
        private SerializedProperty seMixerG;
        private SerializedProperty sePresetList;
        private Label seSearchState;
        private ToolbarSearchField seSearchField;
        private ScrollView seScrollView;

        //SoundLoader設定
        private SerializedProperty loaderType;

        //SoundCache設定
        private SerializedProperty cacheType;
        private PropertyField      cacheTypeField;
        private SerializedProperty param;
        private VisualElement      paramContainer;
        private SerializedProperty enableAutoEvict;
        private PropertyField      enableAutoEvictField;
        private SerializedProperty autoEvictInterval;
        private PropertyField      autoEvictIntervalField;

        //AudioSourcePool設定
        private SerializedProperty poolType;
        private SerializedProperty initSize;
        private SerializedProperty maxSize;
        private SerializedProperty persistentGameObjects;        

        private void OnEnable()
        {
            //BGM
            bgmMixerG      = serializedObject.FindProperty("bgmMixerG");
            var bgmPresets = serializedObject.FindProperty("bgmPresets");
            bgmPresetList  = bgmPresets.FindPropertyRelative("presetList");

            //SE
            seMixerG      = serializedObject.FindProperty("seMixerG");
            var sePresets = serializedObject.FindProperty("sePresets");
            sePresetList  = sePresets.FindPropertyRelative("presetList");

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
            persistentGameObjects = serializedObject.FindProperty("isPersistentGameObjects");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            //BGM 要素作成
            bgmSearchField = new ToolbarSearchField();
            bgmSearchField.RegisterValueChangedCallback(_ => RefreshPresetViews());
            bgmSearchState = new Label("Searching");
            bgmSearchState.style.unityFontStyleAndWeight = FontStyle.Bold;
            bgmSearchState.style.display                 = DisplayStyle.None;
            bgmScrollView = new ScrollView();
            var bgmSearchContainer = new VisualElement();
            bgmSearchContainer.style.flexDirection = FlexDirection.Column;
            bgmSearchContainer.Add(bgmSearchField);
            bgmSearchContainer.Add(bgmSearchState);
            //要素登録
            root.Add(new PropertyField(bgmMixerG));
            root.Add(new Label("BGM Presets"));
            root.Add(bgmSearchContainer);
            root.Add(bgmScrollView);

            //SE 要素作成
            seSearchField = new ToolbarSearchField();
            seSearchField.RegisterValueChangedCallback(_ => RefreshPresetViews());
            seSearchState = new Label("Searching");
            seSearchState.style.unityFontStyleAndWeight = FontStyle.Bold;
            seSearchState.style.display                 = DisplayStyle.None;
            seScrollView = new ScrollView();
            var seSearchContainer = new VisualElement();
            seSearchContainer.style.flexDirection = FlexDirection.Column;
            seSearchContainer.Add(seSearchField);
            seSearchContainer.Add(seSearchState);
            //要素作成
            root.Add(new PropertyField(seMixerG));
            root.Add(new Label("SE Presets"));
            root.Add(seSearchContainer);
            root.Add(seScrollView);

            //SoundLoader 要素作成, 登録
            root.Add(new PropertyField(loaderType));

            //SoundCache 要素作成
            cacheTypeField = new PropertyField(cacheType);
            cacheTypeField.RegisterValueChangeCallback(_ => UpdateParamField());
            paramContainer = new VisualElement();
            enableAutoEvictField = new PropertyField(enableAutoEvict);
            enableAutoEvictField.RegisterValueChangeCallback(_ => 
                autoEvictIntervalField?.SetEnabled(enableAutoEvict.boolValue));
            autoEvictIntervalField = new PropertyField(autoEvictInterval);
            //要素登録
            root.Add(cacheTypeField);
            root.Add(paramContainer);
            root.Add(enableAutoEvictField);
            root.Add(autoEvictIntervalField);

            //AudioSourcePool 要素作成, 登録
            root.Add(new PropertyField(poolType));
            root.Add(new PropertyField(initSize));
            root.Add(new PropertyField(maxSize));
            root.Add(new PropertyField(persistentGameObjects));

            root.Bind(serializedObject);

            UpdateParamField();
            autoEvictIntervalField?.SetEnabled(enableAutoEvict.boolValue);
            RefreshPresetViews();

            return root;
        }

        private void UpdateParamField()
        {
            paramContainer.Clear();
            switch ((SoundCacheFactory.Type)cacheType.enumValueIndex)
            {
                case SoundCacheFactory.Type.LRU:
                    var fieldLRU = new PropertyField(param, "idleTimeThreshold");
                    fieldLRU.Bind(serializedObject);
                    paramContainer.Add(fieldLRU);
                    break;

                case SoundCacheFactory.Type.TTL:
                    var fieldTTL = new PropertyField(param, "ttlSeconds");
                    fieldTTL.Bind(serializedObject);
                    paramContainer.Add(fieldTTL);
                    break;

                case SoundCacheFactory.Type.Random:
                    var intField = new IntegerField("maxCacheCount") { value = Mathf.RoundToInt(param.floatValue) };
                    intField.RegisterValueChangedCallback(e => param.floatValue = e.newValue);
                    paramContainer.Add(intField);
                    break;
            }
        }

        private void RefreshPresetViews()
        {
            string bgmFilter = bgmSearchField?.value ?? string.Empty;
            string seFilter  = seSearchField?.value ?? string.Empty;

            FillPresetScroll(bgmScrollView, bgmPresetList, "BGM Preset", bgmFilter);
            FillPresetScroll(seScrollView, sePresetList, "SE Preset", seFilter);

            //検索バーへの入力があれば SearchState ラベルを表示する
            if (string.IsNullOrEmpty(bgmFilter) == false)
            {
                bgmSearchState.style.display = DisplayStyle.Flex;
            }
            else bgmSearchState.style.display = DisplayStyle.None;

            if (string.IsNullOrEmpty(seFilter) == false)
            {
                seSearchState.style.display = DisplayStyle.Flex;
            }
            else seSearchState.style.display = DisplayStyle.None;
        }

        private void FillPresetScroll(ScrollView target, SerializedProperty list,
            string label, string filter)
        {
            if (target == null ||
                list   == null) return;

            target.Clear();
            for (int i = 0; i < list.arraySize; i++)
            {
                int index = i;
                var element  = list.GetArrayElementAtIndex(index);
                var nameProp = element.FindPropertyRelative("presetName");
                if (string.IsNullOrEmpty(filter) == false &&
                    nameProp != null)
                {
                    string name = nameProp.stringValue;
                    if (name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }
                }

                var field = new PropertyField(element);
                field.style.flexGrow = 1;
                field.Bind(serializedObject);

                var container = new VisualElement();
                container.style.flexDirection = FlexDirection.Row;
                container.Add(field);

                var removeButton = new Button(() =>
                {
                    list.DeleteArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                    RefreshPresetViews();
                })
                {
                    text = "Delete"
                };
                container.Add(removeButton);

                target.Add(container);
            }

            if (string.IsNullOrEmpty(filter) == false) return;
            var addButton = new Button(() =>
            {
                list.InsertArrayElementAtIndex(list.arraySize);
                serializedObject.ApplyModifiedProperties();
                RefreshPresetViews();
            });
            addButton.text = $"Add {label}";
            target.Add(addButton);
        }
    }
}
#endif
