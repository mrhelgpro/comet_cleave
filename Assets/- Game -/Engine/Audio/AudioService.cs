using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

public class AudioService : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private float _musicFadeOutDuration = 0.25f;
    [SerializeField] private float _musicFadeInDuration = 0.5f;
    
    [Header("Sources")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _musicSource;

    private const float SFX_COOLDOWN_SECONDS = 0.1f;

    private static AudioService _instance;
    private static SoundAsset _currentMusicAsset;

    private readonly Dictionary<int, float> _lastSfxPlayTimeByAssetId = new();
        
    private GameSettings _gameSettings;

    [Inject]
    public void Inject(GameSettings gameSettings)
    {
        _gameSettings = gameSettings;
    }
    
    void IInitializable.Initialize()
    {
        _gameSettings.OnSfxStatusChanged += ApplySfxGameSettings;
        _gameSettings.OnMusicStatusChanged += ApplyMusicGameSettings;
        
        ApplySfxGameSettings();
        ApplyMusicGameSettings();
        
        _instance = this;
    }
    
    void IDisposable.Dispose()
    {
        _gameSettings.OnSfxStatusChanged -= ApplySfxGameSettings;
        _gameSettings.OnMusicStatusChanged -= ApplyMusicGameSettings;
        
        _musicSource.DOKill();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static void PlaySfx(SoundBundleAsset soundBundleAsset)
    {
        if (soundBundleAsset == null) return;
        if (_instance.IsSfxOnCooldown(soundBundleAsset)) return;

        var clip = soundBundleAsset.GetRandomClip();
        var volume = soundBundleAsset.Volume;

        _instance.PlaySfxOneShot(clip, volume);
    }

    public static void PlaySfx(SoundAsset soundAsset)
    {
        if (soundAsset == null) return;
        if (_instance.IsSfxOnCooldown(soundAsset)) return;

        _instance.PlaySfxOneShot(soundAsset.AudioClip, soundAsset.Volume);
    }

    public static void PlayMusic(SoundAsset soundAsset)
    {
        _currentMusicAsset = soundAsset;
        _instance.PlayMusicInternal(soundAsset);
    }

    public static void StopMusic()
    {
        _instance.StopMusicInternal();
    }

    public static SoundAsset GetCurrentMusicSoundData()
    {
        return _currentMusicAsset;
    }

    private void ApplySfxGameSettings()
    {
        _sfxSource.mute = !_gameSettings.IsSfxActive;
    }

    private void ApplyMusicGameSettings()
    {
        _musicSource.mute = !_gameSettings.IsMusicActive;
    }

    private bool IsSfxOnCooldown(Object assetKey)
    {
        var id = assetKey.GetInstanceID();
        var now = Time.unscaledTime;
        if (_lastSfxPlayTimeByAssetId.TryGetValue(id, out var last) && now - last < SFX_COOLDOWN_SECONDS)
        {
            return true;
        }

        _lastSfxPlayTimeByAssetId[id] = now;
        return false;
    }

    private void PlaySfxOneShot(AudioClip clip, float volume)
    {
        if (clip)
        {
            _sfxSource.PlayOneShot(clip, volume);
        }
    }

    private void PlayMusicInternal(SoundAsset soundAsset)
    {
        if (_musicSource.isPlaying)
        {
            _musicSource
                .DOFade(0f, _musicFadeOutDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    PlayMusicWithFade(soundAsset);
                });
        }
        else
        {
            PlayMusicWithFade(soundAsset);
        }
    }

    private void StopMusicInternal()
    {
        _musicSource.DOFade(0f, _musicFadeOutDuration).SetUpdate(true);
    }

    private void PlayMusicWithFade(SoundAsset soundAsset)
    {
        _musicSource.Stop();

        if (soundAsset == null) return;
        if (soundAsset.AudioClip == null) return;

        _musicSource.clip = soundAsset.AudioClip;
        _musicSource.volume = 0f;
        _musicSource.Play();
        _musicSource
            .DOFade(soundAsset.Volume, _musicFadeInDuration)
            .SetUpdate(true);
    }
    
}