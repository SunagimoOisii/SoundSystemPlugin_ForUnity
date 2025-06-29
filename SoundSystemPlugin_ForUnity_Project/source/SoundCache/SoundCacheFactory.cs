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
            LeastRecentlyUsed,
            TimeToLive,
            Random
        }
    
        /// <summary>
        /// キャッシュ方式とパラメータからISoundCacheを生成する
        /// </summary>
        public static ISoundCache Create(Type t, float param)
        {
            return t switch
            {
                Type.LeastRecentlyUsed    => CreateLRU(param),
                Type.TimeToLive           => CreateTTL(param),
                Type.Random               => CreateRandom((int)param),
                _           => throw new ArgumentOutOfRangeException(nameof(t))
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
