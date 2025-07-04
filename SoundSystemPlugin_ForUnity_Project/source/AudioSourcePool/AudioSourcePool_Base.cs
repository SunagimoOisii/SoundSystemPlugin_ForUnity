namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SE向けにAudioSourceをプールで管理するクラスの基底クラス<para></para>
    /// - 未使用 AudioSource を返す処理を共通化
    /// - プールが埋まった場合の挙動のみ派生クラスで制御する
    /// </summary>
    internal abstract class AudioSourcePool_Base : IAudioSourcePool
    {
        protected readonly GameObject sourceRoot;
        protected readonly AudioMixerGroup seMixerG;
    
        protected Queue<AudioSource> pool;
        protected readonly int maxSize;
        protected readonly int initSize;
        public IEnumerable<AudioSource> GetAllResources() => pool;

        public AudioSourcePool_Base(AudioMixerGroup seMixerG, int initSize, int maxSize, bool persistent = false)
        {
            pool       = new();
            sourceRoot = new("SE_AudioSources");
            if (persistent) Object.DontDestroyOnLoad(sourceRoot);

            this.maxSize  = maxSize;
            this.initSize = initSize;
            this.seMixerG = seMixerG;
            if(this.initSize > maxSize) this.initSize = maxSize;

            //プール初期化
            for (int i = 0; i < initSize; i++)
            {
                var source = CreateSourceWithOwnerGameObject();
                pool.Enqueue(source);
            }
        }

        public AudioSource Retrieve()
        {
            Log.Safe("Retrieve実行");

            //未使用 AudioSource を検索
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

            //全て使用中だがプールが満杯ではない場合、新規作成
            if (pool.Count < maxSize)
            {
                var created = CreateSourceWithOwnerGameObject();
                pool.Enqueue(created);
                return created;
            }

            //プールが埋まっている場合、派生クラスに処理を委ねる
            return RetrieveWhenPoolFull();
        }

        protected abstract AudioSource RetrieveWhenPoolFull();
    
        public void Reinitialize()
        {
            Log.Safe("Reinitialize実行");
    
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

        public void Destroy()
        {
            foreach(var source in pool)
            {
                Object.Destroy(source.gameObject);
            }
            pool.Clear();
            Object.Destroy(sourceRoot);
        }
    
        protected AudioSource CreateSourceWithOwnerGameObject()
        {
            var obj = new GameObject("SESource");
            obj.transform.parent = sourceRoot.transform;
    
            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = seMixerG;
            return source;
        }
    }
}
