namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 最終アクセス時間に基づく削除戦略
    /// </summary>
    internal sealed class EvictionStrategy_LRU : IEvictionStrategy
    {
        private readonly float idleTimeThreshold;
        private readonly Dictionary<string, float> lastAccessTime = new();

        public EvictionStrategy_LRU(float idleTimeThreshold)
        {
            this.idleTimeThreshold = idleTimeThreshold;
        }

        public void OnAdd(string key) { if (key != null) lastAccessTime[key] = Time.time; }
        public void OnRetrieve(string key){ if (key != null) lastAccessTime[key] = Time.time; }
        public void OnRemove(string key){ if (key != null) lastAccessTime.Remove(key); }
        public void OnClear(){ lastAccessTime.Clear(); }

        public IEnumerable<string> SelectKeys(
            IReadOnlyDictionary<string, AudioClip> cache,
            IReadOnlyDictionary<string, int> usageCount)
        {
            float currentTime = Time.time;
            foreach (var entry in lastAccessTime)
            {
                if (currentTime - entry.Value > idleTimeThreshold &&
                   (usageCount.TryGetValue(entry.Key, out var count) == false || count <= 0))
                {
                    yield return entry.Key;
                }
            }
        }
    }
}
