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
        private Label              seSearchState;
        private ToolbarSearchField seSearchField;
        private ScrollView         seScrollView;

        //Listener Effect
        private SerializedProperty listenerPresetList;
        private Label              listenerSearchState;
        private ToolbarSearchField listenerSearchField;
        private ScrollView         listenerScrollView;

        //SoundLoader設定
        private SerializedProperty loaderKind;

        //SoundCache設定
        private SerializedProperty cacheStrategy;
        private PropertyField      cacheStrategyField;
        private SerializedProperty param;
        private VisualElement      paramContainer;
        private SerializedProperty enableAutoEvict;
        private PropertyField      enableAutoEvictField;
        private SerializedProperty autoEvictInterval;
        private PropertyField      autoEvictIntervalField;

        //AudioSourcePool設定
        private SerializedProperty poolKind;
        private SerializedProperty initSize;
        private SerializedProperty maxSize;

        private SerializedProperty persistentGameObjects;

        private SerializedProperty writingLog;

        private void OnEnable()
        {
            //BGM
            bgmMixerG      = serializedObject.FindProperty("bgmMixerG");
            bgmPresetList  = serializedObject.FindProperty("bgmPresets")
                                .FindPropertyRelative("presetList");

            //SE
            seMixerG      = serializedObject.FindProperty("seMixerG");
            sePresetList  = serializedObject.FindProperty("sePresets")
                                .FindPropertyRelative("presetList");

            //Listener Effect
            listenerPresetList = serializedObject.FindProperty("listenerPresets")
                                   .FindPropertyRelative("presetList");

            //SoundLoader設定
            loaderKind = serializedObject.FindProperty("loaderKind");

            //SoundCache設定
            cacheStrategy          = serializedObject.FindProperty("cacheStrategy");
            param              = serializedObject.FindProperty("param");
            enableAutoEvict    = serializedObject.FindProperty("enableAutoEvict");
            autoEvictInterval  = serializedObject.FindProperty("autoEvictInterval");

            //AudioSourcePool設定
            poolKind              = serializedObject.FindProperty("poolKind");
            initSize              = serializedObject.FindProperty("initSize");
            maxSize               = serializedObject.FindProperty("maxSize");

            persistentGameObjects = serializedObject.FindProperty("isPersistentGameObjects");

            writingLog = serializedObject.FindProperty("canWriteLog");
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

            //Listener Effect 要素作成
            listenerSearchField = new ToolbarSearchField();
            listenerSearchField.RegisterValueChangedCallback(_ => RefreshPresetViews());
            listenerSearchState = new Label("Searching");
            listenerSearchState.style.unityFontStyleAndWeight = FontStyle.Bold;
            listenerSearchState.style.display                 = DisplayStyle.None;
            listenerScrollView = new ScrollView();
            var listenerSearchContainer = new VisualElement();
            listenerSearchContainer.style.flexDirection = FlexDirection.Column;
            listenerSearchContainer.Add(listenerSearchField);
            listenerSearchContainer.Add(listenerSearchState);
            //要素作成
            root.Add(new Label("Listener Effect Presets"));
            root.Add(listenerSearchContainer);
            root.Add(listenerScrollView);

            //SoundLoader 要素作成, 登録
            root.Add(new PropertyField(loaderKind));

            //SoundCache 要素作成
            cacheStrategyField = new PropertyField(cacheStrategy);
            cacheStrategyField.RegisterValueChangeCallback(_ => UpdateParamField());
            paramContainer = new VisualElement();
            enableAutoEvictField = new PropertyField(enableAutoEvict);
            enableAutoEvictField.RegisterValueChangeCallback(_ => 
                autoEvictIntervalField?.SetEnabled(enableAutoEvict.boolValue));
            autoEvictIntervalField = new PropertyField(autoEvictInterval);
            //要素登録
            root.Add(cacheStrategyField);
            root.Add(paramContainer);
            root.Add(enableAutoEvictField);
            root.Add(autoEvictIntervalField);

            //AudioSourcePool 要素作成, 登録
            root.Add(new PropertyField(poolKind));
            root.Add(new PropertyField(initSize));
            root.Add(new PropertyField(maxSize));

            root.Add(new PropertyField(persistentGameObjects));

            root.Add(new PropertyField(writingLog));

            root.Bind(serializedObject);

            UpdateParamField();
            autoEvictIntervalField?.SetEnabled(enableAutoEvict.boolValue);
            RefreshPresetViews();

            return root;
        }

        private void UpdateParamField()
        {
            paramContainer.Clear();
            switch ((SoundCacheFactory.Strategy)cacheStrategy.enumValueIndex)
            {
                case SoundCacheFactory.Strategy.None:
                    return;

                case SoundCacheFactory.Strategy.LeastRecentlyUsed:
                    var fieldLRU = new PropertyField(param, "idleTimeThreshold");
                    fieldLRU.Bind(serializedObject);
                    paramContainer.Add(fieldLRU);
                    break;

                case SoundCacheFactory.Strategy.TimeToLive:
                    var fieldTTL = new PropertyField(param, "ttlSeconds");
                    fieldTTL.Bind(serializedObject);
                    paramContainer.Add(fieldTTL);
                    break;

                case SoundCacheFactory.Strategy.Random:
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
            string listenerFilter = listenerSearchField?.value ?? string.Empty;

            FillPresetScroll(bgmScrollView, bgmPresetList, "BGM Preset", bgmFilter);
            FillPresetScroll(seScrollView, sePresetList, "SE Preset", seFilter);
            FillPresetScroll(listenerScrollView, listenerPresetList, "Listener Preset", listenerFilter);

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

            if (string.IsNullOrEmpty(listenerFilter) == false)
            {
                listenerSearchState.style.display = DisplayStyle.Flex;
            }
            else listenerSearchState.style.display = DisplayStyle.None;
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
