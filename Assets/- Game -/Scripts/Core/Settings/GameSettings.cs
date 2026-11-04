using System;

public class GameSettings : ISavable
{
    private const string SAVE_KEY = "GameSettingsSave";

    public event Action OnSaveRequired;

    public event Action OnSfxStatusChanged;
    public event Action OnMusicStatusChanged;
    public event Action OnVibrationStatusChanged;

    public bool IsSfxActive { get; private set; } = true;
    public bool IsMusicActive { get; private set; } = true;
    public bool IsVibrationActive { get; private set; } = true;

    void ISavable.Load()
    {
        var save = SettingsSaveStorage.Load<GameSettingsSave>(SAVE_KEY);

        IsSfxActive = save.IsSfxActive;
        IsMusicActive = save.IsMusicActive;
        IsVibrationActive = save.IsVibrationActive;
    }

    void ISavable.Save()
    {
        var save = new GameSettingsSave
        {
            IsSfxActive = IsSfxActive,
            IsMusicActive = IsMusicActive,
            IsVibrationActive = IsVibrationActive
        };

        SettingsSaveStorage.Save(SAVE_KEY, save);
    }

    public void SwitchSfxActivity()
    {
        IsSfxActive = !IsSfxActive;
        OnSfxStatusChanged?.Invoke();
        OnSaveRequired?.Invoke();
    }

    public void SwitchMusicActivity()
    {
        IsMusicActive = !IsMusicActive;
        OnMusicStatusChanged?.Invoke();
        OnSaveRequired?.Invoke();
    }

    public void SwitchHapticActivity()
    {
        IsVibrationActive = !IsVibrationActive;
        OnVibrationStatusChanged?.Invoke();
        OnSaveRequired?.Invoke();
    }
}
