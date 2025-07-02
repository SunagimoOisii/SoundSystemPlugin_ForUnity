namespace SoundSystem
{
    using UnityEngine;
    /// <summary>
    /// AudioListener へ適用するエフェクトプリセット
    /// </summary>
    [System.Serializable]
    public struct ListenerEffectPreset
    {
        public string presetName;
        public FilterKind kind;
        [UnityEngine.SerializeReference] public ListenerFilterSettings settings;

        public void ApplyTo(ListenerEffector effector)
        {
            settings?.Apply(effector);
        }
    }
}
