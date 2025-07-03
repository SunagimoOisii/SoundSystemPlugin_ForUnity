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
            return k switch
            {
                Kind.LeastRecentlyUsed => CreateLRU(param),
                Kind.TimeToLive        => CreateTTL(param),
                Kind.Random            => CreateRandom((int)param),
                _                      => throw new ArgumentOutOfRangeException(nameof(k))
            };
        }

        public static ISoundCache CreateLRU(float idleTimeThreshold)
        {
            return new SoundCache_LRU(idleTimeThreshold);
        }

        public static ISoundCache CreateTTL(float ttlSeconds)
        {
            return new SoundCache_TTL(ttlSeconds);
        }

        public static ISoundCache CreateRandom(int maxCacheCount)
        {
            return new SoundCache_Random(maxCacheCount);
        }
    }
}
