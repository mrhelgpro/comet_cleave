using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = ProjectSettings.TITLE + "/Audio/Sound Data")]
public class SoundAsset : ScriptableObject
{
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private float _volume = 1f;

    public AudioClip AudioClip => _audioClip;
    public float Volume => _volume;
}