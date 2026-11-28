using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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

    [Header("เงินที่ลูกค้ายื่น")]
    public GameObject customerHandMoneyObj;
    public SpriteRenderer customerHandMoneyRenderer;
    public Image customerHandMoneyUIImage;

    [Header("หน้าต่างเครื่องคิดเงิน")]
    public GameObject checkoutPanel;
    [HideInInspector] public bool isCheckoutOpen = false;
    [HideInInspector] public bool isMoneyOnCounter = false;
    [HideInInspector] public bool hasReceivedMoney = false;

    [Header("ข้อความบนหน้าจอเครื่องคิดเงิน")]
    public TextMeshProUGUI totalPriceText;
    public TextMeshProUGUI receivedMoneyText;
    public TextMeshProUGUI requiredChangeText;
    public TextMeshProUGUI givenChangeText;
    public Image registerBanknotePreview;

    [Header("ตั้งค่าเงินคนและเงินผี")]
    public List<BanknoteVisual> banknoteVisuals = new List<BanknoteVisual>();
    [Range(0f, 1f)] public float ghostMoneyChance = 0.35f;

    [Header("จังหวะปิดเครื่องอัตโนมัติ & เสียง")]
    [Tooltip("หน่วงเวลาสั้นๆ ให้เห็นยอดสุดท้ายก่อนเครื่องปิด")]
    public float autoCloseDelay = 1f;
    [Tooltip("หน่วงเวลาสั้นๆ กรณีลูกค้าจ่ายเงินพอดีแล้วปิดเครื่องอัตโนมัติ")]
    public float zeroChangeAutoCloseDelay = 2.5f;
    public AudioSource audioSource;
    public AudioClip openRegisterSound;   
    public AudioClip moneyClickSound;    
    public AudioClip changeCompleteSound;
    public AudioClip overChangeSound;

    private int actualOrderTotal = 0;
    private int customerPaidAmount = 0;
    private int requiredChangeAmount = 0;
    private int currentChangeToGive = 0;
    private bool isGhostMoney = false;
    private bool hasRolledPaymentForCurrentCustomer = false;
    private bool isCompletingCheckout = false;

    private Coroutine autoCompleteCoroutine;
    private Stack<int> changeHistory = new Stack<int>();

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (checkoutPanel != null) checkoutPanel.SetActive(false);
        if (customerHandMoneyObj != null) customerHandMoneyObj.SetActive(false);
    }

    public void ResetForNewCustomer()
    {
        if (autoCompleteCoroutine != null)
        {
            StopCoroutine(autoCompleteCoroutine);
            autoCompleteCoroutine = null;
        }

        isCompletingCheckout = false;
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

        isCheckoutOpen = false;
        if (checkoutPanel != null) checkoutPanel.SetActive(false);

        if (RegisterAnomaly.Instance != null) RegisterAnomaly.Instance.ResetAnomalyState();
    }

    public void HideHandMoney()
    {
        isMoneyOnCounter = false;
        if (customerHandMoneyObj != null) customerHandMoneyObj.SetActive(false);
    }

    public void PrepareCustomerPayment(List<string> expectedItems, bool customerHasAnomaly)
    {
        actualOrderTotal = (MoneyManager.Instance != null)
            ? MoneyManager.Instance.CalculateTotal(expectedItems)
            : 0;

        if (!hasRolledPaymentForCurrentCustomer)
        {
            hasRolledPaymentForCurrentCustomer = true;
            customerPaidAmount = GenerateValidCustomerPayment(actualOrderTotal);
            isGhostMoney = customerHasAnomaly && (Random.value <= ghostMoneyChance);
        }

        requiredChangeAmount = Mathf.Max(0, customerPaidAmount - actualOrderTotal);
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

        if (RegisterAnomaly.Instance != null)
        {
            if (requiredChangeAmount > 0)
                RegisterAnomaly.Instance.RollAnomalyForCustomer();
            else
                RegisterAnomaly.Instance.ResetAnomalyState();
        }
    }

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
        WarningManager.Show($"รับเงินมา {customerPaidAmount} บาท กดที่เครื่องคิดเงินเพื่อทอนเงิน!");
    }

    public void OpenRegister()
    {
        if (isCheckoutOpen || isCompletingCheckout) return;
        if (audioSource != null && openRegisterSound != null)
        {
            audioSource.PlayOneShot(openRegisterSound);
        }

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

        currentChangeToGive = 0;
        changeHistory.Clear();
        UpdateRegisterDisplay();

        if (requiredChangeAmount == 0)
        {
            WarningManager.Show("ลูกค้าจ่ายเงินพอดี ไม่ต้องทอน!");
            StartAutoComplete(zeroChangeAutoCloseDelay, false);
            return;
        }

        if (RegisterAnomaly.Instance != null) RegisterAnomaly.Instance.OnRegisterOpened();
    }

    public void CloseCheckout()
    {
        if (isCompletingCheckout) return;
        isCheckoutOpen = false;
        if (checkoutPanel != null) checkoutPanel.SetActive(false);
    }

    #region --- ระบบหยิบเงินทอนจากลิ้นชัก (ทอนพอดีหรือเกินจะปิดเครื่องอัตโนมัติ) ---

    public void AddChange(int amount)
    {
        if (RegisterAnomaly.Instance != null && RegisterAnomaly.Instance.isRebooting) return;
        if (!isCheckoutOpen || isCompletingCheckout) return;

        currentChangeToGive += amount;
        changeHistory.Push(amount);
        UpdateGivenChangeUI();

        if (audioSource != null && moneyClickSound != null)
        {
            audioSource.PlayOneShot(moneyClickSound);
        }

        if (currentChangeToGive == requiredChangeAmount)
        {
            StartAutoComplete(autoCloseDelay, false);
        }
        else if (currentChangeToGive > requiredChangeAmount)
        {
            StartAutoComplete(autoCloseDelay, true);
        }
    }

    public void UndoLastChange()
    {
        if (RegisterAnomaly.Instance != null && RegisterAnomaly.Instance.isRebooting) return;
        if (!isCheckoutOpen || isCompletingCheckout || changeHistory.Count == 0) return;

        int lastAmount = changeHistory.Pop();
        currentChangeToGive = Mathf.Max(0, currentChangeToGive - lastAmount);
        UpdateGivenChangeUI();
    }

    public void ResetChange()
    {
        if (RegisterAnomaly.Instance != null && RegisterAnomaly.Instance.isRebooting) return;
        if (isCompletingCheckout) return;

        currentChangeToGive = 0;
        changeHistory.Clear();
        UpdateGivenChangeUI();
    }

    public void ConfirmCheckoutAndGiveItem()
    {
        if (RegisterAnomaly.Instance != null && RegisterAnomaly.Instance.isRebooting) return;
        if (isCompletingCheckout || DialogueManager.Instance == null || !hasReceivedMoney) return;

        CompleteCheckoutInternal();
    }

    private void StartAutoComplete(float delay, bool isOverChange)
    {
        if (isCompletingCheckout) return;
        isCompletingCheckout = true;

        if (audioSource != null)
        {
            AudioClip clip = (isOverChange && overChangeSound != null)
                ? overChangeSound
                : changeCompleteSound;

            if (clip != null) audioSource.PlayOneShot(clip);
        }

        autoCompleteCoroutine = StartCoroutine(AutoCompleteRoutine(delay));
    }

    private IEnumerator AutoCompleteRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        autoCompleteCoroutine = null;
        CompleteCheckoutInternal();
    }

    private void CompleteCheckoutInternal()
    {
        if (DialogueManager.Instance == null || !hasReceivedMoney)
        {
            isCompletingCheckout = false;
            return;
        }

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

        int[] denominations = { 20, 50, 100, 500 };
        foreach (int note in denominations)
        {
            if (note >= totalPrice && !validPayments.Contains(note))
            {
                validPayments.Add(note);
            }
        }

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