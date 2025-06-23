namespace SoundSystem
{
    using System;

    /// <summary>
    /// ISoundCacheインスタンスを生成するファクトリークラス
    /// </summary>
    public static class SoundCacheFactory
    {
        public enum Type
        {
            LRU,
            TTL,
            Random
        }
    
        /// <summary>
        /// キャッシュ方式とパラメータからISoundCacheを生成する
        /// </summary>
        public static ISoundCache Create(Type t, float param)
        {
            return t switch
            {
                Type.LRU    => CreateLRU(param),
                Type.TTL    => CreateTTL(param),
                Type.Random => CreateRandom((int)param),
                _           => throw new ArgumentOutOfRangeException(nameof(t))
            };
        }

        /// <summary>
        /// LRU方式のキャッシュを生成する
        /// </summary>
        public static ISoundCache CreateLRU(float idleTimeThreshold)
        {
            return new SoundCache_LRU(idleTimeThreshold);
        }

        /// <summary>
        /// TTL方式のキャッシュを生成する
        /// </summary>
        public static ISoundCache CreateTTL(float ttlSeconds)
        {
            return new SoundCache_TTL(ttlSeconds);
        }

        /// <summary>
        /// ランダム削除方式のキャッシュを生成する
        /// </summary>
        public static ISoundCache CreateRandom(int maxCacheCount)
        {
            return new SoundCache_Random(maxCacheCount);
        }
    }
}
