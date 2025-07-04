namespace SoundSystem
{
    using System;

    /// <summary>
    /// ISoundCacheインスタンスを生成するファクトリークラス
    /// </summary>
    public static class SoundCacheFactory
    {
        public enum Strategy
        {
            LeastRecentlyUsed,
            TimeToLive,
            Random
        }
    
        /// <summary>
        /// キャッシュ方式とパラメータからISoundCacheを生成する
        /// </summary>
        public static ISoundCache Create(Strategy k, float param)
        {
            IEvictionStrategy strategy = k switch
            {
                Strategy.LeastRecentlyUsed => new EvictionStrategy_LRU(param),
                Strategy.TimeToLive        => new EvictionStrategy_TTL(param),
                Strategy.Random            => new EvictionStrategy_Random((int)param),
                _ => throw new ArgumentOutOfRangeException(nameof(k))
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
