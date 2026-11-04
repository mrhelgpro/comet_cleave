using UnityEngine;

public class UIViewportShifter : MonoBehaviour
{
    [SerializeField] private RectTransform _rootContainer;
    [SerializeField] private RectTransform _visibleContainer;
    [SerializeField] private float _offset;
    
    private Canvas _rootCanvas;
    private Camera _eventCamera;
    private Vector3 _initialLocalPosition;
    private RectTransform _boundsContainer;

    private readonly Vector3[] _worldCorners = new Vector3[4];

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
        _initialLocalPosition = _rootContainer.localPosition;

        if (_rootCanvas == null)
        {
            return;
        }

        var rootCanvas = _rootCanvas.rootCanvas;
        _eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;
        _rootCanvas = rootCanvas;
    }

    public void Configure(RectTransform boundsContainer)
    {
        _boundsContainer = boundsContainer;
    }

    public void Shift()
    {
        _rootContainer.localPosition = _initialLocalPosition;

        if (_rootCanvas == null || _visibleContainer == null)
        {
            return;
        }

        var parent = _rootContainer.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        _visibleContainer.GetWorldCorners(_worldCorners);

        var bottomLeft = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[0]);
        var bottomRight = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[3]);
        var topLeft = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[1]);
        var topRight = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[2]);
        var bottomY = Mathf.Min(bottomLeft.y, bottomRight.y);
        var topY = Mathf.Max(topLeft.y, topRight.y);
        
        if (TryGetBoundsScreenVertical(out var visibleBottom, out var visibleTop) == false)
        {
            visibleBottom = _rootCanvas.pixelRect.yMin;
            visibleTop = _rootCanvas.pixelRect.yMax;
        }

        var screenOffset = GetScreenOffset(parent);
        visibleBottom += screenOffset;
        visibleTop -= screenOffset;

        if (visibleBottom - bottomY >= 1f)
        {
            ApplyVerticalShift(parent, bottomLeft.x, bottomY, visibleBottom);
            return;
        }

        if (topY - visibleTop >= 1f)
        {
            ApplyVerticalShift(parent, topLeft.x, topY, visibleTop);
        }
    }

    private bool TryGetBoundsScreenVertical(out float bottom, out float top)
    {
        bottom = 0f;
        top = 0f;

        if (_boundsContainer == null)
        {
            return false;
        }

        _boundsContainer.GetWorldCorners(_worldCorners);

        var bottomLeft = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[0]);
        var bottomRight = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[3]);
        var topLeft = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[1]);
        var topRight = RectTransformUtility.WorldToScreenPoint(_eventCamera, _worldCorners[2]);
        bottom = Mathf.Min(bottomLeft.y, bottomRight.y);
        top = Mathf.Max(topLeft.y, topRight.y);
        return true;
    }

    private float GetScreenOffset(RectTransform parent)
    {
        var originScreen = RectTransformUtility.WorldToScreenPoint(_eventCamera, parent.TransformPoint(Vector3.zero));
        var offsetScreen = RectTransformUtility.WorldToScreenPoint(
            _eventCamera,
            parent.TransformPoint(new Vector3(0f, _offset, 0f)));
        return Mathf.Abs(offsetScreen.y - originScreen.y);
    }

    private void ApplyVerticalShift(RectTransform parent, float screenX, float fromScreenY, float toScreenY)
    {
        var fromScreen = new Vector2(screenX, fromScreenY);
        var toScreen = new Vector2(screenX, toScreenY);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, fromScreen, _eventCamera, out var fromLocal) == false)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, toScreen, _eventCamera, out var toLocal) == false)
        {
            return;
        }

        var deltaY = toLocal.y - fromLocal.y;
        if (toScreenY > fromScreenY && deltaY <= 0f)
        {
            return;
        }

        if (toScreenY < fromScreenY && deltaY >= 0f)
        {
            return;
        }

        var localPosition = _initialLocalPosition;
        localPosition.y += deltaY;
        _rootContainer.localPosition = localPosition;
    }

    public void Reset()
    {
        _rootContainer.localPosition = _initialLocalPosition;
    }
}
