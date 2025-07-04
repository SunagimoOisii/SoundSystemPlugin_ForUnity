namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// サウンドリソースのロードを担うクラス<para></para>
    ///  - HTTP ストリーミングで指定 URL から AudioClip を読込
    /// </summary>
    public class SoundLoader_Streaming : SoundLoader_Base
    {
        public SoundLoader_Streaming(ISoundCache cache) : base(cache) { }

        public override async UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress)
        {
            using var request = UnityWebRequestMultimedia.GetAudioClip(resourceAddress, AudioType.UNKNOWN);
            var operation = request.SendWebRequest();
            await operation;

            bool hasError = request.result != UnityWebRequest.Result.Success;
            if (hasError)
            {
                Log.Error($"LoadClip失敗:{resourceAddress},Error = {request.error}");
                return (false, null);
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null)
            {
                return (true, clip);
            }
            else
            {
                Log.Error($"LoadClip失敗:{resourceAddress},ClipNull");
                return (false, null);
            }
        }

        public override void UnloadClip(AudioClip clip)
        {
            if (clip != null) Object.Destroy(clip);
        }
    }
}
