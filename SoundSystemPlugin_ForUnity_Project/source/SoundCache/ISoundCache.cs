using UnityEngine;

public interface ISoundCache
{
    AudioClip Retrieve(string resourceAddress);

    void Add(string resourceAddress, AudioClip clip);

    void Remove(string resourceAddress);

    void ClearAll();

    void Evict();
}