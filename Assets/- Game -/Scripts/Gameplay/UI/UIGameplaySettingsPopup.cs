using System;
using UnityEngine;

namespace Gameplay
{
    public class UIGameplaySettingsPopup : BaseBehaviour, IDisposable
    {
        [SerializeField] private UIButtonClick _closeButton;
        [SerializeField] private UIButtonClick _quitButton;
        [SerializeField] private UIConfirmationPopup _uiConfirmationPopup;

        [Header("Settings Buttons")]
        [SerializeField] private UIButtonClick _soundButton;
        [SerializeField] private UIButtonClick _musicButton;
        [SerializeField] private UIButtonClick _vibrationButton;

        [Header("Settings Marks")]
        [SerializeField] private GameObject _soundOnMark;
        [SerializeField] private GameObject _musicOnMark;
        [SerializeField] private GameObject _vibrationOnMark;

        private GameSettings _settingService;

        [Inject]
        public void Inject(GameSettings settingService)
        {
            _settingService = settingService;
        }

        void IDisposable.Dispose()
        {
            if (this != null)
            {
                gameObject.SetActive(false);
            }
        }

        protected override void Enabled()
        {
            _closeButton.OnClick += CloseButtonClicked;
            _soundButton.OnClick += SoundSwitchClicked;
            _musicButton.OnClick += MusicSwitchClicked;
            _vibrationButton.OnClick += VibrationSwitchClicked;
            _quitButton.OnClick += QuitButtonClicked;

            TimeService.Pause();
            SetupView();
        }

        protected override void Disabled()
        {
            _closeButton.OnClick -= CloseButtonClicked;
            _soundButton.OnClick -= SoundSwitchClicked;
            _musicButton.OnClick -= MusicSwitchClicked;
            _vibrationButton.OnClick -= VibrationSwitchClicked;
            _quitButton.OnClick -= QuitButtonClicked;

            TimeService.Resume();
        }

        private void SetupView()
        {
            _soundOnMark.SetActive(_settingService.IsSfxActive);
            _musicOnMark.SetActive(_settingService.IsMusicActive);
            _vibrationOnMark.SetActive(_settingService.IsVibrationActive);
        }

        private void SoundSwitchClicked()
        {
            _settingService.SwitchSfxActivity();
            _soundOnMark.SetActive(_settingService.IsSfxActive);
        }

        private void MusicSwitchClicked()
        {
            _settingService.SwitchMusicActivity();
            _musicOnMark.SetActive(_settingService.IsMusicActive);
        }

        private void VibrationSwitchClicked()
        {
            _settingService.SwitchHapticActivity();
            _vibrationOnMark.SetActive(_settingService.IsVibrationActive);
        }

        private void QuitButtonClicked()
        {
            _uiConfirmationPopup.ShowConfirmation(ConfirmButtonClicked);
        }

        private void CloseButtonClicked()
        {
            gameObject.SetActive(false);
        }

        private void ConfirmButtonClicked()
        {
            SceneLoadService.LoadGameplayScene(true);
            CloseButtonClicked();
        }
    }
}
