namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// サウンドリソースのロードを担うクラス<para></para>
    /// - Resourcesを介してAudioClipを非同期にロード<para></para>
    /// - ロード結果をキャッシュ管理クラス(ISoundCache)に委譲
    /// </summary>
    public class SoundLoader_Resources : SoundLoader_Base
    {
        public SoundLoader_Resources(ISoundCache cache) : base(cache) { }

        public override async UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress)
        {
            var request = Resources.LoadAsync<AudioClip>(resourceAddress);
            await request;
            var clip = request.asset as AudioClip;
            return (clip != null, clip);
        }

        public override void UnloadClip(AudioClip clip)
        {
            if (clip != null) Resources.UnloadAsset(clip);
        }
    }
}
