namespace SoundSystem
{
    using UnityEngine;
    
    public class SoundPresetProperty : MonoBehaviour
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
    
        [Header("BGM設定リスト")]
        public SerializedBGMSettingDictionary bgmPresets = new();
    
        [Header("SE設定リスト")]
        public SerializedSESettingDictionary sePresets = new();
    
        [Header("SoundCache設定")]
        public SoundCacheFactory.Type cacheType = SoundCacheFactory.Type.LRU;

        //CustomEditor により、選択キャッシュ方式に応じた変数名がインスペクターでは表示される
        //例: idleTimeThreshold, ttlSeconds
        public float param = 30f;
    }
}
