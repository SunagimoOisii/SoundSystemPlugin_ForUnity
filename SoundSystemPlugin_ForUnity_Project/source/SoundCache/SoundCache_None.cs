using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem
{
    /// <summary>
    /// サウンドリソースのキャッシュ管理を無効化するクラス
    /// </summary>
    internal sealed class SoundCache_None : ISoundCache
    {
        public int Count => 0;

        public IEnumerable<string> Keys => System.Array.Empty<string>();


        public AudioClip Retrieve(string resourceAddress) { return null; }

        public void Add(string resourceAddress, AudioClip clip) { }

        public void Remove(string resourceAddress) { }

        public void ClearAll() { }

        public void Evict() { }

        public void BeginUse(string resourceAddress) { }

        public void EndUse(string resourceAddress) { }
    }
}
