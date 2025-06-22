namespace SoundSystem
{
    using System;
    using UnityEngine.Audio;
    
    /// <summary>
    /// IAudioSourcePoolCX^X𐶐t@Ng[NX
    /// </summary>
    public static class AudioSourcePoolFactory
    {
        public enum PoolType
        {
            FIFO,
            Strict
        }
    
        /// <summary>
        /// wv[ǗɉIAudioSourcePoolCX^X𐶐
        /// </summary>
        public static IAudioSourcePool Create(AudioMixerGroup seMixerG,
            int initSize, int maxSize, PoolType type = PoolType.FIFO)
        {
            return type switch
            {
                PoolType.FIFO   => new AudioSourcePool_FIFO(seMixerG, initSize, maxSize),
                PoolType.Strict => new AudioSourcePool_Strict(seMixerG, initSize, maxSize),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }
}
