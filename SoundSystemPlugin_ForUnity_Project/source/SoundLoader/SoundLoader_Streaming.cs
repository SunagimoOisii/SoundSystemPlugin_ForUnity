namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// URL から AudioClip を読み込むローダー
    /// </summary>
    public class SoundLoader_Streaming : ISoundLoader
    {
        private readonly ISoundCache cache;

        public SoundLoader_Streaming(ISoundCache cache)
        {
            this.cache = cache;
            if (cache is SoundCache_Base baseCache)
            {
                baseCache.SetLoader(this);
            }
        }

        public UniTask<(bool success, AudioClip clip)> PreloadClip(string resourceAddress)
            => LoadClipInternal(resourceAddress);

        public UniTask<(bool success, AudioClip clip)> TryLoadClip(string resourceAddress)
            => LoadClipInternal(resourceAddress);

        public async UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress)
        {
            Log.Safe($"LoadClip実行:{resourceAddress}");

            var cached = cache.Retrieve(resourceAddress);
            if (cached != null)
            {
                Log.Safe($"LoadClip成功:CacheHit,{resourceAddress}");
                return (true, cached);
            }

            using var request = UnityWebRequestMultimedia.GetAudioClip(resourceAddress, AudioType.UNKNOWN);
            var operation = request.SendWebRequest();
            await operation;

#if UNITY_2020_2_OR_NEWER
            bool hasError = request.result != UnityWebRequest.Result.Success;
#else
            bool hasError = request.isNetworkError || request.isHttpError;
#endif
            if (hasError)
            {
                Log.Error($"LoadClip失敗:{resourceAddress},Error = {request.error}");
                cache.Remove(resourceAddress);
                return (false, null);
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null)
            {
                cache.Add(resourceAddress, clip);
                Log.Safe($"LoadClip成功:{resourceAddress}");
                return (true, clip);
            }
            else
            {
                Log.Error($"LoadClip失敗:{resourceAddress},ClipNull");
                cache.Remove(resourceAddress);
                return (false, null);
            }
        }

        public void ReleaseClip(AudioClip clip)
        {
            if (clip != null)
            {
                Object.Destroy(clip);
            }
        }
    }
}
