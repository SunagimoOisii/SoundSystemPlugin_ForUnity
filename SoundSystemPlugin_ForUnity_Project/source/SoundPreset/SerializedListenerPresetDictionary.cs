namespace SoundSystem
{
    /// <summary>
    /// Listenerエフェクトプリセット群を保持するクラス
    /// </summary>
    [System.Serializable]
    public sealed class SerializedListenerPresetDictionary : SerializedPresetDictionary<ListenerEffectPreset>
    {
        protected override string GetPresetName(ListenerEffectPreset preset)
        {
            return preset.presetName;
        }
    }
}
