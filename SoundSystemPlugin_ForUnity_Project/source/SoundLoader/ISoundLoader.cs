namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    
    public interface ISoundLoader
    {
        UniTask<(bool success, AudioClip clip)> TryLoadClip(string resourceAddress);

        UniTask<(bool success, AudioClip clip)> LoadClipInternal(string resourceAddress);

        void UnloadClip(AudioClip clip);
    }
}
