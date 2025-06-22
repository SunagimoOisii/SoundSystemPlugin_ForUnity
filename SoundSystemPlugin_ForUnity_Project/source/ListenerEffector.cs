namespace SoundSystem
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// SoundSystem삷NX̂P<para></para>
    /// AudioListenerɃGtFNgtB^[𓮓Iɒǉs
    /// (GtFNgtB^[ƂAudioReverbFilterAudioEchoFilterȂǂŁA
    /// {NXłBehaviourNX^ƂēIɈ)
    /// </summary>
    internal sealed class ListenerEffector
    {
        public AudioListener Listener { private get; set; }
    
        private readonly Dictionary<Type, Component> filterDict = new();
    
        public ListenerEffector(AudioListener l)
        {
            Listener = l;
        }
    
        /// <typeparam name="FilterT">KptB^[̌^</typeparam>
        /// <param name="configure">tB^[̐ݒsANV</param>
        /// <remarks>gp: effector.ApplyFilter<AudioReverbFilter>(filter => filter.reverbLevel = Mathf.Clamp(reverbLevel, -10000f, 2000f));</remarks>
        public void ApplyFilter<FilterT>(Action<FilterT> configure) where FilterT : Behaviour
        {
            Log.Safe($"ApplyFilters:{typeof(FilterT).Name}");
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
            Log.Safe($"DisableFilters:{typeof(FilterT).Name}");
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
    }
}
