namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// SoundPresetPropertyŎgpBGMvZbgQێ,삷NX<para/>
    /// - CXyN^[ł͕ҏW\ListƂĊǗ<para/>
    /// - sɂ́ApresetNameL[ƂDictionary֕ϊAȎQƂ\<para/>
    /// - Dictionaryւ̕ϊISerializationCallbackReceiver.OnAfterDeserialize()ōs
    /// </summary>
    [System.Serializable]
    public class SerializedBGMSettingDictionary : ISerializationCallbackReceiver
    {
    	[SerializeField]
    	private List<SoundPresetProperty.BGMPreset> presetList = new();
    
    	private Dictionary<string, SoundPresetProperty.BGMPreset> presetDict = new();
    
    	public bool TryGetValue(string key, out SoundPresetProperty.BGMPreset value)
    	{
    		return presetDict.TryGetValue(key, out value);
    	}
    
    	void ISerializationCallbackReceiver.OnAfterDeserialize()
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
    				Debug.LogWarning($"L[̏d:presetName = {preset.presetName}");
    				continue;
    			}
    
    			presetDict.Add(preset.presetName, preset);
    		}
    	}
    
    	//iV
    	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
