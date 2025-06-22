namespace SoundSystem
{

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
