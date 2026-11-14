using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ItemPriceData
{
    public string itemName;
    public int price;
}

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("ตั้งค่าราคาสินค้าทั้งหมดในร้าน")]
    public List<ItemPriceData> itemPriceList = new List<ItemPriceData>();
    private Dictionary<string, int> priceLookup = new Dictionary<string, int>();

    [Header("สถานะการเงินของร้าน")]
    public int currentMoney = 0;
    public int dailyEarnedMoney = 0;

    [Header("UI แสดงเงินมุมจอ")]
    public TextMeshProUGUI moneyText;

    void Awake()
    {
        Instance = this;
        priceLookup.Clear();
        foreach (var item in itemPriceList)
        {
            if (!string.IsNullOrEmpty(item.itemName) && !priceLookup.ContainsKey(item.itemName))
            {
                priceLookup.Add(item.itemName, item.price);
            }
        }
    }

    void Start()
    {
        UpdateMoneyUI();
    }

    public int GetItemPrice(string itemName)
    {
        return priceLookup.ContainsKey(itemName) ? priceLookup[itemName] : 0;
    }

    public int CalculateTotal(List<string> items)
    {
        int total = 0;
        if (items == null) return 0;

        foreach (string item in items)
        {
            total += GetItemPrice(item);
        }
        return total;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        dailyEarnedMoney += amount;
        UpdateMoneyUI();
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney < amount) return false;
        currentMoney -= amount;
        UpdateMoneyUI();
        return true;
    }

    public void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentMoney} ฿";
        }
    }
}