namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SE向けにAudioSourceをプールで管理するクラスの基底クラス<para></para>
    /// - 派生クラスの方針ごとにRetrieve関数をオーバーライドさせる
    /// </summary>
    internal abstract class AudioSourcePool_Base : IAudioSourcePool
    {
        protected readonly GameObject sourceRoot;
        protected readonly AudioMixerGroup mixerGroup;
    
        protected Queue<AudioSource> pool;
        protected readonly int maxSize;
        protected readonly int initSize;
        public IEnumerable<AudioSource> GetAllResources() => pool;
    
        public AudioSourcePool_Base(AudioMixerGroup mixerG, int initSize, int maxSize)
        {
            pool         = new();
            sourceRoot   = new("SE_AudioSources");
            this.maxSize = maxSize;
            this.initSize = initSize;
            this.mixerGroup = mixerG;
    
            //プール初期化
            for (int i = 0; i < initSize; i++)
            {
                var source = CreateSourceWithOwnerGameObject();
                pool.Enqueue(source);
            }
        }
    
        public void Reinitialize()
        {
            Log.Safe("Reinitializes実行");
    
            //プール内の要素を全て未使用にする
            foreach (var source in pool)
            {
                source.Stop();
                source.clip = null;
            }
    
            //プールサイズを初期化時の値に戻す
            while (pool.Count > initSize) //超過時
            {
                var source = pool.Dequeue();
                Object.Destroy(source.gameObject);
            }
            while (pool.Count < initSize) //不足時
            {
                pool.Enqueue(CreateSourceWithOwnerGameObject());
            }
        }
    
        public abstract AudioSource Retrieve();
    
        protected AudioSource CreateSourceWithOwnerGameObject()
        {
            var obj = new GameObject("SESource");
            obj.transform.parent = sourceRoot.transform;
    
            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mixerGroup;
            return source;
        }
    }
}
