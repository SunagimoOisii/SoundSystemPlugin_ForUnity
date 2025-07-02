namespace SoundSystem
{
    using UnityEngine;

    /// <summary>
    /// AudioListener へ適用するエフェクトプリセット
    /// </summary>
    [System.Serializable]
    public sealed class ListenerEffectPreset
    {
        public string presetName;
        public FilterKind kind;
        [SerializeReference] public ListenerFilterSettings settings;

        public void ApplyTo(ListenerEffector effector)
        {
            settings?.Apply(effector);
        }
    }
}
