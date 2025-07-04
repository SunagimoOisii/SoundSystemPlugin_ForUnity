namespace SoundSystem
{
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SE向けにAudioSourceをプールで管理するクラス<para></para>
    /// - 未使用の AudioSource がなければプールの最古を再利用
    /// </summary>
    internal sealed class AudioSourcePool_FIFO : AudioSourcePool_Base
    {
        public AudioSourcePool_FIFO(AudioMixerGroup mixerG, int initSize,
            int maxSize, bool persistent = false)
            : base(mixerG, initSize, maxSize, persistent)
        {
        }
    
        protected override AudioSource RetrieveWhenPoolFull()
        {
            var oldest = pool.Dequeue();
            oldest.Stop();
            pool.Enqueue(oldest);
            return oldest;
        }
    }
}
