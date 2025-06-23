namespace SoundSystem
{
    using UnityEngine;
    using UnityEngine.Audio;

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

        [Header("BGM")]
        public AudioMixerGroup bgmMixerG;
        public SerializedBGMSettingDictionary bgmPresets = new();

        [Header("SE")]
        public AudioMixer seMixerG;
        public SerializedSESettingDictionary sePresets = new();

        [Header("SoundLoader設定")]
        public SoundLoaderFactory.Type loaderType = SoundLoaderFactory.Type.Addressables;

        [Header("SoundCache設定")]
        public SoundCacheFactory.Type cacheType = SoundCacheFactory.Type.LRU;

        //CustomEditor により、選択キャッシュ方式に応じた変数名がインスペクターでは表示される
        //例: idleTimeThreshold, ttlSeconds
        public float param = 30f;

        [Header("AudioSourcePool設定")]
        public AudioSourcePoolFactory.Type poolType = AudioSourcePoolFactory.Type.FIFO;
        public AudioMixerGroup seMixer;
        public int initSize = 5;
        public int maxSize  = 10;
    }
}
