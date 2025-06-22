namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public interface IAudioSourcePool
    {
        AudioSource Retrieve();
    
        void Reinitialize();
    
        IEnumerable<AudioSource> GetAllResources();
    }
}
