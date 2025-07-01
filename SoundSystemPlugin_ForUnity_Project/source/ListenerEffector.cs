namespace SoundSystem
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

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
