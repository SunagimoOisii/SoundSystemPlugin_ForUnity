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
        /// 指定キャッシュ方式に応じたISoundCacheインスタンスを生成する
        /// </summary>
        /// <param name="param">方式に応じたパラメータ(秒数または最大数)</param>
        public static ISoundCache Create(float param,
            Type type = Type.LRU)
        {
            return type switch
            {
                Type.LRU    => new SoundCache_LRU(idleTimeThreshold: param),
                Type.TTL    => new SoundCache_TTL(ttlSeconds: param),
                Type.Random => new SoundCache_Random(maxCacheCount: (int)param),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }
}
