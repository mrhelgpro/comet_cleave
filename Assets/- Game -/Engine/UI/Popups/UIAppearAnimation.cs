using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIAppearAnimation : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Transform _rootContainer;
    [SerializeField] private SoundAsset _appearSound;

    private void OnEnable()
    {
        if (_background)
        {
            _background.enabled = false;
        }

        if (_rootContainer)
        {
            _rootContainer.gameObject.SetActive(false);
        }
        
        ScheduleService.RunNextFrame(ShowAppearAnimation);
    }

    private void ShowAppearAnimation()
    {
        if (_background)
        {
            _background.enabled = true;
            _background.DOFade(0.995f, 0.15f)
                .SetEase(Ease.OutQuad)
                .From(0f)
                .SetUpdate(true);
        }

        if (_rootContainer)
        {
            _rootContainer.gameObject.SetActive(true);
            _rootContainer.DOScale(Vector3.one, 0.4f)
                .From(0.4f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
            
        if (_appearSound)
        {
            AudioService.PlaySfx(_appearSound);
        }
    }
}