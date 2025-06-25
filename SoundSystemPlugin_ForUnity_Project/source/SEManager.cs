namespace SoundSystem
{
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using System;

    /// <summary>
    /// SoundSystemが操作するクラスの1つ<para></para>
    /// - AudioSourcePoolを用いたSE再生<para></para>
    /// </summary>
    internal sealed class SEManager : IDisposable
    {
        private readonly IAudioSourcePool sourcePool;
        private readonly ISoundLoader loader;
        private readonly ISoundCache cache;
    
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
            cache.BeginUse(resourceAddress);

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
            cache.EndUse(resourceAddress);
            onComplete?.Invoke();

            Log.Safe($"Play成功:{resourceAddress},vol = {volume},pitch = {pitch}," +
                $"blend = {spatialBlend}");
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
