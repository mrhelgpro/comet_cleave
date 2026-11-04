using System;
using Lofelt.NiceVibrations;
using UnityEngine;

public class VibrationService : MonoBehaviour, IInitializable, IDisposable
{
    private static VibrationService _instance;
    
    private GameSettings _gameSettings;
    
    [Inject]
    public void Inject(GameSettings gameSettings)
    {
        _gameSettings = gameSettings;
    }
    
    void IInitializable.Initialize()
    {
        _instance = this;
    }
    
    void IDisposable.Dispose()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    public void PlayLightImpact()
    {
        if (_gameSettings.IsVibrationActive)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        }
    }
        
    public void PlayFailure()
    {
        if (_gameSettings.IsVibrationActive)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
        }
    }
        
    public void PlayWarning()
    {
        if (_gameSettings.IsVibrationActive)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
        }
    }
}