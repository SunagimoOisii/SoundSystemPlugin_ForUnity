namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// サウンドリソースのロードを担うクラス<para></para>
    ///  - HTTP ストリーミングで指定 URL から AudioClip を読込
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

            if (resourceAddress == null)
            {
                Log.Warn($"LoadClip失敗:{nameof(resourceAddress)}がnull");
                return (false, null);
            }

            //キャッシュを参照し、既に存在する場合はそれを返す
            var cached = cache.Retrieve(resourceAddress);
            if (cached != null)
            {
                Log.Safe($"LoadClip成功:CacheHit,{resourceAddress}");
                return (true, cached);
            }

            using var request = UnityWebRequestMultimedia.GetAudioClip(resourceAddress, AudioType.UNKNOWN);
            var operation = request.SendWebRequest();
            await operation;

            bool hasError = request.result != UnityWebRequest.Result.Success;
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

        public void UnloadClip(AudioClip clip)
        {
            if (clip != null) Object.Destroy(clip);
        }
    }
}
