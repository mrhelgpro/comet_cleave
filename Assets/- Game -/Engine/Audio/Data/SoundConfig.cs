using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Config_Sounds", menuName = ProjectSettings.TITLE + "/Configs/Sounds Config")]
public class SoundsConfig : SerializedScriptableObject
{
    [Header("Musics")]
    [SerializeField] private SoundAsset _menuMusic;
    [SerializeField] private SoundAsset _gameMusic;

    public SoundAsset MenuMusic => _menuMusic;
    public SoundAsset GameMusic => _gameMusic;
}