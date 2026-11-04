using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundBundleData", menuName = ProjectSettings.TITLE + "/Audio/Sound Bundle Data")]
public class SoundBundleAsset : ScriptableObject
{
    [SerializeField] private float _volume;
    [SerializeField] private List<AudioClip> _clips;

    public float Volume => _volume;

    public AudioClip GetRandomClip()
    {
        if (_clips == null || _clips.Count == 0)
        {
            return null;
        }
            
        var randomIndex = Random.Range(0, _clips.Count);
        return _clips[randomIndex];
    }
}