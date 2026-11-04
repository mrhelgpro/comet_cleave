using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
    [SerializeField] private SoundAsset _soundAsset;

    private void OnEnable()
    {
        if (_soundAsset == null)
        {
            return;
        }

        AudioService.PlaySfx(_soundAsset);
    }
}