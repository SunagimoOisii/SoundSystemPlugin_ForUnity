using System;
using System.Collections.Generic;

/// <summary>
/// サウンドリソースのキャッシュ管理を担うクラス<para></para>
/// - キャッシュ数の上限を超えた場合、ランダムにリソースを選択して削除を行う
/// </summary>
internal sealed class SoundCache_Random : SoundCache_Base
{
    private readonly int maxCacheCount;
    private readonly Random random = new();

    public SoundCache_Random(int maxCacheCount)
    {
        this.maxCacheCount = maxCacheCount;
    }

    public override void Evict()
    {
        if (cache.Count <= maxCacheCount)
        {
            return;
        }

        int excessCount = cache.Count - maxCacheCount;
        var keys        = new List<string>(cache.Keys);

        Log.Safe($"Evict実行:{excessCount}件削除,max = {maxCacheCount}");
        for (int i = 0; i < excessCount; i++)
        {
            if (keys.Count == 0) break;

            int randomIndex  = random.Next(keys.Count);
            string randomKey = keys[randomIndex];

            Remove(randomKey);
            keys.RemoveAt(randomIndex);
        }
    }
}