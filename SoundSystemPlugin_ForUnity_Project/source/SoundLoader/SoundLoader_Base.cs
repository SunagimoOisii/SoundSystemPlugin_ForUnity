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
            if (cache is SoundCache c)
            {
                c.SetLoader(this);
            }
        }

        public async UniTask<(bool success, AudioClip clip)> TryLoadClip(string resourceAddress)
        {
            Log.Safe($"LoadClip実行:{resourceAddress}");

            if (resourceAddress == null ||
                string.IsNullOrWhiteSpace(resourceAddress))
            {
                Log.Warn($"LoadClip失敗:不正なアドレス:{resourceAddress}");
                return (false, null);
            }

            //読込対象が既にキャッシュで存在していれば返す
            var cached = cache.Retrieve(resourceAddress);
            if (cached != null)
            {
                Log.Safe($"LoadClip成功:CacheHit,{resourceAddress}");
                return (true, cached);
            }

            //読込開始(読込方法は派生クラスごとに定義)
            var (success, clip) = await LoadClipInternal(resourceAddress);
            if (success && 
                clip != null)
            {
                Log.Safe($"LoadClip成功:{resourceAddress}");
                cache.Add(resourceAddress, clip);
                return (true, clip);
            }
            else
            {
                Log.Error($"LoadClip失敗:{resourceAddress}");
                return (false, null);
            }
        }

        public abstract UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress);

        public abstract void UnloadClip(AudioClip clip);
    }
}
