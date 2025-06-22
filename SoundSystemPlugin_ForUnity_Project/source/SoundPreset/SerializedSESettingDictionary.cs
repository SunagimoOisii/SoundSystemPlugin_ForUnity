namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// SoundPresetPropertyŎgpSEvZbgQێ,삷NX<para/>
    /// - CXyN^[ł͕ҏW\ListƂĊǗ<para/>
    /// - sɂ́ApresetNameL[ƂDictionary֕ϊAȎQƂ\<para/>
    /// - Dictionaryւ̕ϊISerializationCallbackReceiver.OnAfterDeserialize()ōs
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
                    Debug.LogWarning($"L[̏d:key = {preset.presetName}");
                    continue;
                }
    
                presetDict.Add(preset.presetName, preset);
            }
        }
    
        //iV
        public void OnBeforeSerialize() { }
    }
}
