namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// サウンドリソースのキャッシュ管理を担うクラス<para></para>
    /// - 最終アクセス時間に基づき、指定時間未使用のリソースを削除対象とする
    /// </summary>
    internal sealed class SoundCache_LRU : SoundCache_Base
    {
        private readonly float idleTimeThreshold;
        private readonly Dictionary<string, float> lastAccessTime = new();
    
        public SoundCache_LRU(float idleTimeThreshold)
        {
            this.idleTimeThreshold = idleTimeThreshold;
        }
    
        public override AudioClip Retrieve(string resourceAddress)
        {
            if(resourceAddress == null) return null;

            var clip = base.Retrieve(resourceAddress);
            if (clip != null) lastAccessTime[resourceAddress] = Time.time;

            return clip;
        }
    
        public override void Add(string resourceAddress, AudioClip clip)
        {
            if(resourceAddress == null ||
               clip == null) return;

            base.Add(resourceAddress, clip);
            lastAccessTime[resourceAddress] = Time.time;
        }
    
        public override void Remove(string resourceAddress)
        {
            if (resourceAddress == null) return;

            base.Remove(resourceAddress);
            lastAccessTime.Remove(resourceAddress);
        }
    
        public override void ClearAll()
        {
            base.ClearAll();
            lastAccessTime.Clear();
        }
    
        public override void Evict()
        {
            var toRemove    = new List<string>();
            var currentTime = Time.time;

            foreach (var entry in lastAccessTime)
            {
                if (currentTime - entry.Value > idleTimeThreshold &&
                    (usageCount.TryGetValue(entry.Key, out var count) == false || count <= 0))
                {
                    toRemove.Add(entry.Key);
                }
            }
            Log.Safe($"Evict実行:{toRemove.Count}件削除,idle = {idleTimeThreshold}");
    
            foreach (var key in toRemove)
            {
                Remove(key);
            }
        }
    }
}
