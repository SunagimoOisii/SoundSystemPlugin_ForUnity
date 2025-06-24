namespace SoundSystem
{
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SE向けにAudioSourceをプールで管理するクラス<para></para>
    /// - 未使用のAudioSourceがあればそれを返す<para></para>
    /// - 全て使用中で最大サイズ未満なら新規作成したものを返す<para></para>
    /// - 全て使用中で最大サイズならnullを返す
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
            Log.Safe("Retrieve実行");
    
            //未使用のAudioSourceがあれば、それを返す
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
    
            //プールが最大サイズ未満なら新規作成したものを返す
            //最大サイズで全て使用中ならnull
            if (pool.Count < maxSize)
            {
                var created = CreateSourceWithOwnerGameObject();
                pool.Enqueue(created);
                return created;
            }            
            return null;
        }
    }
}
