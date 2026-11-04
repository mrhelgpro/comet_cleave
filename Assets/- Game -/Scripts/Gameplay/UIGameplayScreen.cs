using UnityEngine;

namespace Gameplay
{
    public class UIGameplayScreen : BaseBehaviour
    {
        [SerializeField] private UIButtonClick _uiSettingsButtonClick;
        [SerializeField] private UIGameplaySettingsPopup _uiGameplaySettingsPopup;

        protected override void Enabled()
        {
            _uiSettingsButtonClick.OnClick += SettingsButtonClicked;
        }

        protected override void Disabled()
        {
            _uiSettingsButtonClick.OnClick -= SettingsButtonClicked;
        }

        private void SettingsButtonClicked()
        {
            _uiGameplaySettingsPopup.gameObject.SetActive(true);
        }
    }
}
