namespace SoundSystem
{
    using System;
    using UnityEngine.Audio;
    
    /// <summary>
    /// IAudioSourcePoolインスタンスを生成するファクトリークラス
    /// </summary>
    public static class AudioSourcePoolFactory
    {
        public enum Kind
        {
            FIFO,
            Strict
        }

        /// <summary>
        /// 指定プール管理方式に応じたIAudioSourcePoolインスタンスを生成
        /// </summary>
        public static IAudioSourcePool Create(Kind k,
            AudioMixerGroup seMixerG, int initSize, int maxSize, bool persistent = false)
        {
            return k switch
            {
                Kind.FIFO   => new AudioSourcePool_FIFO(seMixerG, initSize, maxSize, persistent),
                Kind.Strict => new AudioSourcePool_Strict(seMixerG, initSize, maxSize, persistent),
                _ => throw new ArgumentOutOfRangeException(nameof(k)),
            };
        }
    }
}
