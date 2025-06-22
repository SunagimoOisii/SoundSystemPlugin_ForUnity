namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// サウンドリソースのキャッシュ管理を担うクラス<para></para>
    /// - 登録時刻から指定時間を超えたリソースを削除対象とする
    /// </summary>
    internal sealed class SoundCache_TTL : SoundCache_Base
    {
        private readonly float ttlSeconds;
        private readonly Dictionary<string, float> registerTime = new();
    
        public SoundCache_TTL(float ttlSeconds)
        {
            this.ttlSeconds = ttlSeconds;
        }
    
        public override void Add(string resourceAddress, AudioClip clip)
        {
            base.Add(resourceAddress, clip);
            registerTime[resourceAddress] = Time.time;
        }
    
        public override void Remove(string resourceAddress)
        {
            base.Remove(resourceAddress);
            registerTime.Remove(resourceAddress);
        }
    
        public override void ClearAll()
        {
            base.ClearAll();
            registerTime.Clear();
        }
    
        public override void Evict()
        {
            var currentTime = Time.time;
            var toRemove = new List<string>();
    
            Log.Safe($"Evict実行:{toRemove.Count}件削除, ttl = {ttlSeconds}");
            foreach (var entry in registerTime)
            {
                if (currentTime - entry.Value > ttlSeconds)
                {
                    toRemove.Add(entry.Key);
                }
            }
    
            foreach (var key in toRemove)
            {
                Remove(key);
            }
        }
    }
}
