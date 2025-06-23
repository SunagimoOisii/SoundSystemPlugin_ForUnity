namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using System;
    using UnityEngine;
    using UnityEngine.Audio;
    
    /// <summary>
    /// サウンド管理のエントリーポイントを提供するクラス<para></para>
    /// - 各マネージャと機能インターフェースを外部から受け取り統一的に管理<para></para>
    /// - 本モジュールに同梱のAudioMixerオブジェクトの使用を前提としている
    /// </summary>
    public sealed class SoundSystem
    {
        private readonly BGMManager bgm;
        private readonly SEManager  se;
        private readonly ListenerEffector effector;
    
        private readonly AudioMixer mixer;
    
        private SerializedBGMSettingDictionary bgmPresets;
        private SerializedSESettingDictionary  sePresets;

        public SoundSystem(ISoundLoader loader, IAudioSourcePool pool,
            AudioListener listener, AudioMixer mixer, AudioMixerGroup bgmGroup,
            bool canLogging = true)
        {
            if (canLogging)
            {
                Log.Initialize();
                Application.quitting += () => Log.Close();
            }
    
            bgm        = new(bgmGroup, loader);
            se         = new(pool, loader);
            effector   = new(listener);
            this.mixer = mixer;
        }

        public static SoundSystem CreateFromPreset(SoundPresetProperty preset, 
            AudioListener listener, AudioMixer mixer, bool canLogging = true)
        {
            var cache  = SoundCacheFactory.Create(preset.cacheType, preset.param);
            var loader = SoundLoaderFactory.Create(cache, preset.loaderType);
            var pool   = AudioSourcePoolFactory.Create(preset.seMixerG, preset.initSize, preset.maxSize);
            var ss     = new SoundSystem(loader, pool, listener, mixer, preset.bgmMixerG, canLogging);
            ss.SetPresets(preset.bgmPresets, preset.sePresets);
            return ss;
        }
    
        //CreateFromPreset関数のために実装
        private void SetPresets(SerializedBGMSettingDictionary bgmList,
            SerializedSESettingDictionary seList)
        {
            bgmPresets = bgmList;
            sePresets  = seList;
        }
    
        public float? RetrieveMixerParameter(string exposedParamName)
        {
            if (mixer.GetFloat(exposedParamName, out float value))
            {
                return value;
            }
            else
            {
                Debug.LogWarning($"SoundSystem: パラメータ '{exposedParamName}' の取得に失敗");
                return null;
            }
        }
    
        public void SetMixerParameter(string exposedParamName, float value)
        {
            if (mixer.SetFloat(exposedParamName, value) == false)
            {
                Debug.LogWarning($"SoundSystem: パラメータ '{exposedParamName}' の設定に失敗");
            }
        }
    
        private bool TryRetrieveBGMPreset(string presetName, out SoundPresetProperty.BGMPreset preset)
        {
            return bgmPresets.TryGetValue(presetName, out preset);
        }
    
        private bool TryRetrieveSEPreset(string presetName, out SoundPresetProperty.SEPreset preset)
        {
            return sePresets.TryGetValue(presetName, out preset);
        }
    
        public void MuteAllSound()
        {
            bgm.Stop();
            se.StopAll();
        }
    
        #region BGM
        public async UniTask PlayBGM(string resourceAddress, float volume = 0.5f)
        {
            await bgm.Play(resourceAddress, volume);
        }
    
        public async UniTask PlayBGMWithPreset(string resourceAddress, string presetName)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await FadeInBGM(resourceAddress, preset.fadeInDuration, preset.volume);
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
            float volume = 1.0f)
        {
            await bgm.FadeIn(resourceAddress, duration, volume);
        }
    
        public async UniTask FadeInBGMWithPreset(string resourceAddress, string presetName)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await bgm.FadeIn(resourceAddress, preset.fadeInDuration, preset.volume);
            }
        }
    
        public async UniTask FadeOutBGM(float duration)
        {
            await bgm.FadeOut(duration);
        }
    
        public async UniTask FadeOutBGMWithPreset(string presetName)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await bgm.FadeOut(preset.fadeOutDuration);
            }
        }
    
        public async UniTask CrossFadeBGM(string resourceAddress, float duration)
        {
            await bgm.CrossFade(resourceAddress, duration);
        }
    
        public async UniTask CrossFadeBGMWithPreset(string resourceAddress, string presetName)
        {
            if (TryRetrieveBGMPreset(presetName, out SoundPresetProperty.BGMPreset preset))
            {
                await bgm.CrossFade(resourceAddress, preset.crossFadeDuration);
            }
        }
        #endregion
    
        #region SE
        public async UniTask PlaySE(string resourceAddress, Vector3 position = default,
            float volume = 0.5f, float pitch = 1.0f, float spatialBlend = 1.0f)
        {
            await se.Play(resourceAddress, volume, pitch, spatialBlend, position);
        }
    
        public async UniTask PlaySEWithPreset(string resourceAddress, string presetName)
        {
            if (TryRetrieveSEPreset(presetName, out SoundPresetProperty.SEPreset preset))
            {
                await se.Play(resourceAddress,
                    preset.volume, preset.pitch, preset.spatialBlend, preset.position);
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
        #endregion
    
        #region ListenerEffector
        public void SetAudioListener(AudioListener l)
        {
            effector.Listener = l;
        }
    
        public void ApplyEffectFilter<T>(Action<T> configure) where T : Behaviour
        {
            effector.ApplyFilter(configure);
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
    }
}
