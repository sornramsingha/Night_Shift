using UnityEngine;
using TMPro;

public class ShelfPriceTag : MonoBehaviour
{
    public string itemName;
    public TextMeshProUGUI uiPriceText;
    public TextMeshPro worldPriceText;

    void Start()
    {
        if (MoneyManager.Instance == null) return;

        int price = MoneyManager.Instance.GetItemPrice(itemName);
        string formatted = $"{price} ฿";

        if (uiPriceText != null) uiPriceText.text = formatted;
        if (worldPriceText != null) worldPriceText.text = formatted;
    }
}