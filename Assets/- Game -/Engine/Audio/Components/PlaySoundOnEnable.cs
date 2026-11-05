using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
    [SerializeField] private SoundAsset _soundAsset;
    [SerializeField] private bool _useSfx;

    private void OnEnable()
    {
        if (_soundAsset == null)
        {
            return;
        }

        if (_useSfx)
        {
            AudioService.PlaySfx(_soundAsset);
            return;
        }
        
        AudioService.PlayMusic(_soundAsset);
    }
}