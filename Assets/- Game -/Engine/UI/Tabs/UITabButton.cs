using TMPro;
using UnityEngine;

public class UITabButton : MonoBehaviour
{
    [SerializeField] private GameObject _pageContainer;
    
    [Header("View")] 
    [SerializeField] private UIButtonClick _buttonClick;
    [SerializeField] private GameObject _buttonFocus;
    [SerializeField] private GameObject _buttonLock;
    [SerializeField] private TMP_Text _labelText;
    
    [Header("Button Settings")] 
    [SerializeField] private Vector2 _defaultButtonSize;
    [SerializeField] private Vector2 _focusButtonSize;
    
    [Header("Icon Settings")] 
    [SerializeField] private RectTransform _iconContainer;
    [SerializeField] private Vector2 _defaultIconSize;
    [SerializeField] private Vector2 _focusIconSize;
    [SerializeField] private Vector2 _defaultIconPosition;
    [SerializeField] private Vector2 _focusIconPosition;
        
    private RectTransform _rootContainer;
    private UITabController _tabController;
    
    public void Initialize(UITabController tabController)
    {
        _rootContainer = GetComponent<RectTransform>();
        _tabController = tabController;
        _buttonClick.OnClick += Focus;
    }

    public void Unsubscribe()
    {
        _buttonClick.OnClick -= Focus;
    }
    
    public void RefreshLabel(string labelText)
    {
        _labelText.text = labelText;
    }

    public void SetUnlocked(bool status)
    {
        _rootContainer.sizeDelta = _defaultButtonSize;
        _iconContainer.sizeDelta = _defaultIconSize;
        _iconContainer.anchoredPosition = _defaultIconPosition;
            
        _pageContainer.SetActive(false);
        _iconContainer.gameObject.SetActive(status);
        _buttonClick.gameObject.SetActive(status);
        _buttonFocus.SetActive(false);
        _buttonLock.SetActive(!status);
    }
     
    public void Focus()
    {
        _rootContainer.sizeDelta = _focusButtonSize;
        _iconContainer.sizeDelta = _focusIconSize;
        _iconContainer.anchoredPosition = _focusIconPosition;
            
        _tabController.ClosePreviousPage(tabButton:this);
        _pageContainer.SetActive(true);
        _buttonClick.gameObject.SetActive(false);
        _buttonFocus.SetActive(true);
        _buttonLock.SetActive(false);
    }
}