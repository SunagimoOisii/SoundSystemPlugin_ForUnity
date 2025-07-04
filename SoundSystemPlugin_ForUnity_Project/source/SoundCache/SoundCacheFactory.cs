namespace SoundSystem
{
    using System;

    /// <summary>
    /// ISoundCacheインスタンスを生成するファクトリークラス
    /// </summary>
    public static class SoundCacheFactory
    {
        public enum Kind
        {
            LeastRecentlyUsed,
            TimeToLive,
            Random
        }
    
        /// <summary>
        /// キャッシュ方式とパラメータからISoundCacheを生成する
        /// </summary>
        public static ISoundCache Create(Kind k, float param)
        {
            IEvictionStrategy strategy = k switch
            {
                Kind.LeastRecentlyUsed => new EvictionStrategy_LRU(param),
                Kind.TimeToLive        => new EvictionStrategy_TTL(param),
                Kind.Random            => new EvictionStrategy_Random((int)param),
                _                      => throw new ArgumentOutOfRangeException(nameof(k))
            };
            return new SoundCache(strategy);
        }

        public static ISoundCache CreateLRU(float idleTimeThreshold)
        {
            return new SoundCache(new EvictionStrategy_LRU(idleTimeThreshold));
        }

        public static ISoundCache CreateTTL(float ttlSeconds)
        {
            return new SoundCache(new EvictionStrategy_TTL(ttlSeconds));
        }

        public static ISoundCache CreateRandom(int maxCacheCount)
        {
            return new SoundCache(new EvictionStrategy_Random(maxCacheCount));
        }
    }
}
