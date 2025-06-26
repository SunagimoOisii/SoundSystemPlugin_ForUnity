namespace SoundSystem
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Audio;

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

        private readonly GameObject sourceRoot = null;
        private (AudioSource active, AudioSource inactive) bgmSources;
        private (AudioSourceFader active, AudioSourceFader inactive) faders;

        /// <param name="mixerGroup">BGM出力先のAudioMixerGroup</param>
        public BGMManager(AudioMixerGroup mixerGroup, ISoundLoader loader, bool persistent = false)
        {
            this.loader = loader;

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
            faders = (new AudioSourceFader(bgmSources.active),
                      new AudioSourceFader(bgmSources.inactive));

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

            State = BGMState.Play;
            bgmSources.active.clip = clip;
            bgmSources.active.volume = volume;
            bgmSources.active.Play();
            onComplete?.Invoke();

            Log.Safe($"Play成功:{resourceAddress},vol = {volume}");
        }

        public void Stop()
        {
            Log.Safe("Stop実行");

            State = BGMState.Idle;
            bgmSources.active.Stop();
            bgmSources.active.clip = null;
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
            bgmSources.active.clip = clip;
            bgmSources.active.volume = 0;
            bgmSources.active.Play();

            State = BGMState.FadeIn;
            await faders.active.Fade(0f, volume, duration);
            State = BGMState.Play;
            onComplete?.Invoke();

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
            State = BGMState.FadeOut;

            float startVol = bgmSources.active.volume;
            await faders.active.Fade(startVol, 0.0f, duration);

            State = BGMState.Idle;
            bgmSources.active.Stop();
            bgmSources.active.clip = null;
            onComplete?.Invoke();

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
            State = BGMState.CrossFade;
            bgmSources.inactive.clip = clip;
            bgmSources.inactive.volume = 0f;
            bgmSources.inactive.Play();

            await UniTask.WhenAll(
                faders.active.Fade(1f, 0f, duration),
                faders.inactive.Fade(0f, 1f, duration)
            );
            bgmSources.active.Stop();
            bgmSources = (bgmSources.inactive, bgmSources.active);
            faders = (faders.inactive, faders.active);
            onComplete?.Invoke();
            State = BGMState.Play;

            Log.Safe($"CrossFade終了:{resourceAddress}");
        }

        public void Dispose()
        {
            faders.active.Dispose();
            faders.inactive.Dispose();

            if (sourceRoot != null)
            {
                UnityEngine.Object.Destroy(sourceRoot);
            }

            bgmSources = (null, null);
        }
    }
}
