namespace SoundSystem
{
    using System;

    /// <summary>
    /// ISoundLoader インスタンスを生成するファクトリークラス
    /// </summary>
    public static class SoundLoaderFactory
    {
        public enum Type
        {
            Addressables,
            Resources
        }

        public static ISoundLoader Create(ISoundCache cache, Type type = Type.Addressables)
        {
            return type switch
            {
#if USE_ADDRESSABLES
                LoaderType.Addressables => new SoundLoader_Addressables(cache),
#endif
                Type.Resources    => new SoundLoader_Resources(cache),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
