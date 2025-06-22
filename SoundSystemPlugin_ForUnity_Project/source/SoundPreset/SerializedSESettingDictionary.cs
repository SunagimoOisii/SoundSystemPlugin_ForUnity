namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// SoundPresetPropertyで使用されるSEプリセット群を保持,操作するクラス<para/>
    /// - インスペクターでは編集可能なListとして管理される<para/>
    /// - 実行時には、presetNameをキーとするDictionaryへ変換し、高速な参照が可能<para/>Add commentMore actions
    /// - Dictionaryへの変換はISerializationCallbackReceiver.OnAfterDeserialize()内で行う
    /// </summary>
    [System.Serializable]
    public class SerializedSESettingDictionary : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SoundPresetProperty.SEPreset> presetList = new();
    
        private Dictionary<string, SoundPresetProperty.SEPreset> presetDict = new();
    
        public bool TryGetValue(string key, out SoundPresetProperty.SEPreset value)
        {
            return presetDict.TryGetValue(key, out value);
        }
    
        public void OnAfterDeserialize()
        {
            presetDict.Clear();
            foreach (var preset in presetList)
            {
                if (string.IsNullOrEmpty(preset.presetName))
                {
                    continue;
                }
    
                if (presetDict.ContainsKey(preset.presetName))
                {
                    Debug.LogWarning($"キーの重複:key = {preset.presetName}");
                    continue;
                }
    
                presetDict.Add(preset.presetName, preset);
            }
        }
    
        //処理ナシ
        public void OnBeforeSerialize() { }
    }
}
