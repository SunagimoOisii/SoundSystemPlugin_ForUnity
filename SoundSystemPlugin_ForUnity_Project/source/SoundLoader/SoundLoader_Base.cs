namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// サウンドリソースをロードするクラスの共通基底クラス
    /// </summary>
    internal abstract class SoundLoader_Base : ISoundLoader
    {
        protected readonly ISoundCache cache;

        protected SoundLoader_Base(ISoundCache cache)
        {
            this.cache = cache;
            if (cache is SoundCache baseCache)
            {
                baseCache.SetLoader(this);
            }
        }

        public async UniTask<(bool success, AudioClip clip)> TryLoadClip(string resourceAddress)
        {
            Log.Safe($"LoadClip実行:{resourceAddress}");

            if (resourceAddress == null)
            {
                Log.Warn($"LoadClip失敗:{nameof(resourceAddress)}がnull");
                return (false, null);
            }

            var cached = cache.Retrieve(resourceAddress);
            if (cached != null)
            {
                Log.Safe($"LoadClip成功:CacheHit,{resourceAddress}");
                return (true, cached);
            }

            var (success, clip) = await LoadClipInternal(resourceAddress);

            if (success && 
                clip != null)
            {
                cache.Add(resourceAddress, clip);
                Log.Safe($"LoadClip成功:{resourceAddress}");
                return (true, clip);
            }
            else
            {
                Log.Error($"LoadClip失敗:{resourceAddress}");
                cache.Remove(resourceAddress);
                return (false, null);
            }
        }

        public abstract UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress);

        public abstract void UnloadClip(AudioClip clip);
    }
}
