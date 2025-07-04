namespace SoundSystem
{
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SE向けにAudioSourceをプールで管理するクラス<para></para>
    /// - 未使用の AudioSource がなく、プールが埋まっていれば null を返す
    /// </summary>
    internal sealed class AudioSourcePool_Strict : AudioSourcePool_Base
    {
        public AudioSourcePool_Strict(AudioMixerGroup mixerG, int initSize,
            int maxSize, bool persistent = false)
            : base(mixerG, initSize, maxSize, persistent)
        {
        }
    
        protected override AudioSource RetrieveWhenPoolFull()
        {
            return null;
        }
    }
}
