using System;
using UnityEngine;

namespace Gameplay
{
    public class UIGameplayScreen : BaseBehaviour, IInitializable, IDisposable
    {
        [SerializeField] private UIButtonClick _uiSettingsButtonClick;
        [SerializeField] private UIGameplaySettingsPopup _uiGameplaySettingsPopup;
        [SerializeField] private UIGameplayStartPopup _uiStartPopup;
        [SerializeField] private UIGameplayVictoryPopup _uiVictoryPopup;
        [SerializeField] private UIGameplayDefeatPopup _uiDefeatPopup;

        private GameplaySession _gameplaySession;

        [Inject]
        public void Inject(GameplaySession gameplaySession)
        {
            _gameplaySession = gameplaySession;
        }

        void IInitializable.Initialize()
        {
            _uiSettingsButtonClick.OnClick += SettingsButtonClicked;
            _gameplaySession.OnStateChanged += StateChanged;
            ApplyState(_gameplaySession.CurrentState);
        }

        void IDisposable.Dispose()
        {
            _uiSettingsButtonClick.OnClick -= SettingsButtonClicked;

            if (_gameplaySession == null)
            {
                return;
            }

            _gameplaySession.OnStateChanged -= StateChanged;
        }

        protected override void Enabled()
        {
        }

        protected override void Disabled()
        {
        }

        private void SettingsButtonClicked()
        {
            _uiGameplaySettingsPopup.gameObject.SetActive(true);
        }

        private void StateChanged(GameplaySession.State state)
        {
            ApplyState(state);
        }

        private void ApplyState(GameplaySession.State state)
        {
            if (state == GameplaySession.State.WaitingToStart)
            {
                _uiVictoryPopup.gameObject.SetActive(false);
                _uiDefeatPopup.gameObject.SetActive(false);
                _uiStartPopup.Open(_gameplaySession.StartRound);
                return;
            }

            if (state == GameplaySession.State.Won)
            {
                _uiStartPopup.gameObject.SetActive(false);
                _uiDefeatPopup.gameObject.SetActive(false);
                _uiVictoryPopup.Open(ContinueClicked);
                return;
            }

            if (state == GameplaySession.State.Lost)
            {
                _uiStartPopup.gameObject.SetActive(false);
                _uiVictoryPopup.gameObject.SetActive(false);
                _uiDefeatPopup.Open(ContinueClicked);
                return;
            }

            _uiStartPopup.gameObject.SetActive(false);
            _uiVictoryPopup.gameObject.SetActive(false);
            _uiDefeatPopup.gameObject.SetActive(false);
        }

        private void ContinueClicked()
        {
            SceneLoadService.LoadGameplayScene(true);
        }
    }
}
