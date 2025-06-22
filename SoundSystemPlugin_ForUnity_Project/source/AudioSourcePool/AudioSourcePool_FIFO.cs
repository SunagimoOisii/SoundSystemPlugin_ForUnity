namespace SoundSystem
{
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SEAudioSourcev[ŊǗNX<para></para>
    /// - gpAudioSource΂Ԃ<para></para>
    /// - SĎgpōőTCYȂŌÂ̂̂ėp<para></para>
    /// - SĎgpōőTCYȂVK쐬̂Ԃ
    /// </summary>
    internal sealed class AudioSourcePool_FIFO : AudioSourcePool_Base
    {
        public AudioSourcePool_FIFO(AudioMixerGroup mixerG, int initSize,
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
    
            //v[őTCY̏ꍇAŌÂ̂̂Iɒfėp
            if (pool.Count >= maxSize)
            {
                var oldest = pool.Dequeue();
                oldest.Stop();
                pool.Enqueue(oldest);
                return oldest;
            }
            else //őTCYȂVK쐬
            {
                var created = CreateSourceWithOwnerGameObject();
                pool.Enqueue(created);
                return created;
            }
        }
    }
}
