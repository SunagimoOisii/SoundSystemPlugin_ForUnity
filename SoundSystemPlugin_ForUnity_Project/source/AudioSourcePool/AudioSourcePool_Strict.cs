namespace SoundSystem
{
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SEAudioSourcev[ŊǗNX<para></para>
    /// - gpAudioSource΂Ԃ<para></para>
    /// - SĎgpōőTCYȂVK쐬̂Ԃ<para></para>
    /// - SĎgpōőTCYȂnullԂ
    /// </summary>
    internal sealed class AudioSourcePool_Strict : AudioSourcePool_Base
    {
        public AudioSourcePool_Strict(AudioMixerGroup mixerG, int initSize,
            int maxSize)
            : base(mixerG, initSize, maxSize)
        {
        }
    
        public override AudioSource Retrieve()
        {
            Log.Safe("Retrieves");
    
            //gpAudioSource΁AԂ
            for (int i = 0; i < pool.Count; i++)
            {
                var source = pool.Dequeue();
                if (source.isPlaying == false)
                {
                    pool.Enqueue(source);
                    return source;
                }
    
                pool.Enqueue(source);
            }
    
            //v[őTCYȂVK쐬̂Ԃ
            if (pool.Count < maxSize)
            {
                var created = CreateSourceWithOwnerGameObject();
                pool.Enqueue(created);
                return created;
            }
    
            //őTCYőSĎgpȂnull
            return null;
        }
    }
}
