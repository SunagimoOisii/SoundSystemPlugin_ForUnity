namespace SoundSystem
{
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// SoundSystemが操作するクラスの1つ<para></para>
    /// - AudioSourcePoolを用いたSE再生<para></para>
    /// </summary>
    internal sealed class SEManager : IDisposable
    {
        private readonly IAudioSourcePool sourcePool;
        private readonly ISoundLoader loader;
        private readonly ISoundCache cache;
        private readonly Dictionary<AudioSource, CancellationTokenSource> fadeCtsMap = new();

        //各トラック(AudioSource)でのサウンドの利用状況を追跡するために使用
        private readonly Dictionary<AudioSource, string> usageResourceMap = new();
    
        public SEManager(IAudioSourcePool sourcePool, ISoundLoader loader, ISoundCache cache)
        {
            this.sourcePool = sourcePool;
            this.loader     = loader;
            this.cache      = cache;
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

            //AudioSourceをプールから取得
            var source = sourcePool.Retrieve();
            if (source == null)
            {
                Log.Warn("Play中断:AudioSource取得に失敗");
                return;
            }

            CancelFade(source);
            RegisterResourceAddress(source, resourceAddress);

            //指定の音声設定で再生
            source.pitch              = pitch;
            source.volume             = volume;
            source.spatialBlend       = spatialBlend;
            source.transform.position = position;
            source.PlayOneShot(clip);
            await UniTask.WaitWhile(() => source.isPlaying);
            UnregisterResourceAddress(source);
            onComplete?.Invoke();

            Log.Safe($"Play成功:{resourceAddress},vol = {volume},pitch = {pitch}," +
                $"blend = {spatialBlend}");
        }

        public async UniTask FadeIn(string resourceAddress, float duration,
            float volume, float pitch, float spatialBlend, Vector3 position,
            Action onComplete = null)
        {
            Log.Safe($"FadeIn実行:{resourceAddress},dura = {duration},vol = {volume}");

            //サウンドリソースのロード
            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"FadeIn失敗:リソース読込に失敗,{resourceAddress}");
                return;
            }

            //AudioSourceをプールから取得
            var source = sourcePool.Retrieve();
            if (source == null)
            {
                Log.Warn("FadeIn中断:AudioSource取得に失敗");
                return;
            }

            CancelFade(source);
            RegisterResourceAddress(source, resourceAddress);

            //指定の音声設定で再生
            source.pitch              = pitch;
            source.spatialBlend       = spatialBlend;
            source.transform.position = position;
            source.clip               = clip;
            source.volume             = 0f;
            source.Play();

            await ExecuteVolumeTransition(
                source,
                duration,
                t => source.volume = Mathf.Lerp(0f, volume, t));

            await UniTask.WaitWhile(() => source.isPlaying);
            UnregisterResourceAddress(source);
            onComplete?.Invoke();

            Log.Safe($"FadeIn終了:{resourceAddress}");
        }

        public async UniTask FadeOutAll(float duration, Action onComplete = null)
        {
            Log.Safe($"FadeOut実行:dura = {duration}");

            var tasks = sourcePool.GetAllResources()
                .Where(s => s != null && s.isPlaying)
                .Select(s => FadeOutSource(s, duration));

            await UniTask.WhenAll(tasks);

            onComplete?.Invoke();

            Log.Safe($"FadeOut終了:dura = {duration}");
        }

        private async UniTask FadeOutSource(AudioSource source, float duration)
        {
            CancelFade(source);

            float start = source.volume;
            await ExecuteVolumeTransition(
                source,
                duration,
                t => source.volume = Mathf.Lerp(start, 0f, t),
                () =>
                {
                    source.Stop();
                    source.clip = null;
                    UnregisterResourceAddress(source);
                });
        }

        private async UniTask ExecuteVolumeTransition(AudioSource source,
            float duration, Action<float> onProgress, Action onComplete = null)
        {
            if (duration <= 0f)
            {
                Log.Warn($"ExecuteVolumeTransition中断:不正な duration {duration}");
                return;
            }

            var cts = new CancellationTokenSource();
            fadeCtsMap[source] = cts;

            float elapsed = 0f;
            var token = cts.Token;
            try
            {
                while (elapsed < duration)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    float t = elapsed / duration;
                    onProgress(t);

                    elapsed += Time.deltaTime;
                    await UniTask.Yield(token);
                }

                onProgress(1.0f);
                onComplete?.Invoke();
            }
            finally
            {
                cts.Dispose();
                fadeCtsMap.Remove(source);
            }
        }

        private void RegisterResourceAddress(AudioSource source, string resourceAddress)
        {
            cache.BeginUse(resourceAddress);
            usageResourceMap[source] = resourceAddress;
        }

        private void UnregisterResourceAddress(AudioSource source)
        {
            if (usageResourceMap.TryGetValue(source, out var resourceAddress))
            {
                cache.EndUse(resourceAddress);
                usageResourceMap.Remove(source);
            }
        }

        private void CancelFade(AudioSource source)
        {
            if (fadeCtsMap.TryGetValue(source, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                fadeCtsMap.Remove(source);
            }
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
            sourcePool.Destroy();
        }
    }
}
