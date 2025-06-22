namespace SoundSystem
{
    using System;
    
    /// <summary>
    /// ISoundCacheCX^X𐶐t@Ng[NX
    /// </summary>
    public static class SoundCacheFactory
    {
        public enum SoundCacheType
        {
            LRU,
            TTL,
            Random
        }
    
        /// <summary>
        /// wLbVɉISoundCacheCX^X𐶐
        /// </summary>
        /// <param name="param">ɉp[^(b܂͍ő吔)</param>
        public static ISoundCache Create(float param,
            SoundCacheType type = SoundCacheType.LRU)
        {
            return type switch
            {
                SoundCacheType.LRU    => new SoundCache_LRU(idleTimeThreshold: param),
                SoundCacheType.TTL    => new SoundCache_TTL(ttlSeconds: param),
                SoundCacheType.Random => new SoundCache_Random(maxCacheCount: (int)param),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }
}
