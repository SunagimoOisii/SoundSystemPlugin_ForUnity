namespace SoundSystem
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Audio;
    using System.Threading;
    
    /// <summary>
    /// BGM̍ĐA~AtF[h@\񋟂NX<para></para>
    /// </summary>
    internal sealed class BGMManager
    {
        private readonly ISoundLoader loader;
    
        private readonly GameObject sourceRoot = null;
        private (AudioSource active, AudioSource inactive) bgmSources;
    
        private CancellationTokenSource fadeCTS;
        private string currentBGMAddress;
    
        private enum BGMState
        {
            Idle,
            Play,
            Pause,
            FadeIn,
            FadeOut,
            CrossFade
        }
        private BGMState State { get; set; } = BGMState.Idle;
    
        /// <param name="mixerGroup">BGMo͐AudioMixerGroup</param>
        public BGMManager(AudioMixerGroup mixerGroup, ISoundLoader loader)
        {
            this.loader = loader;
    
            //BGMpAudioSourceƂꂪA^b`ꂽGameObject쐬
            sourceRoot = new("BGM_AudioSources");
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
                source.loop                  = true;
                source.playOnAwake           = false;
                source.outputAudioMixerGroup = mixerGroup;
                return source;
            }
        }
    
        /// <param name="volume">(͈: 0.0`1.0)</param>
        public async UniTask Play(string resourceAddress, float volume)
        {
            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"Plays:\[XǍɎs,{resourceAddress}");
                return;
            }
    
            State = BGMState.Play;
            bgmSources.active.clip   = clip;
            bgmSources.active.volume = volume;
            bgmSources.active.Play();
            Log.Safe($"Play:{resourceAddress},vol = {volume}");
        }
    
        public void Stop()
        {
            Log.Safe("Stops");
            State = BGMState.Idle;
            bgmSources.active.Stop();
            bgmSources.active.clip = null;
        }
    
        public void Resume()
        {
            Log.Safe("Resumes");
    
            if (State != BGMState.Pause)
            {
                Log.Warn($"Resumef:PauseXe[gȊOł͎ss");
                return;
            }
    
            State = BGMState.Play;
            bgmSources.active.UnPause();
        }
    
        public void Pause()
        {
            Log.Safe($"Pauses");
            State = BGMState.Pause;
            bgmSources.active.Pause();
        }
    
        /// <param name="volume">ڕW(͈: 0.0`1.0)</param>
        public async UniTask FadeIn(string resourceAddress, float duration, float volume)
        {
            Log.Safe($"FadeIns:{resourceAddress},dura = {duration},vol = {volume}");
    
            if (State == BGMState.Play || 
                State == BGMState.CrossFade)
            {
                Log.Warn($"FadeInf:Xe[g̕sv,State = {State}");
                return;
            }
    
            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"FadeIns:\[XǍɎs,{resourceAddress}");
                return;
            }
            bgmSources.active.clip   = clip;
            bgmSources.active.volume = 0;
            bgmSources.active.Play();
    
            State = BGMState.FadeIn;
            await ExecuteVolumeTransition(
                duration,
                progressRate => bgmSources.active.volume = Mathf.Lerp(0f, volume, progressRate)
                );
            Log.Safe($"FadeInI:{resourceAddress},dura = {duration},vol = {volume}");
        }
    
        public async UniTask FadeOut(float duration)
        {
            Log.Safe($"FadeOuts:dura = {duration}");
    
            if (State != BGMState.Play)
            {
                Log.Warn($"FadeOutf:Xe[g̕sv,State = {State}");
                return;
            }
            State = BGMState.FadeOut;
    
            float startVol = bgmSources.active.volume;
            await ExecuteVolumeTransition(
                duration,
                progressRate => bgmSources.active.volume = Mathf.Lerp(startVol, 0.0f, progressRate)
                );
    
            bgmSources.active.Stop();
            bgmSources.active.clip = null;
            Log.Safe($"FadeOutI:dura = {duration}");
        }
    
        public async UniTask CrossFade(string resourceAddress, float duration)
        {
            Log.Safe($"CrossFades:{resourceAddress}");
    
            if (resourceAddress == currentBGMAddress)
            {
                Log.Warn($"CrossFadef:BGM {resourceAddress} w肳ꂽߒf");
                State = BGMState.Idle;
                return;
            }
    
            var (success, clip) = await loader.TryLoadClip(resourceAddress);
            if (success == false)
            {
                Log.Error($"CrossFades:\[XǍɎs,{resourceAddress}");
                return;
            }
            bgmSources.inactive.clip   = clip;
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
                    bgmSources.active.Stop();
                    bgmSources = (bgmSources.inactive, bgmSources.active);
                    currentBGMAddress = resourceAddress;
                });
            Log.Safe($"CrossFadeI:{resourceAddress}");
        }
    
        private async UniTask ExecuteVolumeTransition(float duration, Action<float> onProgress,
            Action onComplete = null)
        {
            //ɃtF[hsĂꍇ͏㏑
            fadeCTS?.Cancel();
            fadeCTS?.Dispose();
            fadeCTS = new();
            var token = fadeCTS.Token;
    
            try //tF[hs
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    if (token.IsCancellationRequested) return;
    
                    float t = elapsed / duration;
                    onProgress(t);
    
                    elapsed += Time.deltaTime;
                    await UniTask.Yield();
                }
    
                onProgress(1.0f);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Log.Safe("ExecuteVolumeTransitionf:OperationCanceledException");
            }
            finally
            {
                State = BGMState.Idle;
            }
        }
    }
}
