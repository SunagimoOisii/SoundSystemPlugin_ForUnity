namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using System;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.Audio;
    using UnityEngine.SceneManagement;
    
    /// <summary>
    /// サウンド管理のエントリーポイントを提供するクラス<para></para>
    /// - 各マネージャと機能インターフェースを外部から受け取り統一的に管理<para></para>
    /// - 本モジュールに同梱のAudioMixerオブジェクトの使用を前提としている
    /// </summary>
    public sealed class SoundSystem : IDisposable
    {
        private SerializedBGMPresetDictionary      bgmPresets;
        private SerializedSEPresetDictionary       sePresets;
        private SerializedListenerPresetDictionary listenerPresets;

        private readonly BGMManager bgm;
        private readonly SEManager  se;
        private readonly ListenerEffector effector;

        private readonly AudioMixer mixer;

        private readonly ISoundLoader loader;

        private readonly ISoundCache cache;
        private CancellationTokenSource autoEvictCTS;

        private bool autoDisposeOnSceneChange;

        public SoundSystem(ISoundLoader loader, ISoundCache cache, IAudioSourcePool pool,
            AudioListener listener, AudioMixer mixer, AudioMixerGroup bgmGroup,
            bool persistent = false, bool canLogging = true)
        {
            if (canLogging)
            {
                Log.Initialize();
                Application.quitting += () => Log.Close();
            }
            
            this.mixer  = mixer;
            this.loader = loader;
            this.cache  = cache;
            bgm         = new(bgmGroup, loader, cache, persistent);
            se          = new(pool, loader, cache);
            effector    = new(listener);
        }

        public static SoundSystem CreateFromPreset(SoundPresetProperty preset,
            AudioListener listener, AudioMixer mixer)
        {
            var cache  = SoundCacheFactory.Create(preset.cacheStrategy, preset.param);
            var loader = SoundLoaderFactory.Create(preset.loaderKind, cache);
            var pool   = AudioSourcePoolFactory.Create(preset.poolKind,
                            preset.seMixerG, preset.initSize, preset.maxSize, 
                            preset.isPersistentGameObjects);
            var ss     = new SoundSystem(loader, cache, pool, listener, mixer,
                            preset.bgmMixerG, preset.isPersistentGameObjects, preset.canWriteLog);
            ss.bgmPresets      = preset.bgmPresets;
            ss.sePresets       = preset.sePresets;
            ss.listenerPresets = preset.listenerPresets;
            foreach (var lp in ss.listenerPresets.Presets)
            {
                lp.ApplyTo(ss.effector);
            }
            if (preset.enableAutoEvict) ss.StartAutoEvict(preset.autoEvictIntervalSeconds);
            return ss;
        }
    
        public float? RetrieveMixerParameter(string exposedParamName)
        {
            if (mixer.GetFloat(exposedParamName, out float value))
            {
                return value;
            }
            else
            {
                Log.Warn($"RetrieveMixerParameter失敗:exposedParamName = {exposedParamName}");
                return null;
            }
        }
    
        public void SetMixerParameter(string exposedParamName, float value)
        {
            if (mixer.SetFloat(exposedParamName, value) == false)
            {
                Log.Warn($"SetMixerParameter失敗:exposedParamName = {exposedParamName}");
            }
        }
    
        public void MuteAllSound()
        {
            bgm.Stop();
            se.StopAll();
        }
    
        #region BGM
        public BGMState CurrentBGMState => bgm.State;

        public async UniTask PlayBGM(string resourceAddress, float volume = 0.5f,
            Action onComplete = null)
        {
            await bgm.Play(resourceAddress, volume, onComplete);
        }
    
        public async UniTask PlayBGMWithPreset(string resourceAddress, string presetName,
            Action onComplete = null)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await FadeInBGM(resourceAddress, preset.fadeInDuration, preset.volume,
                    onComplete);
            }
        }
    
        public void StopBGM() 
        { 
            bgm.Stop(); 
        }
    
        public void ResumeBGM()
        {
            bgm.Resume();
        }
    
        public void PauseBGM()
        {
            bgm.Pause();
        }
    
        public async UniTask FadeInBGM(string resourceAddress, float duration,
            float volume = 1.0f, Action onComplete = null)
        {
            await bgm.FadeIn(resourceAddress, duration, volume, onComplete);
        }
    
        public async UniTask FadeInBGMWithPreset(string resourceAddress, string presetName,
            Action onComplete = null)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await bgm.FadeIn(resourceAddress, preset.fadeInDuration, preset.volume,
                    onComplete);
            }
        }
    
        public async UniTask FadeOutBGM(float duration, Action onComplete = null)
        {
            await bgm.FadeOut(duration, onComplete);
        }
    
        public async UniTask FadeOutBGMWithPreset(string presetName, Action onComplete = null)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await bgm.FadeOut(preset.fadeOutDuration, onComplete);
            }
        }
    
        public async UniTask CrossFadeBGM(string resourceAddress, float duration,
            Action onComplete = null)
        {
            await bgm.CrossFade(resourceAddress, duration, onComplete);
        }
    
        public async UniTask CrossFadeBGMWithPreset(string resourceAddress, string presetName,
            Action onComplete = null)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await bgm.CrossFade(resourceAddress, preset.crossFadeDuration, onComplete);
            }
        }

        private bool TryRetrieveBGMPreset(string presetName,
            out SoundPresetProperty.BGMPreset preset)
        {
            return bgmPresets.TryGetValue(presetName, out preset);
        }

        /// <summary>
        /// クロスフェード中断時、音声が大きい方の再生を続行する
        /// </summary>
        public void InterruptBGMFade()
        {
            bgm.InterruptFade();
        }
        #endregion
    
        #region SE

        public async UniTask PlaySE(string resourceAddress, Vector3 position = default,
            float volume = 0.5f, float pitch = 1.0f, float spatialBlend = 1.0f,
            Action onComplete = null)
        {
            await se.Play(resourceAddress, volume, pitch, spatialBlend, position,
                onComplete);
        }
    
        public async UniTask PlaySEWithPreset(string resourceAddress, string presetName,
            Action onComplete = null)
        {
            if (TryRetrieveSEPreset(presetName, out SoundPresetProperty.SEPreset preset))
            {
                await se.Play(resourceAddress,
                    preset.volume, preset.pitch, preset.spatialBlend, preset.position,
                    onComplete);
            }
        }

        public void StopAllSE()
        {
            se.StopAll();
        }
    
        public void ResumeAllSE()
        {
            se.ResumeAll();
        }
    
        public void PauseAllSE()
        {
            se.PauseAll();
        }

        public async UniTask FadeInSE(string resourceAddress, float duration,
            Vector3 position = default, float volume = 1.0f, float pitch = 1.0f,
            float spatialBlend = 1.0f, Action onComplete = null)
        {
            await se.FadeIn(resourceAddress, duration, volume, pitch, spatialBlend,
                position, onComplete);
        }

        public async UniTask FadeInSEWithPreset(string resourceAddress, string presetName,
            Action onComplete = null)
        {
            if (TryRetrieveSEPreset(presetName, out SoundPresetProperty.SEPreset preset))
            {
                await se.FadeIn(resourceAddress, preset.fadeInDuration,
                    preset.volume, preset.pitch, preset.spatialBlend, preset.position,
                    onComplete);
            }
        }

        public async UniTask FadeOutAllSE(float duration, Action onComplete = null)
        {
            await se.FadeOutAll(duration, onComplete);
        }

        private bool TryRetrieveSEPreset(string presetName,
            out SoundPresetProperty.SEPreset preset)
        {
            return sePresets.TryGetValue(presetName, out preset);
        }

        public void InterruptAllSEFade()
        {
            se.InterruptAllFade();
        }
        #endregion
    
        #region ListenerEffector
        public void SetAudioListener(AudioListener newL)
        {
            effector.ChangeListener(newL);
        }
    
        public void ApplyEffectFilter<T>(Action<T> configure) where T : Behaviour
        {
            effector.ApplyFilter(configure);
        }

        public void ApplyEffectFilterFromPreset(string presetName)
        {
            if (listenerPresets.TryGetValue(presetName, out var preset))
            {
                preset.ApplyTo(effector);
            }
        }
    
        public void DisableEffectFilter<T>() where T : Behaviour
        {
            effector.DisableFilter<T>();
        }
    
        public void DisableAllEffectFilter()
        {
            effector.DisableAllFilters();
        }
        #endregion

        #region Loader

        public UniTask<(bool success, AudioClip clip)> PreloadClip(string resourceAddress)
        {
            return loader.TryLoadClip(resourceAddress);
        }

        #endregion

        #region Cache

        public void RemoveCache(string resourceAddress)
        {
            cache.Remove(resourceAddress);
        }

        public void ClearCache()
        {
            cache.ClearAll();
        }

        #endregion

        #region AutoEvict

        public void StartAutoEvict(float intervalSeconds)
        {
            StopAutoEvict();

            autoEvictCTS = new();
            AutoEvictLoop(intervalSeconds, autoEvictCTS.Token).Forget();
        }

        public void StopAutoEvict()
        {
            autoEvictCTS?.Cancel();
            autoEvictCTS?.Dispose();
            autoEvictCTS = null;
        }

        private async UniTask AutoEvictLoop(float intervalSeconds, CancellationToken token)
        {
            while (true)
            {
                var isCancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(intervalSeconds),cancellationToken: token)
                    .SuppressCancellationThrow();
                if (isCancelled)
                {
                    Log.Safe("AutoEvictLoop中断:OperationCanceledException");
                    break;
                }

                Log.Safe($"AutoEvict実行:intervalSeconds = {intervalSeconds}");
                cache.Evict();
            }
        }

        #endregion

        #region AutoDispose

        public void EnableAutoDisposeOnSceneChange()
        {
            if (autoDisposeOnSceneChange) return;

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            autoDisposeOnSceneChange = true;
        }

        public void DisableAutoDisposeOnSceneChange()
        {
            if (autoDisposeOnSceneChange == false) return;

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            autoDisposeOnSceneChange = false;
        }

        private void OnActiveSceneChanged(Scene current, Scene next)
        {
            Dispose();
        }

        #endregion

        public void Dispose()
        {
            DisableAutoDisposeOnSceneChange();
            StopAutoEvict();

            effector.RemoveAllFilters();
            bgm.Dispose();
            se.Dispose();
            cache.ClearAll();
            Log.Close();
        }
    }
}
