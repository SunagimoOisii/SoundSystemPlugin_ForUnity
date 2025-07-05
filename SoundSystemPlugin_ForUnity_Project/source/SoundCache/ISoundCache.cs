namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public interface ISoundCache
    {
        int Count { get; }

        IEnumerable<string> Keys { get; }

        AudioClip Retrieve(string resourceAddress);

        void Add(string resourceAddress, AudioClip clip);
    
        void Remove(string resourceAddress);
    
        void ClearAll();

        void Evict();

        void BeginUse(string resourceAddress);

        void EndUse(string resourceAddress);
    }
}
