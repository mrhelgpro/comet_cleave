using UnityEngine;

public class UIButtonActivator : BaseBehaviour
{
    [SerializeField] private UIButtonClick _uiInfoButtonClick;
    [SerializeField] private RectTransform _targetContainer;
        
    protected override void Enabled()
    {
        _uiInfoButtonClick.OnClick += ActivatorButtonClick;
    }

    protected override void Disabled()
    {
        _uiInfoButtonClick.OnClick += ActivatorButtonClick;
    }
        
    private void ActivatorButtonClick()
    {
        _targetContainer.gameObject.SetActive(true);
    }
}