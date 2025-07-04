namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// サウンドリソースのロードを担うクラス<para></para>
    ///  - HTTP ストリーミングで指定 URL から AudioClip を読込
    /// </summary>
    internal class SoundLoader_Streaming : SoundLoader_Base
    {
        public SoundLoader_Streaming(ISoundCache cache) : base(cache) { }

        public override async UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress)
        {
            using var request = UnityWebRequestMultimedia.GetAudioClip(resourceAddress, AudioType.UNKNOWN);
            var operation = request.SendWebRequest();
            await operation;

            if (request.result != UnityWebRequest.Result.Success) return (false, null);

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null) return (true, clip);
            else              return (false, null);
        }

        public override void UnloadClip(AudioClip clip)
        {
            if (clip != null) Object.Destroy(clip);
        }
    }
}
