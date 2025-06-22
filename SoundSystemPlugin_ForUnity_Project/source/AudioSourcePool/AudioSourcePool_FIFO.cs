namespace SoundSystem
{
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SE向けにAudioSourceをプールで管理するクラス<para></para>
    /// - 未使用のAudioSourceがあればそれを返す<para></para>
    /// - 全て使用中で最大サイズなら最古のものを再利用<para></para>
    /// - 全て使用中で最大サイズ未満なら新規作成したものを返す
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
            Log.Safe("Retrieves実行");
    
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
    
            //プールが最大サイズの場合、最古のものを再利用
            if (pool.Count >= maxSize)
            {
                var oldest = pool.Dequeue();
                oldest.Stop();
                pool.Enqueue(oldest);
                return oldest;
            }
            else //最大サイズ未満なら新規作成
            {
                var created = CreateSourceWithOwnerGameObject();
                pool.Enqueue(created);
                return created;
            }
        }
    }
}
