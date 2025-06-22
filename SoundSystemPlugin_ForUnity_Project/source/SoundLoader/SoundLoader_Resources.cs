namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// サウンドリソースのロードを担うクラス<para></para>
    /// - Resourcesを介してAudioClipを非同期にロード<para></para>
    /// - ロード結果をキャッシュ管理クラス(ISoundCache)に委譲
    public class SoundLoader_Resources : ISoundLoader
    {
        private readonly ISoundCache cache;

        public SoundLoader_Resources(ISoundCache cache)
        {
            this.cache = cache;
            if (cache is SoundCache_Base baseCache)
            {
                baseCache.SetLoader(this);
            }
        }

        public async UniTask<(bool success, AudioClip clip)> TryLoadClip(string resourceAddress)
        {
            Log.Safe($"TryLoadClip実行:{resourceAddress}");

            // キャッシュを参照し、既に存在する場合はそれを返す
            var cached = cache.Retrieve(resourceAddress);
            if (cached != null)
            {
                Log.Safe($"TryLoadClip成功:CacheHit,{resourceAddress}");
                return (true, cached);
            }

            var request = Resources.LoadAsync<AudioClip>(resourceAddress);
            await request;
            var clip = request.asset as AudioClip;
            if (clip != null)
            {
                cache.Add(resourceAddress, clip);
                Log.Safe($"TryLoadClip成功:{resourceAddress}");
                return (true, clip);
            }
            else
            {
                Log.Error($"TryLoadClip失敗:{resourceAddress}");
                cache.Remove(resourceAddress);
                return (false, null);
            }
        }

        public void ReleaseClip(AudioClip clip)
        {
            if (clip != null)
            {
                Resources.UnloadAsset(clip);
            }
        }
    }
}
