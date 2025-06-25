namespace SoundSystem
{
    /// <summary>
    /// BGMプリセット群を保持するクラス
    /// </summary>
    [System.Serializable]
    public sealed class SerializedBGMPresetDictionary : SerializedPresetDictionary<SoundPresetProperty.BGMPreset>
    {
        protected override string GetPresetName(SoundPresetProperty.BGMPreset preset)
        {
            return preset.presetName;
        }
    }
}
