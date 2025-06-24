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

        //SE
        private SerializedProperty sePresets;
        private SerializedProperty seMixerG;

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

        private void OnEnable()
        {
            //BGM
            bgmMixerG  = serializedObject.FindProperty("bgmMixerG");
            bgmPresets = serializedObject.FindProperty("bgmPresets");

            //SE
            seMixerG  = serializedObject.FindProperty("seMixerG");
            sePresets = serializedObject.FindProperty("sePresets");

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

            //BGM
            EditorGUILayout.PropertyField(bgmMixerG, true);
            EditorGUILayout.PropertyField(bgmPresets, true);

            //SE
            EditorGUILayout.PropertyField(seMixerG, true);
            EditorGUILayout.PropertyField(sePresets, true);

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
