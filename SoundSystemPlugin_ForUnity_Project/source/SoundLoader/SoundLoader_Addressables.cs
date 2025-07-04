#if USE_ADDRESSABLES
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
    public class SoundLoader_Addressables : SoundLoader_Base
    {
        public SoundLoader_Addressables(ISoundCache cache) : base(cache) { }

        public override async UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(resourceAddress);
            var clip   = await handle.Task;

            if (clip != null && handle.Status == AsyncOperationStatus.Succeeded)
            {
                return (true, clip);
            }
            else
            {
                Log.Error($"LoadClip失敗:{resourceAddress},Status = {handle.Status}");
                Addressables.Release(handle);
                return (false, null);
            }
        }
    
        public override void UnloadClip(AudioClip clip)
        {
            if (clip != null) Addressables.Release(clip);
        }
    }
}
#endif
