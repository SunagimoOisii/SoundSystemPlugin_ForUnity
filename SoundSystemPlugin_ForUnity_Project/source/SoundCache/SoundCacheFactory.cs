using System;

/// <summary>
/// ISoundCacheインスタンスを生成するファクトリークラス
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
    /// 指定キャッシュ方式に応じたISoundCacheインスタンスを生成する
    /// </summary>
    /// <param name="param">方式に応じたパラメータ(秒数または最大数)</param>
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