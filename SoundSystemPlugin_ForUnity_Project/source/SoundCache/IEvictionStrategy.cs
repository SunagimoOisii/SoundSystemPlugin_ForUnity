namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// キャッシュ削除方針を表すインターフェース
    /// </summary>
    internal interface IEvictionStrategy
    {
        void OnAdd(string key);

        void OnRetrieve(string key);

        void OnRemove(string key);

        void OnClear();

        IEnumerable<string> SelectKeys(
            IReadOnlyDictionary<string, AudioClip> cache,
            IReadOnlyDictionary<string, int> usageCount);
    }
}
