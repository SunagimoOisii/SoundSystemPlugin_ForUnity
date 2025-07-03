namespace SoundSystem
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Audio;
    using System.Threading;

    public enum BGMState
    {
        Idle,
        Play,
        Pause,
        FadeIn,
        FadeOut,
        CrossFade
    }

    /// <summary>
    /// BGMの再生、停止、フェード機能を提供するクラス<para></para>
    /// </summary>
    internal sealed class BGMManager : IDisposable
    {
        public BGMState State { get; private set; } = BGMState.Idle;

        private readonly ISoundLoader loader;
        private readonly ISoundCache cache;

        private string usageResourceAddress;

        private readonly GameObject sourceRoot = null;
        private (AudioSource active, AudioSource inactive) bgmSources;

        private CancellationTokenSource fadeCTS;
        private string pendingResourceAddress;

        /// <param name="mixerGroup">BGM出力先のAudioMixerGroup</param>
        public BGMManager(AudioMixerGroup mixerGroup, ISoundLoader loader, ISoundCache cache, bool persistent = false)
        {
            this.loader = loader;
            this.cache  = cache;

            //BGM専用AudioSourceとそれがアタッチされたGameObjectを作成
            sourceRoot = new("BGM_AudioSources");
            if (persistent)
            {
                UnityEngine.Object.DontDestroyOnLoad(sourceRoot);
            }
            bgmSources =
            (
                CreateSourceObj("BGMSource_0"),
                CreateSourceObj("BGMSource_1")
            );

            AudioSource CreateSourceObj(string name)
            {
                var obj = new GameObject(name);
                obj.transform.parent = sourceRoot.transform;

                var source = obj.AddComponent<AudioSource>();
                source.loop = true;
                source.playOnAwake = false;
                source.outputAudioMixerGroup = mixerGroup;
                return source;
            }
        }

        /// <param name="volume">音量(範囲: 0.0～1.0)</param>
        public async UniTask Play(string resourceAddress, float volume,
            Action onComplete = null)
        {
            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"Play失敗:リソース読込に失敗,{resourceAddress}");
                return;
            }
            cache.BeginUse(resourceAddress);
            usageResourceAddress = resourceAddress;

            State = BGMState.Play;
            bgmSources.active.clip   = clip;
            bgmSources.active.volume = volume;
            bgmSources.active.Play();
            onComplete?.Invoke();

            Log.Safe($"Play成功:{resourceAddress},vol = {volume}");
        }

        public void Stop()
        {
            Log.Safe("Stop実行");
            State = BGMState.Idle;

            CancelFade();
            bgmSources.active.Stop();
            bgmSources.active.clip = null;
            //クロスフェードキャンセル対応のため
            //もう一つの AudioSource も停止
            bgmSources.inactive.Stop();
            bgmSources.inactive.clip = null;

            cache.EndUse(usageResourceAddress);
            usageResourceAddress = null;
        }

        public void Resume()
        {
            Log.Safe("Resume実行");

            if (State != BGMState.Pause)
            {
                Log.Warn($"Resume中断:Pauseステート以外では実行不可");
                return;
            }

            State = BGMState.Play;
            bgmSources.active.UnPause();
        }

        public void Pause()
        {
            Log.Safe($"Pause実行");

            State = BGMState.Pause;
            bgmSources.active.Pause();
        }

        /// <param name="volume">目標音量(範囲: 0.0～1.0)</param>
        public async UniTask FadeIn(string resourceAddress, float duration, float volume,
            Action onComplete = null)
        {
            Log.Safe($"FadeIn実行:{resourceAddress},dura = {duration},vol = {volume}");

            if (State == BGMState.Play ||
                State == BGMState.CrossFade)
            {
                Log.Warn($"FadeIn中断:ステートの不一致,State = {State}");
                return;
            }

            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"FadeIn失敗:リソース読込に失敗,{resourceAddress}");
                return;
            }
            cache.BeginUse(resourceAddress);
            usageResourceAddress = resourceAddress;

            //フェード開始
            State = BGMState.FadeIn;
            bgmSources.active.clip   = clip;
            bgmSources.active.volume = 0;
            bgmSources.active.Play();
            await ExecuteVolumeTransition(
                duration,
                progressRate => bgmSources.active.volume = Mathf.Lerp(0f, volume, progressRate),
                () =>
                {
                    State = BGMState.Play;
                    onComplete?.Invoke();
                });

            Log.Safe($"FadeIn終了:{resourceAddress},dura = {duration},vol = {volume}");
        }

        public async UniTask FadeOut(float duration, Action onComplete = null)
        {
            Log.Safe($"FadeOut実行:dura = {duration}");

            if (State == BGMState.Pause)
            {
                Log.Warn($"FadeOut中断:Pause ステートでの実行");
                return;
            }

            //フェード開始
            State = BGMState.FadeOut;
            float startVol = bgmSources.active.volume;
            await ExecuteVolumeTransition(
                duration,
                progressRate => bgmSources.active.volume = Mathf.Lerp(startVol, 0.0f, progressRate),
                () => //onComplete
                {
                    State = BGMState.Idle;
                    bgmSources.active.Stop();
                    bgmSources.active.clip = null;

                    cache.EndUse(usageResourceAddress);
                    usageResourceAddress = null;

                    onComplete?.Invoke();
                });

            Log.Safe($"FadeOut終了:dura = {duration}");
        }

        public async UniTask CrossFade(string resourceAddress, float duration,
            Action onComplete = null)
        {
            Log.Safe($"CrossFade実行:{resourceAddress}");

            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"CrossFade失敗:リソース読込に失敗,{resourceAddress}");
                return;
            }
            cache.BeginUse(resourceAddress);
            pendingResourceAddress = resourceAddress;

            //フェード開始
            State = BGMState.CrossFade;
            bgmSources.inactive.clip = clip;
            bgmSources.inactive.volume = 0f;
            bgmSources.inactive.Play();
            await ExecuteVolumeTransition(
                duration,
                progressRate =>
                {
                    bgmSources.active.volume   = Mathf.Lerp(1f, 0f, progressRate);
                    bgmSources.inactive.volume = Mathf.Lerp(0f, 1f, progressRate);
                },
                () => //onComplete
                {
                    State = BGMState.Play;

                    //AudioSource 入れ替え
                    bgmSources.active.Stop();
                    bgmSources = (bgmSources.inactive, bgmSources.active);

                    //再生中リソースのアドレス更新
                    cache.EndUse(usageResourceAddress);
                    usageResourceAddress = resourceAddress;

                    onComplete?.Invoke();
                });

            pendingResourceAddress = null;
            Log.Safe($"CrossFade終了:{resourceAddress}");
        }

        private async UniTask ExecuteVolumeTransition(float duration, Action<float> onProgress,
            Action onComplete = null)
        {
            if (duration <= 0f)
            {
                Log.Warn($"ExecuteVolumeTransition中断:不正な duration {duration}");
                return;
            }

            //既に実行中のフェード処理は終了
            CancelFade();
            fadeCTS = new CancellationTokenSource();

            float elapsed = 0f;
            var token = fadeCTS.Token;
            try
            {
                while (elapsed < duration)
                {
                    var isCancelled = await UniTask.Yield(token).SuppressCancellationThrow();
                    if (isCancelled)
                    {
                        Log.Safe("ExecuteVolumeTransition中断:SuppressCancellationThrow");
                        return;
                    }

                    float t = elapsed / duration;
                    onProgress(t);

                    elapsed += Time.deltaTime;
                }

                onProgress(1.0f);
                onComplete?.Invoke();
            }
            finally
            {
                fadeCTS.Dispose();
                fadeCTS = null;
            }
        }

        public void InterruptFade()
        {
            if (fadeCTS == null) return;
            CancelFade();

            switch (State)
            {
                case BGMState.FadeIn:
                case BGMState.FadeOut:
                    State = BGMState.Play;
                    break;

                case BGMState.CrossFade:
                    HandleCrossFadeInterrupt();
                    State = BGMState.Play;
                    break;
            }
        }

        private void HandleCrossFadeInterrupt()
        {
            if (pendingResourceAddress == null) return;

            //音量が大きい方の再生を続行する
            if (bgmSources.inactive.volume >= bgmSources.active.volume)
            {
                bgmSources.active.Stop();
                bgmSources = (bgmSources.inactive, bgmSources.active);

                cache.EndUse(usageResourceAddress);
                usageResourceAddress = pendingResourceAddress;
            }
            else
            {
                bgmSources.inactive.Stop();
                cache.EndUse(pendingResourceAddress);
            }

            pendingResourceAddress = null;
        }

        public void Dispose()
        {
            CancelFade();
            fadeCTS?.Dispose();
            fadeCTS                = null;
            bgmSources             = (null, null);
            pendingResourceAddress = null;

            cache.EndUse(usageResourceAddress);
            usageResourceAddress = null;

            if (sourceRoot != null) UnityEngine.Object.Destroy(sourceRoot);
        }

        private void CancelFade() { fadeCTS?.Cancel(); }
    }
}
