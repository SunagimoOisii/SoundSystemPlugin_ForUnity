namespace SoundSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// ランダムに削除する戦略
    /// </summary>
    internal sealed class RandomEvictionStrategy : IEvictionStrategy
    {
        private readonly int maxCacheCount;
        private readonly Random random = new();

        public RandomEvictionStrategy(int maxCacheCount)
        {
            this.maxCacheCount = maxCacheCount;
        }

        public void OnAdd(string key)
        {
        }

        public void OnRetrieve(string key)
        {
        }

        public void OnRemove(string key)
        {
        }

        public void OnClear()
        {
        }

        public IEnumerable<string> SelectKeys(
            IReadOnlyDictionary<string, AudioClip> cache,
            IReadOnlyDictionary<string, int> usageCount)
        {
            if (cache.Count <= maxCacheCount) yield break;

            int excessCount = cache.Count - maxCacheCount;
            var keys = new List<string>();
            foreach (var k in cache.Keys)
            {
                if (!usageCount.TryGetValue(k, out var c) || c <= 0)
                {
                    keys.Add(k);
                }
            }

            excessCount = Math.Min(excessCount, keys.Count);

            for (int i = 0; i < excessCount; i++)
            {
                if (keys.Count == 0) yield break;

                int index  = random.Next(keys.Count);
                string key = keys[index];
                keys.RemoveAt(index);
                yield return key;
            }
        }
    }
}
