#if UNITY_EDITOR
namespace SoundSystem
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SoundPresetProperty))]
    internal sealed class SoundPresetPropertyEditor : Editor
    {
        private SerializedProperty bgmPresetsProp;
        private SerializedProperty sePresetsProp;
        private SerializedProperty cacheTypeProp;
        private SerializedProperty paramProp;

        private void OnEnable()
        {
            bgmPresetsProp = serializedObject.FindProperty("bgmPresets");
            sePresetsProp  = serializedObject.FindProperty("sePresets");
            cacheTypeProp  = serializedObject.FindProperty("cacheType");
            paramProp      = serializedObject.FindProperty("param");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(bgmPresetsProp, true);
            EditorGUILayout.PropertyField(sePresetsProp, true);
            EditorGUILayout.PropertyField(cacheTypeProp);

            var type = (SoundCacheFactory.Type)cacheTypeProp.enumValueIndex;
            switch (type)
            {
                case SoundCacheFactory.Type.LRU:
                    EditorGUILayout.PropertyField(paramProp, new GUIContent("idleTimeThreshold"));
                    break;

                case SoundCacheFactory.Type.TTL:
                    EditorGUILayout.PropertyField(paramProp, new GUIContent("ttlSeconds"));
                    break;

                case SoundCacheFactory.Type.Random:
                    int intVal = Mathf.RoundToInt(paramProp.floatValue);
                    intVal = EditorGUILayout.IntField("maxCacheCount", intVal);
                    paramProp.floatValue = intVal;
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
