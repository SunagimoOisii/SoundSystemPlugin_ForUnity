namespace SoundSystem
{
    using UnityEngine;

    /// <summary>
    /// AudioListener用フィルター設定の基底クラス
    /// </summary>
    internal abstract class ListenerFilterSettings
    {
        public abstract void Apply(ListenerEffector effector);
    }

    [System.Serializable]
    internal sealed class ChorusFilterSettings : ListenerFilterSettings
    {
        [Range(0f, 1f)] public float dryMix  = 0.5f;
        [Range(0f, 1f)] public float wetMix1 = 0.5f;
        [Range(0f, 1f)] public float wetMix2 = 0.5f;
        [Range(0f, 1f)] public float wetMix3 = 0.5f;
        [Range(0f, 100f)] public float delay = 40f;
        [Range(0f, 20f)] public float rate   = 0.8f;
        [Range(0f, 1f)] public float depth   = 0.03f;

        public override void Apply(ListenerEffector effector)
        {
            effector.ApplyFilter<AudioChorusFilter>(f =>
            {
                f.dryMix  = dryMix;
                f.wetMix1 = wetMix1;
                f.wetMix2 = wetMix2;
                f.wetMix3 = wetMix3;
                f.delay   = delay;
                f.rate    = rate;
                f.depth   = depth;
            });
        }
    }

    [System.Serializable]
    internal sealed class DistortionFilterSettings : ListenerFilterSettings
    {
        [Range(0f, 1f)] public float distortionLevel = 0f;

        public override void Apply(ListenerEffector effector)
        {
            effector.ApplyFilter<AudioDistortionFilter>(f =>
            {
                f.distortionLevel = distortionLevel;
            });
        }
    }

    [System.Serializable]
    internal sealed class EchoFilterSettings : ListenerFilterSettings
    {
        [Range(10f, 5000f)] public float delay  = 500f;
        [Range(0f, 1f)] public float decayRatio = 0.5f;
        [Range(0f, 1f)] public float dryMix     = 1f;
        [Range(0f, 1f)] public float wetMix     = 1f;

        public override void Apply(ListenerEffector effector)
        {
            effector.ApplyFilter<AudioEchoFilter>(f =>
            {
                f.delay      = delay;
                f.decayRatio = decayRatio;
                f.dryMix     = dryMix;
                f.wetMix     = wetMix;
            });
        }
    }

    [System.Serializable]
    internal sealed class HighPassFilterSettings : ListenerFilterSettings
    {
        [Range(10f, 22000f)] public float cutoffFrequency = 5000f;
        [Range(1f, 10f)]     public float resonanceQ      = 1f;

        public override void Apply(ListenerEffector effector)
        {
            effector.ApplyFilter<AudioHighPassFilter>(f =>
            {
                f.cutoffFrequency    = cutoffFrequency;
                f.highpassResonanceQ = resonanceQ;
            });
        }
    }

    [System.Serializable]
    internal sealed class LowPassFilterSettings : ListenerFilterSettings
    {
        [Range(10f, 22000f)] public float cutoffFrequency = 5000f;
        [Range(1f, 10f)]     public float resonanceQ      = 1f;

        public override void Apply(ListenerEffector effector)
        {
            effector.ApplyFilter<AudioLowPassFilter>(f =>
            {
                f.cutoffFrequency   = cutoffFrequency;
                f.lowpassResonanceQ = resonanceQ;
            });
        }
    }

    [System.Serializable]
    internal sealed class ReverbFilterSettings : ListenerFilterSettings
    {
        public AudioReverbPreset reverbPreset = AudioReverbPreset.Off;

        public override void Apply(ListenerEffector effector)
        {
            effector.ApplyFilter<AudioReverbFilter>(f =>
            {
                f.reverbPreset = reverbPreset;
            });
        }
    }
}
