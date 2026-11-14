using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class BanknoteVisual
{
    public int amount;
    public Sprite normalSprite;
    public Sprite ghostSprite;
}

public class CheckoutManager : MonoBehaviour
{
    public static CheckoutManager Instance;

    [Header("หน้าต่างคิดเงิน")]
    public GameObject checkoutPanel;
    [HideInInspector] public bool isCheckoutOpen = false;

    [Header("แสดงข้อมูลการจ่ายเงินของลูกค้า")]
    public Image customerMoneyImage;
    public TextMeshProUGUI customerPaidText;
    public TextMeshProUGUI bagItemsListText;

    [Header("ตั้งค่าแบงก์และเงินผี")]
    public List<BanknoteVisual> banknoteVisuals = new List<BanknoteVisual>();
    [Range(0f, 1f)] public float ghostMoneyChance = 0.35f;

    [Header("หน้าจอเครื่องคิดเลข")]
    public TextMeshProUGUI calculatorDisplayText;
    private string currentInput = "0";
    private int storedValue = 0;
    private string pendingOperator = "";
    private bool isNewNumber = true;

    [Header("ช่องทอนเงินที่จะส่งคืนลูกค้า")]
    public TextMeshProUGUI changeToGiveText;
    private int currentChangeToGive = 0;

    [Header("ค่าสติที่ได้รับ ลดลงเมื่อคิดเงิน")]
    public float sanityRewardExactChange = 5f;
    public float sanityPenaltyWrongChange = 8f;

    private int actualOrderTotal = 0;
    private int customerPaidAmount = 0;
    private bool isGhostMoney = false;
    private bool hasRolledPaymentForCurrentCustomer = false;

    void Awake()
    {
        Instance = this;
        if (checkoutPanel != null) checkoutPanel.SetActive(false);
    }

    public void ResetForNewCustomer()
    {
        hasRolledPaymentForCurrentCustomer = false;
        isGhostMoney = false;
        customerPaidAmount = 0;
        CloseCheckout();
    }

    public void OpenCheckout(List<string> expectedItems, List<string> itemsInBag, bool customerHasAnomaly)
    {
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            WarningManager.Show("ไฟดับ! ต้องซ่อมไฟก่อนคิดเงิน!");
            return;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            WarningManager.Show("คุยกับลูกค้าให้จบก่อน!");
            return;
        }

        isCheckoutOpen = true;
        if (checkoutPanel != null) checkoutPanel.SetActive(true);

        actualOrderTotal = (MoneyManager.Instance != null)
            ? MoneyManager.Instance.CalculateTotal(expectedItems)
            : 0;

        if (!hasRolledPaymentForCurrentCustomer)
        {
            hasRolledPaymentForCurrentCustomer = true;
            customerPaidAmount = GenerateValidCustomerPayment(actualOrderTotal);
            isGhostMoney = customerHasAnomaly && (Random.value <= ghostMoneyChance);
        }

        if (customerPaidText != null)
        {
            customerPaidText.text = $"รับเงินมา: {customerPaidAmount} Baht";
        }
        UpdateBanknoteVisual(customerPaidAmount, isGhostMoney);

        if (bagItemsListText != null)
        {
            bagItemsListText.text = "สินค้าในถุง:\n- " + string.Join("\n- ", itemsInBag);
        }

        OnCalcClear();
        ResetChangeToGive();
    }

    public void CloseCheckout()
    {
        isCheckoutOpen = false;
        if (checkoutPanel != null) checkoutPanel.SetActive(false);
    }

    private int GenerateValidCustomerPayment(int totalPrice)
    {
        if (totalPrice <= 0) return 20;

        List<int> validPayments = new List<int> { totalPrice };

        int[] denominations = { 20, 50, 100, 500, 1000 };
        foreach (int note in denominations)
        {
            if (note >= totalPrice && !validPayments.Contains(note))
            {
                validPayments.Add(note);
            }
        }

        int roundUp50 = Mathf.CeilToInt(totalPrice / 50f) * 50;
        int roundUp100 = Mathf.CeilToInt(totalPrice / 100f) * 100;
        if (roundUp50 >= totalPrice && !validPayments.Contains(roundUp50)) validPayments.Add(roundUp50);
        if (roundUp100 >= totalPrice && !validPayments.Contains(roundUp100)) validPayments.Add(roundUp100);

        return validPayments[Random.Range(0, validPayments.Count)];
    }

    private void UpdateBanknoteVisual(int amount, bool ghost)
    {
        if (customerMoneyImage == null) return;

        BanknoteVisual matched = null;
        foreach (var note in banknoteVisuals)
        {
            if (note.amount <= amount) matched = note;
        }
        if (matched == null && banknoteVisuals.Count > 0) matched = banknoteVisuals[0];

        if (matched != null)
        {
            customerMoneyImage.sprite = (ghost && matched.ghostSprite != null)
                ? matched.ghostSprite
                : matched.normalSprite;
        }
    }

    #region --- ระบบปุ่มกดเครื่องคิดเลข (ผูกกับ Button OnClick ใน Inspector) ---

    public void OnCalcDigit(string digit)
    {
        if (isNewNumber || currentInput == "0")
        {
            currentInput = digit;
            isNewNumber = false;
        }
        else if (currentInput.Length < 6)
        {
            currentInput += digit;
        }
        UpdateCalcDisplay();
    }

    public void OnCalcOperator(string op)
    {
        if (!isNewNumber && !string.IsNullOrEmpty(pendingOperator))
        {
            OnCalcEquals();
        }
        else
        {
            int.TryParse(currentInput, out storedValue);
        }

        pendingOperator = op;
        isNewNumber = true;
    }

    public void OnCalcEquals()
    {
        if (string.IsNullOrEmpty(pendingOperator)) return;

        int currentVal = 0;
        int.TryParse(currentInput, out currentVal);

        int result = storedValue;
        if (pendingOperator == "+") result = storedValue + currentVal;
        else if (pendingOperator == "-") result = Mathf.Max(0, storedValue - currentVal);

        currentInput = result.ToString();
        storedValue = result;
        pendingOperator = "";
        isNewNumber = true;
        UpdateCalcDisplay();
        UseCalculatorResultAsChange();
    }

    public void OnCalcClear()
    {
        currentInput = "0";
        storedValue = 0;
        pendingOperator = "";
        isNewNumber = true;
        UpdateCalcDisplay();
        ResetChangeToGive();
    }

    private void UpdateCalcDisplay()
    {
        if (calculatorDisplayText != null)
            calculatorDisplayText.text = currentInput;
    }

    #endregion

    #region --- ระบบจัดเตรียมเงินทอน & ยืนยันการขาย ---

    public void UseCalculatorResultAsChange()
    {
        if (!string.IsNullOrEmpty(pendingOperator)) OnCalcEquals();

        int val = 0;
        int.TryParse(currentInput, out val);
        currentChangeToGive = Mathf.Max(0, val);
        UpdateChangeUI();
    }

    public void AddChangeAmount(int amount)
    {
        currentChangeToGive = Mathf.Max(0, currentChangeToGive + amount);
        UpdateChangeUI();
    }

    public void ResetChangeToGive()
    {
        currentChangeToGive = 0;
        UpdateChangeUI();
    }

    private void UpdateChangeUI()
    {
        if (changeToGiveText != null)
            changeToGiveText.text = $"เงินทอน: {currentChangeToGive} ฿";
    }

    public void ConfirmCheckoutAndGiveItem()
    {
        if (DialogueManager.Instance == null) return;

        UseCalculatorResultAsChange();
        CloseCheckout();
        DialogueManager.Instance.CompleteSaleWithChange(actualOrderTotal, customerPaidAmount, currentChangeToGive, isGhostMoney);
    }

    #endregion
}