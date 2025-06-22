#if SOUND_USE_ADDRESSABLES
namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    
    /// <summary>
    /// サウンドリソースのロードを担うクラス<para></para>
    /// - Addressableを介してAudioClipを非同期にロード<para></para>
    /// - ロード結果をキャッシュ管理クラス(ISoundCache)に委譲
    /// </summary>
    public class SoundLoader_Addressables : ISoundLoader
    {
        private readonly ISoundCache cache;

        public SoundLoader_Addressables(ISoundCache cache)
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
            var handle = Addressables.LoadAssetAsync<AudioClip>(resourceAddress);
            var clip   = await handle.Task;
    
            if (clip != null &&
                handle.Status == AsyncOperationStatus.Succeeded)
            {
                cache.Add(resourceAddress, clip);
                Log.Safe($"TryLoadClip成功:{resourceAddress}");
                return (success: true, clip);
            }
            else
            {
                Log.Error($"TryLoadClip失敗:{resourceAddress},Status = {handle.Status}");
                cache.Remove(resourceAddress);
                Addressables.Release(handle);
                return (success: false, null);
            }
        }
    
        public void ReleaseClip(AudioClip clip)
        {
            if (clip != null)
            {
                Addressables.Release(clip);
            }
        }
    }
}
#endif
