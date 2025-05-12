using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// サウンドリソースのキャッシュ管理を担うクラスの基底クラス<para></para>
/// - 派生クラスの削除方針ごとにEvict関数をオーバーライドさせる
/// </summary>
internal abstract class SoundCache_Base : ISoundCache
{
    protected readonly Dictionary<string, AudioClip> cache = new();

    /// <summary>
    /// 指定リソースをキャッシュから取得する<para></para>
    /// 取得時に最終アクセス時間も更新する
    /// </summary>
    public virtual AudioClip Retrieve(string resourceAddress)
    {
        if (cache.TryGetValue(resourceAddress, out var clip))
        {
            return clip;
        }
        return null;
    }

    /// <summary>
    /// 指定リソースをキャッシュに追加する<para></para>
    /// </summary>
    public virtual void Add(string resourceAddress, AudioClip clip)
    {
        cache[resourceAddress] = clip;
    }

    public virtual void Remove(string resourceAddress)
    {
        if (cache.TryGetValue(resourceAddress, out var clip))
        {
            Log.Safe($"Remove実行:{resourceAddress}");
            Addressables.Release(clip);
            cache.Remove(resourceAddress);
        }
    }

    /// <summary>
    /// キャッシュ内のAudioSourceを全て破棄する
    /// </summary>
    public virtual void ClearAll()
    {
        Log.Safe("ClearAll実行");
        foreach (var clip in cache.Values)
        {
            Addressables.Release(clip);
        }
        cache.Clear();
    }

    public abstract void Evict();
}