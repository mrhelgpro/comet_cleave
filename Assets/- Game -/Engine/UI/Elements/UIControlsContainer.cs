using UnityEngine;
using UnityEngine.UI;

public class UIControlsContainer : MonoBehaviour
{
    [SerializeField] private RectTransform _controlsContainer;
    [SerializeField] private RectTransform _controlsSize;
    [SerializeField] private RectTransform _footerSpace;
    [SerializeField] private RectTransform _headerSpace;

    private readonly Vector3[] _worldCorners = new Vector3[4];

    private void OnEnable()
    {
        if (_controlsSize == null)
        {
            return;
        }

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        var rootCanvas = canvas.rootCanvas;
        var eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;

        _controlsSize.GetWorldCorners(_worldCorners);

        var bottomLeft = RectTransformUtility.WorldToScreenPoint(eventCamera, _worldCorners[0]);
        var bottomRight = RectTransformUtility.WorldToScreenPoint(eventCamera, _worldCorners[3]);
        var bottomY = Mathf.Min(bottomLeft.y, bottomRight.y);
        var visibleBottom = rootCanvas.pixelRect.yMin;

        if (visibleBottom - bottomY >= 1f)
        {
            RefreshHeaderPosition();
        }
        else
        {
            RefreshFooterPosition();
        }
    }

    private void RefreshFooterPosition()
    {
        RefreshControlsAnchor(new Vector2(0.5f, 1f));
        _footerSpace.gameObject.SetActive(true);
        _headerSpace.gameObject.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_controlsContainer);
    }

    private void RefreshHeaderPosition()
    {
        RefreshControlsAnchor(new Vector2(0.5f, 0f));
        _footerSpace.gameObject.SetActive(false);
        _headerSpace.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_controlsContainer);
    }

    private void RefreshControlsAnchor(Vector2 anchor)
    {
        _controlsContainer.anchorMin = anchor;
        _controlsContainer.anchorMax = anchor;
        _controlsContainer.pivot = anchor;
        _controlsContainer.anchoredPosition3D = Vector3.zero;
    }
}