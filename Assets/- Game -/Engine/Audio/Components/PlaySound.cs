using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] private SoundAsset _soundAsset;
        
    public void Play()
    {
        if (_soundAsset == null)
        {
            return;
        }

        AudioService.PlaySfx(_soundAsset);
    }   
}