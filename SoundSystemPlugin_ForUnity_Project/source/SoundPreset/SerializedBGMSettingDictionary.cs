namespace SoundSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// SoundPresetPropertyで使用されるBGMプリセット群を保持,操作するクラス<para/>
    /// - インスペクターでは編集可能なListとして管理される<para/>
    /// - 実行時には、presetNameをキーとするDictionaryへ変換し、高速な参照が可能<para/>
    /// - Dictionaryへの変換はISerializationCallbackReceiver.OnAfterDeserialize()内で行う
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
    				Debug.LogWarning($"キーの重複:presetName = {preset.presetName}");
    				continue;
    			}
    
    			presetDict.Add(preset.presetName, preset);
    		}
    	}
    
    	//処理ナシ
    	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
