namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    
    /// <summary>
    /// TEh\[X̃LbVǗSNX̊NX<para></para>
    /// - hNX̍폜jƂEvict֐I[o[Ch
    /// </summary>
    internal abstract class SoundCache_Base : ISoundCache
    {
        protected readonly Dictionary<string, AudioClip> cache = new();
    
        /// <summary>
        /// w胊\[XLbV擾<para></para>
        /// 擾ɍŏIANZXԂXV
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
        /// w胊\[XLbVɒǉ<para></para>
        /// </summary>
        public virtual void Add(string resourceAddress, AudioClip clip)
        {
            cache[resourceAddress] = clip;
        }
    
        public virtual void Remove(string resourceAddress)
        {
            if (cache.TryGetValue(resourceAddress, out var clip))
            {
                Log.Safe($"Removes:{resourceAddress}");
                Addressables.Release(clip);
                cache.Remove(resourceAddress);
            }
        }
    
        /// <summary>
        /// LbVAudioSourceSĔj
        /// </summary>
        public virtual void ClearAll()
        {
            Log.Safe("ClearAlls");
            foreach (var clip in cache.Values)
            {
                Addressables.Release(clip);
            }
            cache.Clear();
        }
    
        public abstract void Evict();
    }
}
