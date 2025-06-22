namespace SoundSystem
{
    using System;
    using UnityEngine.Audio;
    
    /// <summary>
    /// IAudioSourcePoolインスタンスを生成するファクトリークラス
    /// </summary>
    public static class AudioSourcePoolFactory
    {
        public enum Type
        {
            FIFO,
            Strict
        }
    
        /// <summary>
        /// 指定プール管理方式に応じたIAudioSourcePoolインスタンスを生成
        /// </summary>
        public static IAudioSourcePool Create(AudioMixerGroup seMixerG,
            int initSize, int maxSize, Type type = Type.FIFO)
        {
            return type switch
            {
                Type.FIFO   => new AudioSourcePool_FIFO(seMixerG, initSize, maxSize),
                Type.Strict => new AudioSourcePool_Strict(seMixerG, initSize, maxSize),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }
}
