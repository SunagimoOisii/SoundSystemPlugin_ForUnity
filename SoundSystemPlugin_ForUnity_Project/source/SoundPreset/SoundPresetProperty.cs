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
            [Range(0f, 1f)] public float fadeInDuration;
            [Range(0f, 1f)] public float fadeOutDuration;
        }

        [System.Serializable]
        public struct ListenerEffectPreset
        {
            public string                   presetName;
            public ListenerEffectFilterType filterType;
            public Behaviour               template;
        }

        [Header("BGM")]
        public AudioMixerGroup bgmMixerG;
        public SerializedBGMPresetDictionary bgmPresets = new();

        [Header("SE")]
        public AudioMixerGroup seMixerG;
        public SerializedSEPresetDictionary sePresets = new();

        [Header("Listener Effect Preset")]
        public SerializedListenerPresetDictionary listenerPresets = new();

        [Header("SoundLoader")]
#if USE_ADDRESSABLES
        public SoundLoaderFactory.Type loaderType = SoundLoaderFactory.Type.Addressables;
#else
        public SoundLoaderFactory.Type loaderType = SoundLoaderFactory.Type.Resources;
#endif

        [Header("SoundCache")]
        public SoundCacheFactory.Type cacheType = SoundCacheFactory.Type.LeastRecentlyUsed;
        public bool  enableAutoEvict   = false;
        public float autoEvictInterval = 60f;

        //CustomEditor により、選択キャッシュ方式に応じた変数名がインスペクターでは表示される
        //例: idleTimeThreshold, ttlSeconds
        public float param = 30f;

        [Header("AudioSourcePool")]
        public AudioSourcePoolFactory.Type poolType = AudioSourcePoolFactory.Type.FIFO;
        public int initSize = 5;
        public int maxSize  = 10;

        [Header("Persistence of generated GameObjects")]
        public bool isPersistentGameObjects = false;
    }
}
