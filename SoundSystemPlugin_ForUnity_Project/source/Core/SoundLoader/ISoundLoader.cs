namespace SoundSystem
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    
    public interface ISoundLoader
    {
        UniTask<(bool success, AudioClip clip)> PreloadClip(string resourceAddress);

        UniTask<(bool success, AudioClip clip)> TryLoadClip(string resourceAddress);

        UniTask<(bool success, AudioClip clip)> LoadClip(string resourceAddress);

        void ReleaseClip(AudioClip clip);
    }
}
