namespace SoundSystem
{
    /// <summary>
    /// SEプリセット群を保持するクラス
    /// </summary>
    [System.Serializable]
    public sealed class SerializedSEPresetDictionary : SerializedPresetDictionary<SoundPresetProperty.SEPreset>
    {
        protected override string GetPresetName(SoundPresetProperty.SEPreset preset)
        {
            return preset.presetName;
        }
    }
}
