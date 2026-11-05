using System;
using TMPro;
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
        [SerializeField] private TMP_Text _chargeValue;

        private GameplaySession _gameplaySession;
        private ChargeTransfer _chargeTransfer;
        private PlayerConfig _playerConfig;

        [Inject]
        public void Inject(GameplaySession gameplaySession, ChargeTransfer chargeTransfer, PlayerConfig playerConfig)
        {
            _gameplaySession = gameplaySession;
            _chargeTransfer = chargeTransfer;
            _playerConfig = playerConfig;
        }

        void IInitializable.Initialize()
        {
            _uiSettingsButtonClick.OnClick += SettingsButtonClicked;
            _gameplaySession.OnStateChanged += StateChanged;
            _chargeTransfer.OnChargeChanged += ChargeChanged;
            _chargeValue.raycastTarget = false;
            ApplyState(_gameplaySession.CurrentState);
        }

        void IDisposable.Dispose()
        {
            _uiSettingsButtonClick.OnClick -= SettingsButtonClicked;

            if (_gameplaySession != null)
            {
                _gameplaySession.OnStateChanged -= StateChanged;
            }

            if (_chargeTransfer != null)
            {
                _chargeTransfer.OnChargeChanged -= ChargeChanged;
            }
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

        private void ChargeChanged(float sourceRemaining, float chargeAmount)
        {
            ApplyCharge(sourceRemaining);
        }

        private void ApplyCharge(float sourceRemaining)
        {
            _chargeValue.text = $"{RemainingPercent(sourceRemaining)}%";
        }

        private int RemainingPercent(float sourceRemaining)
        {
            var startSize = _playerConfig.StartSize;

            if (startSize <= 0f)
            {
                return 0;
            }

            return Mathf.Clamp(Mathf.RoundToInt(sourceRemaining / startSize * 100f), 0, 100);
        }

        private static bool ShowsCharge(GameplaySession.State state)
        {
            return state is GameplaySession.State.Ready
                or GameplaySession.State.Charging
                or GameplaySession.State.ShotInFlight
                or GameplaySession.State.Clearing
                or GameplaySession.State.Advancing;
        }

        private void ApplyState(GameplaySession.State state)
        {
            var showCharge = ShowsCharge(state);
            _chargeValue.gameObject.SetActive(showCharge);

            if (showCharge)
            {
                ApplyCharge(_chargeTransfer.SourceRemaining);
            }

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
                _uiVictoryPopup.Open(RemainingPercent(_chargeTransfer.SourceRemaining), ContinueClicked);
                return;
            }

            if (state == GameplaySession.State.Lost)
            {
                _uiStartPopup.gameObject.SetActive(false);
                _uiVictoryPopup.gameObject.SetActive(false);
                _uiDefeatPopup.Open(RemainingPercent(_chargeTransfer.SourceRemaining), ContinueClicked);
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
