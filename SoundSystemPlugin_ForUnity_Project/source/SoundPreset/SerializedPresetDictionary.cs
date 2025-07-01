namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// SoundPresetPropertyで使用されるBGMプリセット群を保持,操作するクラス<para/>
    /// - インスペクターでは編集可能なListとして管理される<para/>
    /// - 実行時には、presetNameをキーとするDictionaryへ変換し、高速な参照が可能<para/>
    /// - Dictionaryへの変換はISerializationCallbackReceiver.OnAfterDeserialize()内で行う
    /// BGMプリセット群を保持するクラス
    /// </summary>
    [System.Serializable]
    public abstract class SerializedPresetDictionary<TPreset> : ISerializationCallbackReceiver
    {
        [SerializeField] private List<TPreset>  presetList = new();
        public IReadOnlyList<TPreset> Presets => presetList;
        private readonly Dictionary<string, TPreset> presetDict = new();

        /// <summary>
        /// キーを指定してプリセットを取得する
        /// </summary>
        public bool TryGetValue(string key, out TPreset value)
        {
            return presetDict.TryGetValue(key, out value);
        }

        protected abstract string GetPresetName(TPreset preset);

        //明示的実装で関数を隠蔽
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            //List を Dictionary 化
            presetDict.Clear();
            foreach (var preset in presetList)
            {
                string name = GetPresetName(preset);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (presetDict.ContainsKey(name))
                {
                    Debug.LogWarning($"キーの重複:presetName = {name}");
                    continue;
                }

                presetDict.Add(name, preset);
            }
        }

        //処理ナシ
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
