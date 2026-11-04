public class UIQueuePopup
{
    public BaseBehaviour ActivePopup { get; private set; }

    public bool HasActivePopup => ActivePopup != null;
    
    public void Clear()
    {
        ActivePopup = null;
    }

    public void SetActivePopup(BaseBehaviour popup)
    {
        if (ActivePopup != null)
        {
            ActivePopup.gameObject.SetActive(false);
        }

        ActivePopup = popup;
    }
}
