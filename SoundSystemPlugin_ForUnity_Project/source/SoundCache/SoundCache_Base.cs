namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// サウンドリソースのキャッシュ管理を担うクラスの基底クラス<para></para>
    /// - 削除方針を IEvictionStrategy として外部から受け取る
    /// </summary>
    internal class SoundCache_Base : ISoundCache
    {
        protected readonly Dictionary<string, AudioClip> cache = new();
        protected readonly Dictionary<string, int> usageCount  = new();
        private readonly IEvictionStrategy strategy;
        private ISoundLoader loader;

        internal SoundCache_Base(IEvictionStrategy strategy)
        {
            this.strategy = strategy;
        }

        internal void SetLoader(ISoundLoader l)
        {
            if (l != null) loader = l;
        }
    
        /// <summary>
        /// 指定リソースをキャッシュから取得する
        /// </summary>
        public virtual AudioClip Retrieve(string resourceAddress)
        {
            if(resourceAddress == null) return null;

            if (cache.TryGetValue(resourceAddress, out var clip))
            {
                strategy?.OnRetrieve(resourceAddress);
                return clip;
            }
            return null;
        }
    
        /// <summary>
        /// 指定リソースをキャッシュに追加する<para></para>
        /// </summary>
        public virtual void Add(string resourceAddress, AudioClip clip)
        {
            if(resourceAddress == null ||
               clip == null) return;

            cache[resourceAddress] = clip;
            if (usageCount.ContainsKey(resourceAddress) == false)
            {
                usageCount[resourceAddress] = 0;
            }
            strategy?.OnAdd(resourceAddress);
        }
    
        public virtual void Remove(string resourceAddress)
        {
            if (resourceAddress == null) return;

            if (cache.TryGetValue(resourceAddress, out var clip))
            {
                Log.Safe($"Remove実行:{resourceAddress}");
                loader?.UnloadClip(clip);
                cache.Remove(resourceAddress);
                usageCount.Remove(resourceAddress);
                strategy?.OnRemove(resourceAddress);
            }
        }
    
        /// <summary>
        /// キャッシュ内のAudioClipを全て破棄する
        /// </summary>
        public virtual void ClearAll()
        {
            Log.Safe("ClearAll実行");
            foreach (var clip in cache.Values)
            {
                loader?.UnloadClip(clip);
            }
            cache.Clear();
            usageCount.Clear();
            strategy?.OnClear();
        }

        public virtual void Evict()
        {
            if (strategy == null) return;

            var keys = strategy.SelectKeys(cache, usageCount);
            var toRemove = new List<string>(keys);
            Log.Safe($"Evict実行:{toRemove.Count}件削除");
            foreach (var key in toRemove)
            {
                Remove(key);
            }
        }

        public void BeginUse(string resourceAddress)
        {
            if (resourceAddress == null) return;

            if (usageCount.ContainsKey(resourceAddress)) usageCount[resourceAddress]++;
            else                                         usageCount[resourceAddress] = 1;
        }

        public void EndUse(string resourceAddress)
        {
            if (resourceAddress == null ||
                usageCount.TryGetValue(resourceAddress, out var count) == false) return;

            count--;
            if(count <= 0) usageCount.Remove(resourceAddress);
            else           usageCount[resourceAddress] = count;
        }
    }
}
