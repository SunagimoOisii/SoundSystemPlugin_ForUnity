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
                Kind.LeastRecentlyUsed => new LRUEvictionStrategy(param),
                Kind.TimeToLive        => new TTLEvictionStrategy(param),
                Kind.Random            => new RandomEvictionStrategy((int)param),
                _                      => throw new ArgumentOutOfRangeException(nameof(k))
            };
            return new SoundCache_Base(strategy);
        }

        public static ISoundCache CreateLRU(float idleTimeThreshold)
        {
            return new SoundCache_Base(new LRUEvictionStrategy(idleTimeThreshold));
        }

        public static ISoundCache CreateTTL(float ttlSeconds)
        {
            return new SoundCache_Base(new TTLEvictionStrategy(ttlSeconds));
        }

        public static ISoundCache CreateRandom(int maxCacheCount)
        {
            return new SoundCache_Base(new RandomEvictionStrategy(maxCacheCount));
        }
    }
}
