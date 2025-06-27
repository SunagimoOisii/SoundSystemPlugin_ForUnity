#if UNITY_EDITOR
namespace SoundSystem
{
    using UnityEditor;
    using UnityEngine;

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

        //検索
        private string searchText = string.Empty;

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

        private void DrawPresetList(SerializedProperty list, string label)
        {
            if (list == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                EditorGUILayout.PropertyField(list, new GUIContent(label), true);
            }
            else
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty element = list.GetArrayElementAtIndex(i);
                    var nameProp = element.FindPropertyRelative("presetName");
                    if (nameProp == null)
                    {
                        continue;
                    }
                    string name = nameProp.stringValue;
                    if (name.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    EditorGUILayout.PropertyField(element, true);
                }
            }
        }

        private void OnEnable()
        {
            //BGM
            bgmMixerG  = serializedObject.FindProperty("bgmMixerG");
            bgmPresets = serializedObject.FindProperty("bgmPresets");
            bgmPresetList = bgmPresets.FindPropertyRelative("presetList");

            //SE
            seMixerG  = serializedObject.FindProperty("seMixerG");
            sePresets = serializedObject.FindProperty("sePresets");
            sePresetList = sePresets.FindPropertyRelative("presetList");

            //SoundLoader設定
            loaderType = serializedObject.FindProperty("loaderType");

            //SoundCache設定
            cacheType          = serializedObject.FindProperty("cacheType");
            param              = serializedObject.FindProperty("param");
            enableAutoEvict    = serializedObject.FindProperty("enableAutoEvict");
            autoEvictInterval  = serializedObject.FindProperty("autoEvictInterval");

            //AudioSourcePool設定
            poolType = serializedObject.FindProperty("poolType");
            initSize = serializedObject.FindProperty("initSize");
            maxSize  = serializedObject.FindProperty("maxSize");
            persistentGameObjects = serializedObject.FindProperty("persistentGameObjects");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            searchText = EditorGUILayout.ToolbarSearchField(searchText);
            EditorGUILayout.Space();

            //BGM
            EditorGUILayout.PropertyField(bgmMixerG, true);
            DrawPresetList(bgmPresetList, "BGM Presets");

            //SE
            EditorGUILayout.PropertyField(seMixerG, true);
            DrawPresetList(sePresetList, "SE Presets");

            //SoundLoader設定
            EditorGUILayout.PropertyField(loaderType, true);

            //SoundCache設定
            EditorGUILayout.PropertyField(cacheType);
            var type = (SoundCacheFactory.Type)cacheType.enumValueIndex;
            switch (type)
            {
                case SoundCacheFactory.Type.LRU:
                    EditorGUILayout.PropertyField(param, new GUIContent("idleTimeThreshold"));
                    break;

                case SoundCacheFactory.Type.TTL:
                    EditorGUILayout.PropertyField(param, new GUIContent("ttlSeconds"));
                    break;

                case SoundCacheFactory.Type.Random:
                    int intVal = Mathf.RoundToInt(param.floatValue);
                    intVal = EditorGUILayout.IntField("maxCacheCount", intVal);
                    param.floatValue = intVal;
                    break;
            }

            EditorGUILayout.PropertyField(enableAutoEvict);
            EditorGUI.BeginDisabledGroup(enableAutoEvict.boolValue == false);
            EditorGUILayout.PropertyField(autoEvictInterval);
            EditorGUI.EndDisabledGroup();

            //AudioSourcePool設定
            EditorGUILayout.PropertyField(poolType, true);
            EditorGUILayout.PropertyField(initSize, true);
            EditorGUILayout.PropertyField(maxSize, true);
            EditorGUILayout.PropertyField(persistentGameObjects);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
