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

    [Header("1. วัตถุเงินที่ลูกค้ายื่นมาหน้าร้าน")]
    public GameObject customerHandMoneyObj;              // Object เงินที่โผล่เมื่อวางถุงบนถาด
    public SpriteRenderer customerHandMoneyRenderer;     // ใส่ถ้าวัตถุในฉากเป็น 2D Sprite
    public Image customerHandMoneyUIImage;               // ใส่ถ้าทำเป็นปุ่ม UI แทน

    [Header("2. หน้าต่างเครื่องคิดเงินและลิ้นชัก (UI)")]
    public GameObject checkoutPanel;
    [HideInInspector] public bool isCheckoutOpen = false;
    [HideInInspector] public bool isMoneyOnCounter = false;   // ลูกค้ายื่นเงินออกมาแล้วหรือยัง
    [HideInInspector] public bool hasReceivedMoney = false;   // ผู้เล่นกดรับเงินจากลูกค้าแล้วหรือยัง

    [Header("3. ข้อความบนหน้าจอเครื่องคิดเงิน (ตามเรฟ)")]
    public TextMeshProUGUI totalPriceText;       // Total (ค่าสินค้าทั้งหมด)
    public TextMeshProUGUI receivedMoneyText;    // Received (เงินที่ได้รับมา)
    public TextMeshProUGUI requiredChangeText;   // Change (จำนวนเงินที่ต้องทอน)
    public TextMeshProUGUI givenChangeText;      // Given (จำนวนเงินที่เราหยิบทอน)
    public Image registerBanknotePreview;        // (ไม่บังคับ) โชว์รูปแบงก์ที่รับมาบนจอเครื่องคิดเงินด้วย

    [Header("4. ตั้งค่าแบงก์และโอกาสเจอเงินผี")]
    public List<BanknoteVisual> banknoteVisuals = new List<BanknoteVisual>();
    [Range(0f, 1f)] public float ghostMoneyChance = 0.35f;

    [Header("5. ค่าสติที่ได้รับ / ลดลงเมื่อทอนเงิน")]
    public float sanityRewardExactChange = 5f;
    public float sanityPenaltyWrongChange = 8f;

    // ข้อมูลบิลของลูกค้าคนปัจจุบัน
    private int actualOrderTotal = 0;
    private int customerPaidAmount = 0;
    private int requiredChangeAmount = 0;
    private int currentChangeToGive = 0;
    private bool isGhostMoney = false;
    private bool hasRolledPaymentForCurrentCustomer = false;

    // เก็บประวัติการหยิบเงินทอน เผื่อผู้เล่นกดเกินแล้วอยากกดย้อนกลับทีละใบ
    private Stack<int> changeHistory = new Stack<int>();

    void Awake()
    {
        Instance = this;
        if (checkoutPanel != null) checkoutPanel.SetActive(false);
        if (customerHandMoneyObj != null) customerHandMoneyObj.SetActive(false);
    }

    // เรียกตอนเปลี่ยนลูกค้าคนใหม่ หรือจบการบริการ (หมดเวลา / แปะยันต์ / ขายเสร็จ)
    public void ResetForNewCustomer()
    {
        hasRolledPaymentForCurrentCustomer = false;
        isMoneyOnCounter = false;
        hasReceivedMoney = false;
        isGhostMoney = false;
        actualOrderTotal = 0;
        customerPaidAmount = 0;
        requiredChangeAmount = 0;
        currentChangeToGive = 0;
        changeHistory.Clear();

        if (customerHandMoneyObj != null) customerHandMoneyObj.SetActive(false);
        CloseCheckout();
    }

    // ซ่อนเงินบนเคาน์เตอร์กรณีผู้เล่นกดเปลี่ยนถุงก่อนรับเงิน
    public void HideHandMoney()
    {
        isMoneyOnCounter = false;
        if (customerHandMoneyObj != null) customerHandMoneyObj.SetActive(false);
    }

    // ขั้นที่ 1: เรียกอัตโนมัติทันทีที่ผู้เล่นวางถุงสินค้าลงบนถาด -> ลูกค้ายื่นเงินมาให้
    public void PrepareCustomerPayment(List<string> expectedItems, bool customerHasAnomaly)
    {
        actualOrderTotal = (MoneyManager.Instance != null)
            ? MoneyManager.Instance.CalculateTotal(expectedItems)
            : 0;

        // สุ่มเงินที่ลูกค้าจ่ายแค่ครั้งเดียวต่อคน กันการรีโรล
        if (!hasRolledPaymentForCurrentCustomer)
        {
            hasRolledPaymentForCurrentCustomer = true;
            customerPaidAmount = GenerateValidCustomerPayment(actualOrderTotal);
            isGhostMoney = customerHasAnomaly && (Random.value <= ghostMoneyChance);
        }

        requiredChangeAmount = Mathf.Max(0, customerPaidAmount - actualOrderTotal);

        // อัปเดตรูปแบงก์ปกติ / แบงก์ผี และเปิด Object เงินให้ลูกค้ายื่นออกมา
        Sprite noteSprite = GetBanknoteSprite(customerPaidAmount, isGhostMoney);
        if (noteSprite != null)
        {
            if (customerHandMoneyRenderer != null) customerHandMoneyRenderer.sprite = noteSprite;
            if (customerHandMoneyUIImage != null) customerHandMoneyUIImage.sprite = noteSprite;
            if (registerBanknotePreview != null) registerBanknotePreview.sprite = noteSprite;
        }

        isMoneyOnCounter = true;
        hasReceivedMoney = false;
        if (customerHandMoneyObj != null) customerHandMoneyObj.SetActive(true);
    }

    // ขั้นที่ 2: ผู้เล่นคลิกที่เงินที่ลูกค้ายื่นมา
    public void TakeCustomerMoney()
    {
        if (!isMoneyOnCounter || hasReceivedMoney) return;

        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            WarningManager.Show("ไฟดับ! ต้องซ่อมไฟก่อนรับเงิน!");
            return;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            WarningManager.Show("คุยกับลูกค้าให้จบก่อน!");
            return;
        }

        isMoneyOnCounter = false;
        hasReceivedMoney = true;

        if (customerHandMoneyObj != null) customerHandMoneyObj.SetActive(false);
        WarningManager.Show($"รับเงินมา {customerPaidAmount} บาท ไปกดที่เครื่องคิดเงินเพื่อทอนเงิน!");
    }

    // ขั้นที่ 3: ผู้เล่นคลิกที่วัตถุเครื่องคิดเงินในฉาก
    public void OpenRegister()
    {
        if (isCheckoutOpen) return;

        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            WarningManager.Show("ไฟดับ! เครื่องคิดเงินใช้งานไม่ได้ ซ่อมไฟก่อน!");
            return;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            WarningManager.Show("คุยกับลูกค้าให้จบก่อน!");
            return;
        }

        if (!hasReceivedMoney)
        {
            if (isMoneyOnCounter)
                WarningManager.Show("หยิบเงินจากลูกค้าก่อน!");
            else
                WarningManager.Show("ยังไม่ได้วางสินค้าบนถาดและรับเงินจากลูกค้า!");
            return;
        }

        isCheckoutOpen = true;
        if (checkoutPanel != null) checkoutPanel.SetActive(true);

        ResetChange();
        UpdateRegisterDisplay();
    }

    public void CloseCheckout()
    {
        isCheckoutOpen = false;
        if (checkoutPanel != null) checkoutPanel.SetActive(false);
    }

    #region --- ระบบหยิบเงินทอนจากลิ้นชัก (ผูกกับ Button OnClick ของแต่ละช่องเงิน) ---

    // ผูกกับปุ่มแบงก์และเหรียญ เช่น ส่งค่า 1, 5, 10, 20, 50, 100, 500
    public void AddChange(int amount)
    {
        if (!isCheckoutOpen) return;

        currentChangeToGive += amount;
        changeHistory.Push(amount);
        UpdateGivenChangeUI();
    }

    // ปุ่ม "หยิบออก 1 ชิ้น" (Undo) เผื่อกดหยิบแบงก์เกิน
    public void UndoLastChange()
    {
        if (!isCheckoutOpen || changeHistory.Count == 0) return;

        int lastAmount = changeHistory.Pop();
        currentChangeToGive = Mathf.Max(0, currentChangeToGive - lastAmount);
        UpdateGivenChangeUI();
    }

    // ปุ่ม "ล้างเงินทอน" (Clear) เก็บเงินทอนคืนลิ้นชักทั้งหมดเป็น 0
    public void ResetChange()
    {
        currentChangeToGive = 0;
        changeHistory.Clear();
        UpdateGivenChangeUI();
    }

    // ปุ่ม "ยืนยันทอนเงิน" (Confirm) ส่งเงินทอนและสินค้าให้ลูกค้า
    public void ConfirmCheckoutAndGiveItem()
    {
        if (DialogueManager.Instance == null || !hasReceivedMoney) return;

        int finalTotal = actualOrderTotal;
        int finalPaid = customerPaidAmount;
        int finalGiven = currentChangeToGive;
        bool finalGhost = isGhostMoney;

        ResetForNewCustomer();
        DialogueManager.Instance.CompleteSaleWithChange(finalTotal, finalPaid, finalGiven, finalGhost);
    }

    #endregion

    #region --- อัปเดตหน้าจอ & สุ่มเงิน ---

    private void UpdateRegisterDisplay()
    {
        if (totalPriceText != null)
            totalPriceText.text = $"ค่าสินค้า : {actualOrderTotal} บาท";

        if (receivedMoneyText != null)
            receivedMoneyText.text = $"ได้รับเงินมา : {customerPaidAmount} บาท";

        if (requiredChangeText != null)
            requiredChangeText.text = $"ต้องทอน : {requiredChangeAmount} บาท";

        UpdateGivenChangeUI();
    }

    private void UpdateGivenChangeUI()
    {
        if (givenChangeText != null)
            givenChangeText.text = $"หยิบทอน : {currentChangeToGive} บาท";
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

    private Sprite GetBanknoteSprite(int amount, bool ghost)
    {
        BanknoteVisual matched = null;
        foreach (var note in banknoteVisuals)
        {
            if (note.amount <= amount && (matched == null || note.amount > matched.amount))
            {
                matched = note;
            }
        }
        if (matched == null && banknoteVisuals.Count > 0) matched = banknoteVisuals[0];
        if (matched == null) return null;

        return (ghost && matched.ghostSprite != null) ? matched.ghostSprite : matched.normalSprite;
    }

    #endregion
}