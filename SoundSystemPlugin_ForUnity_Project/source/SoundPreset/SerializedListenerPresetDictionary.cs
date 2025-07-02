namespace SoundSystem
{
    /// <summary>
    /// Listenerエフェクトプリセット群を保持するクラス
    /// </summary>
    [System.Serializable]
    public sealed class SerializedListenerPresetDictionary : SerializedPresetDictionary<SoundPresetProperty.ListenerEffectPreset>
    {
        protected override string GetPresetName(SoundPresetProperty.ListenerEffectPreset preset)
        {
            return preset.presetName;
        }
    }
}
