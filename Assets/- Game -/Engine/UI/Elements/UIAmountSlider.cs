using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAmountSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _amountText;

    public void Configure(int currentAmount, int requiredAmount)
    {
        _slider.minValue = 0;
        _slider.maxValue = requiredAmount;
        _slider.value = currentAmount;
        _amountText.text = $"{currentAmount}/{requiredAmount}";
    }
}
