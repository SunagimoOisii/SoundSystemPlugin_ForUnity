namespace SoundSystem
{
    using System;

    /// <summary>
    /// ISoundLoader の生成を担うファクトリー
    /// </summary>
    public static class SoundLoaderFactory
    {
        public enum LoaderType
        {
            Addressables,
            Resources
        }

        public static ISoundLoader Create(ISoundCache cache, LoaderType type = LoaderType.Addressables)
        {
            return type switch
            {
#if SOUND_USE_ADDRESSABLES
                LoaderType.Addressables => new SoundLoader_Addressables(cache),
#endif
                LoaderType.Resources    => new SoundLoader_Resources(cache),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
