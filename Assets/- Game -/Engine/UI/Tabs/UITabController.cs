using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UITabController : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private UITabButton _defaultTabButton;
    
    [ReadOnly]
    [SerializeField] private List<UITabButton> _tabButtons = new();

    [ReadOnly]
    [SerializeField] private RectTransform _rootContainer;
    
    private UITabButton _previousTabButton;

    void IInitializable.Initialize()
    {
        foreach (var uiTabButton in _tabButtons)
        {
            uiTabButton.Initialize(this);
        }

        if (_previousTabButton == null)
        {
            if (_defaultTabButton == null)
            {
                _defaultTabButton = _tabButtons[0];
            }

            _defaultTabButton.Focus();
        }
    }

    void IDisposable.Dispose()
    {
        foreach (var uiTabButton in _tabButtons)
        {
            uiTabButton.Unsubscribe();
        }
    }

    public void ClosePreviousPage(UITabButton tabButton)
    {
        if (_previousTabButton != tabButton)
        {
            _previousTabButton?.SetUnlocked(true);
            _previousTabButton = tabButton;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootContainer);
        }
    }
    
#if UNITY_EDITOR
    [Button]
    private void RefreshCollection()
    {
        _tabButtons.Clear();
        _rootContainer = GetComponent<RectTransform>();
        
        var uiTabButtons = _rootContainer.GetComponentsInChildren<UITabButton>();

        foreach (var uiTabButton in uiTabButtons)
        {
            _tabButtons.Add(uiTabButton);
        }

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
}
