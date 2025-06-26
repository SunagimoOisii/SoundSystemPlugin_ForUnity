namespace SoundSystem
{
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// SoundSystemが操作するクラスの1つ<para></para>
    /// - AudioSourcePoolを用いたSE再生<para></para>
    /// </summary>
    internal sealed class SEManager : IDisposable
    {
        private readonly IAudioSourcePool sourcePool;
        private readonly ISoundLoader loader;
        private readonly Dictionary<AudioSource, AudioSourceFader> faders = new();

        public SEManager(IAudioSourcePool sourcePool, ISoundLoader loader)
        {
            this.sourcePool = sourcePool;
            this.loader     = loader;
        }

        private AudioSourceFader GetFader(AudioSource source)
        {
            if (faders.TryGetValue(source, out var f)) return f;
            f = new AudioSourceFader(source);
            faders[source] = f;
            return f;
        }
    
        /// <param name="volume">音量(0〜1)</param>
        /// <param name="pitch">ピッチ(0〜1)</param>
        /// <param name="spatialBlend">サラウンド度(0〜1)</param>
        /// <param name="position">再生座標</param>
        public async UniTask Play(string resourceAddress,
            float volume, float pitch, float spatialBlend, Vector3 position,
            Action onComplete = null)
        {
            //サウンドリソースのロード
            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"Play失敗:リソース読込に失敗,{resourceAddress}");
                return;
            }
    
            //AudioSourcePoolを取得
            var source = sourcePool.Retrieve();
            if (source == null)
            {
                Log.Warn("Play中断:AudioSource取得に失敗");
                return;
            }
    
            //指定の音声設定で再生
            source.pitch              = pitch;
            source.volume             = volume;
            source.spatialBlend       = spatialBlend;
            source.transform.position = position;
            source.PlayOneShot(clip);
            await UniTask.WaitWhile(() => source.isPlaying);
            onComplete?.Invoke();

            Log.Safe($"Play成功:{resourceAddress},vol = {volume},pitch = {pitch}," +
                $"blend = {spatialBlend}");
        }

        /// <summary>
        /// フェードイン付きでSEを再生
        /// </summary>
        public async UniTask FadeIn(string resourceAddress, float duration,
            float volume, float pitch, float spatialBlend, Vector3 position,
            Action onComplete = null)
        {
            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"FadeIn失敗:リソース読込に失敗,{resourceAddress}");
                return;
            }

            var source = sourcePool.Retrieve();
            if (source == null)
            {
                Log.Warn("FadeIn中断:AudioSource取得に失敗");
                return;
            }

            var fader = GetFader(source);
            fader.Cancel();

            source.pitch              = pitch;
            source.spatialBlend       = spatialBlend;
            source.transform.position = position;
            source.clip               = clip;
            source.volume             = 0f;
            source.Play();

            await fader.Fade(0f, volume, duration);
            await UniTask.WaitWhile(() => source.isPlaying);
            onComplete?.Invoke();

            source.clip = null;
        }

        /// <summary>
        /// 全てのSEをフェードアウト
        /// </summary>
        public async UniTask FadeOutAll(float duration, Action onComplete = null)
        {
            var sources = sourcePool.GetAllResources();
            var tasks = new List<UniTask>();
            foreach (var source in sources)
            {
                if (source == null || source.isPlaying == false) continue;
                var fader = GetFader(source);
                fader.Cancel();
                tasks.Add(fader.Fade(source.volume, 0f, duration));
            }
            await UniTask.WhenAll(tasks);

            foreach (var source in sources)
            {
                if (source == null || source.isPlaying == false) continue;
                source.Stop();
                source.clip = null;
            }
            onComplete?.Invoke();
        }
    
        public void StopAll()
        {
            Log.Safe("StopAll実行");

            var sources = sourcePool.GetAllResources();
            foreach (var source in sources)
            {
                if (source == null) continue;
                source.Stop();
            }
        }
    
        public void ResumeAll()
        {
            Log.Safe("ResumeAll実行");

            var sources = sourcePool.GetAllResources();
            foreach (var source in sources)
            {
                if (source == null) continue;
                source.UnPause();
            }
        }
    
        public void PauseAll()
        {
            Log.Safe("PauseAll実行");

            var sources = sourcePool.GetAllResources();
            foreach (var source in sources)
            {
                if (source == null) continue;
                source.Pause();
            }
        }

        public void Dispose()
        {
            foreach (var f in faders.Values)
            {
                f.Dispose();
            }
            faders.Clear();
            sourcePool.Destroy();
        }
    }
}
