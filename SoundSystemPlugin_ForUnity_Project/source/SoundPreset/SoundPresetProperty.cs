namespace SoundSystem
{
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "SoundPresetProperty", menuName = "SoundSystem/SoundPresetProperty", order = 0)]
    public class SoundPresetProperty : ScriptableObject
    {
        [System.Serializable]
        public struct BGMPreset
        {
            public string presetName;
            [Range(0f, 1f)] public float volume;
            [Range(0f, 1f)] public float fadeInDuration;
            [Range(0f, 1f)] public float fadeOutDuration;
            [Range(0f, 1f)] public float crossFadeDuration;
        }
    
        [System.Serializable]
        public struct SEPreset
        {
            public string presetName;
            [Range(0f, 1f)] public float volume;
            [Range(0f, 1f)] public float pitch;
            [Range(0f, 1f)] public float spatialBlend; //0 = 2D, 1 = 3D
            public Vector3 position;
        }
    
        [Header("BGMݒ胊Xg")]
        public SerializedBGMSettingDictionary bgmPresets = new();
    
        [Header("SEݒ胊Xg")]
        public SerializedSESettingDictionary sePresets = new();
    
        [Header("SoundCacheݒ")]
        public SoundCacheFactory.SoundCacheType cacheType = SoundCacheFactory.SoundCacheType.LRU;
        public float param = 30f; //idleTimeThresholdttlSecondsɎg
    }
}
