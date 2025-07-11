namespace SoundSystem
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// AudioListener へ適用可能なエフェクトフィルターの種類
    /// </summary>
    public enum FilterKind
    {
        Chorus,
        Distortion,
        Echo,
        HighPass,
        LowPass,
        Reverb,
    }

    /// <summary>
    /// SoundSystemが操作するクラスの１つ<para></para>
    /// AudioListenerにエフェクトフィルターを動的に追加し制御を行う
    /// (エフェクトフィルターとはAudioReverbFilterやAudioEchoFilterなどで、
    /// 本クラスではBehaviourクラスを基底型として統一的に扱う)
    /// </summary>
    internal sealed class ListenerEffector
    {
        public AudioListener Listener { private get; set; }

        private readonly Dictionary<Type, Component> filterDict = new();

        public ListenerEffector(AudioListener l)
        {
            Listener = l;
        }

        /// <summary>
        /// 現在のフィルター設定を保持したまま新しい AudioListener に差し替える
        /// </summary>
        public void ChangeListener(AudioListener newL)
        {
            if (Listener == newL) return;

            foreach (var pair in new Dictionary<Type, Component>(filterDict))
            {
                if (pair.Value == null) continue;

                //同じフィルターをアタッチし、その設定も JSON を介してコピー
                var comp = newL.gameObject.AddComponent(pair.Key);
                var json = JsonUtility.ToJson(pair.Value);
                JsonUtility.FromJsonOverwrite(json, comp);
                filterDict[pair.Key] = comp;

                UnityEngine.Object.Destroy(pair.Value);
            }

            Listener = newL;
        }

        /// <typeparam name="FilterT">適用するフィルターの型</typeparam>
        /// <param name="configure">フィルターの設定を行うアクション</param>
        /// <remarks>使用例: effector.ApplyFilter<AudioReverbFilter>(filter => filter.reverbLevel = Mathf.Clamp(reverbLevel, -10000f, 2000f));</remarks>
        public void ApplyFilter<FilterT>(Action<FilterT> configure) where FilterT : Behaviour
        {
            Log.Safe($"ApplyFilter実行:{typeof(FilterT).Name}");
            if (filterDict.TryGetValue(typeof(FilterT), out var component) == false)
            {
                component = Listener.gameObject.AddComponent<FilterT>();
                filterDict[typeof(FilterT)] = component;
            }

            var filter = component as FilterT;
            filter.enabled = true;
            configure?.Invoke(filter);
        }

        public void ApplyFilter(FilterKind kind, Behaviour filterSettings = null)
        {
            var filterClass = GetFilterClass(kind);
            if (filterClass == null) return;

            Log.Safe($"ApplyFilter実行:{filterClass.Name}");
            if (filterDict.TryGetValue(filterClass, out var comp) == false)
            {
                comp = Listener.gameObject.AddComponent(filterClass);
                filterDict[filterClass] = comp;
            }

            //プリセットでのフィルター設定を適用
            if (filterSettings != null)
            {
                var json = JsonUtility.ToJson(filterSettings);
                JsonUtility.FromJsonOverwrite(json, comp);
            }

            if (comp is Behaviour b) b.enabled = true;
        }

        private Type GetFilterClass(FilterKind type)
        {
            return type switch
            {
                FilterKind.Chorus      => typeof(AudioChorusFilter),
                FilterKind.Distortion  => typeof(AudioDistortionFilter),
                FilterKind.Echo        => typeof(AudioEchoFilter),
                FilterKind.HighPass    => typeof(AudioHighPassFilter),
                FilterKind.LowPass     => typeof(AudioLowPassFilter),
                FilterKind.Reverb      => typeof(AudioReverbFilter),
                _ => null,
            };
        }

        public void DisableFilter<FilterT>() where FilterT : Behaviour
        {
            Log.Safe($"DisableFilter実行:{typeof(FilterT).Name}");
            if (filterDict.TryGetValue(typeof(FilterT), out var component))
            {
                var filter = component as FilterT;
                filter.enabled = false;
            }
        }

        public void DisableAllFilters()
        {
            foreach (var filter in filterDict.Values)
            {
                if (filter is Behaviour b) b.enabled = false;
            }
        }

        public void RemoveFilter<FilterT>() where FilterT : Behaviour
        {
            if (filterDict.TryGetValue(typeof(FilterT), out var component))
            {
                filterDict.Remove(typeof(FilterT));
                UnityEngine.Object.Destroy(component);
            }
        }

        public void RemoveAllFilters()
        {
            foreach (var comp in filterDict.Values)
            {
                if (comp == null) continue;
                UnityEngine.Object.Destroy(comp);
            }
            filterDict.Clear();
        }
    }
}
