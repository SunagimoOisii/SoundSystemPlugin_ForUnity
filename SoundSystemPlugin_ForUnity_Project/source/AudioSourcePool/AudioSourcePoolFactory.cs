namespace SoundSystem
{
    using System;
    using UnityEngine.Audio;
    
    /// <summary>
    /// IAudioSourcePoolインスタンスを生成するファクトリークラス
    /// </summary>
    public static class AudioSourcePoolFactory
    {
        public enum PoolType
        {
            FIFO,
            Strict
        }
    
        /// <summary>
        /// 指定プール管理方式に応じたIAudioSourcePoolインスタンスを生成
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
