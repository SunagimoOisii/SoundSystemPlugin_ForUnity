namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 登録時刻に基づく削除戦略
    /// </summary>
    internal sealed class TTLEvictionStrategy : IEvictionStrategy
    {
        private readonly float ttlSeconds;
        private readonly Dictionary<string, float> registerTime = new();

        public TTLEvictionStrategy(float ttlSeconds)
        {
            this.ttlSeconds = ttlSeconds;
        }

        public void OnAdd(string key)
        {
            if (key != null) registerTime[key] = Time.time;
        }

        public void OnRetrieve(string key)
        {
        }

        public void OnRemove(string key)
        {
            if (key != null) registerTime.Remove(key);
        }

        public void OnClear()
        {
            registerTime.Clear();
        }

        public IEnumerable<string> SelectKeys(
            IReadOnlyDictionary<string, AudioClip> cache,
            IReadOnlyDictionary<string, int> usageCount)
        {
            float currentTime = Time.time;
            foreach (var entry in registerTime)
            {
                if (currentTime - entry.Value > ttlSeconds &&
                    (!usageCount.TryGetValue(entry.Key, out var count) || count <= 0))
                {
                    yield return entry.Key;
                }
            }
        }
    }
}
