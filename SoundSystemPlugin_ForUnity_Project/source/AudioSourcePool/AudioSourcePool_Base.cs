namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// SEAudioSourcev[ŊǗNX̊NX<para></para>
    /// - hNX̕jƂRetrieve֐I[o[Ch
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
    
            //v[
            for (int i = 0; i < initSize; i++)
            {
                var source = CreateSourceWithOwnerGameObject();
                pool.Enqueue(source);
            }
        }
    
        public void Reinitialize()
        {
            Log.Safe("Reinitializes");
    
            //v[̗vfSĖgpɂ
            foreach (var source in pool)
            {
                source.Stop();
                source.clip = null;
            }
    
            //v[TCY̒lɖ߂
            while (pool.Count > initSize) //ߎ
            {
                var source = pool.Dequeue();
                Object.Destroy(source.gameObject);
            }
            while (pool.Count < initSize) //s
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
