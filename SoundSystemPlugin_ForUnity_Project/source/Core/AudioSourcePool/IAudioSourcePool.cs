namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public interface IAudioSourcePool
    {
        IEnumerable<AudioSource> GetAllResources();

        AudioSource Retrieve();
    
        void Reinitialize();

        void Destroy();
    }
}
