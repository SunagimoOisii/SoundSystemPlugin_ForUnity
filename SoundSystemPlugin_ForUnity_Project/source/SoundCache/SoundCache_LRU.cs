namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// TEh\[X̃LbVǗSNX<para></para>
    /// - ŏIANZXԂɊÂAw莞Ԗgp̃\[X폜ΏۂƂ
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
            var clip = base.Retrieve(resourceAddress);
            if (clip != null)
            {
                lastAccessTime[resourceAddress] = Time.time;
            }
            return clip;
        }
    
        public override void Add(string resourceAddress, AudioClip clip)
        {
            base.Add(resourceAddress, clip);
            lastAccessTime[resourceAddress] = Time.time;
        }
    
        public override void Remove(string resourceAddress)
        {
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
            var currentTime = Time.time;
            var toRemove = new List<string>();
    
            Log.Safe($"Evicts:{toRemove.Count}폜,idle = {idleTimeThreshold}");
            foreach (var entry in lastAccessTime)
            {
                if (currentTime - entry.Value > idleTimeThreshold)
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
