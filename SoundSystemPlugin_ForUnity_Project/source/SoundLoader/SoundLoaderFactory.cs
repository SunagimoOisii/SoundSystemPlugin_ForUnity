namespace SoundSystem
{
    using System;

    /// <summary>
    /// ISoundLoader インスタンスを生成するファクトリークラス
    /// </summary>
    public static class SoundLoaderFactory
    {
        public enum Kind
        {
#if USE_ADDRESSABLES
            Addressables,
#endif
            Resources,
            Streaming
        }

        public static ISoundLoader Create(Kind k, ISoundCache cache)
        {
            return k switch
            {
#if USE_ADDRESSABLES
                Kind.Addressables => new SoundLoader_Addressables(cache),
#endif
                Kind.Resources    => new SoundLoader_Resources(cache),
                Kind.Streaming    => new SoundLoader_Streaming(cache),
                _ => throw new ArgumentOutOfRangeException(nameof(k))
            };
        }
    }
}
