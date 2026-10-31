using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI และข้อความ")]
    public TextMeshProUGUI dialogueText;
    public GameObject choicePanel;
    public GameObject startTalkButton;
    public GameObject hintText;
    public GameObject choicePanels;
    public GameObject acceptOrderButton;
    public TextMeshProUGUI acceptButtonTextUI;

    [Header("ระบบหลอดเวลา UI")]
    public GameObject patienceUIContainer;
    public UnityEngine.UI.Image patienceFillBar;
    public Gradient patienceColorGradient;

    private float currentPatience;
    private float customerMaxPatience;
    private bool isTimerRunning = false;

    [Header("UI ปุ่มออเดอร์เสริม")]
    public GameObject trickBubbleButton;

    [Header("UI เปลี่ยนถุง")]
    public GameObject bagConfirmPanel;
    [HideInInspector] public bool isConfirmingBagReset = false;

    [Header("ระบบหลัก")]
    public CustomerWalker customerWalker;
    [HideInInspector] public bool isFacingFront = true;

    [Header("ระบบหยิบสินค้า & ถุง")]
    public List<string> expectedItems = new List<string>();
    public List<string> itemsInBag = new List<string>();
    private List<string> lockedBaseOrderItems = new List<string>();
    public int maxBagCapacity = 1;

    [HideInInspector] public bool hasBag = false;
    [HideInInspector] public bool isBagOnTray = false;
    [HideInInspector] public bool isWaitingForOrder = false;
    private bool hasRolledLightEvent = false;

    [Header("ระบบยันต์")]
    public bool isHoldingTalisman = false;
    public TalismanType heldTalisman = TalismanType.None;
    public GameObject activeTalismanItem;

    [Header("UI ถุงและยันต์กลางจอ")]
    public GameObject activeBagUI;
    public Transform bagDropTarget;
    public GameObject giveItemButton;
    public GameObject bagOnTrayUI;
    public GameObject activeTalismanUI;
    public SpriteRenderer centerTalismanSprite;

    [Header("ตั้งค่าการพิมพ์")]
    public float typingSpeed = 0.05f;
    [HideInInspector] public string[] sentences;

    private int index;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;
    private bool isDialogueActive = false;

    private Coroutine typingCoroutine;
    private TextMeshProUGUI startTalkBtnText;

    private CustomerDialogue currentData;
    private int cameraTurnCount = 0;
    private bool customerHasAnomaly = false;
    private TalismanType customerWeakness = TalismanType.None;

    private bool hasPendingTrickOrder = false;
    private bool isReadyToTalkTrick = false;

    void Awake() { Instance = this; }

    void Start()
    {
        if (customerWalker == null) customerWalker = FindFirstObjectByType<CustomerWalker>();
        if (startTalkButton != null) startTalkBtnText = startTalkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (centerTalismanSprite == null && activeTalismanUI != null) centerTalismanSprite = activeTalismanUI.GetComponent<SpriteRenderer>();

        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        if (trickBubbleButton != null) trickBubbleButton.SetActive(false);

        ResetAllUI();
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetMouseButtonDown(1) && index > 0 && !isWaitingForOrder)
            {
                PreviousSentence();
            }

            if (Input.GetMouseButtonDown(0) && !isWaitingForChoice)
            {
                if (isTyping)
                {
                    StopTyping();
                    dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
                }
                else
                {
                    NextSentence();
                }
            }
        }

        if (!isTimerRunning) return;
        if (LightRepairMinigame.Instance != null && LightRepairMinigame.Instance.isPlaying) return;
        currentPatience -= Time.deltaTime;

        if (patienceUIContainer != null)
        {
            patienceUIContainer.SetActive(isFacingFront);

            if (isFacingFront && patienceFillBar != null && customerMaxPatience > 0)
            {
                float fillRatio = currentPatience / customerMaxPatience;
                patienceFillBar.fillAmount = fillRatio;
                patienceFillBar.color = patienceColorGradient.Evaluate(fillRatio);
            }
        }
        if (currentPatience <= 0)
        {
            TriggerCustomerTimeout();
        }
    }

    public void SetupNewCustomer(CustomerDialogue data)
    {
        hasRolledLightEvent = false;
        currentData = data;
        sentences = data.sentences;

        expectedItems.Clear();
        lockedBaseOrderItems.Clear();
        if (data.requiredItems != null) expectedItems.AddRange(data.requiredItems);
        maxBagCapacity = expectedItems.Count;

        customerHasAnomaly = data.hasAnomaly;
        customerWeakness = data.weaknessType;
        cameraTurnCount = 0;

        hasPendingTrickOrder = data.isTrickOrder;
        isReadyToTalkTrick = false;
        if (trickBubbleButton != null) trickBubbleButton.SetActive(false);

        if (Random.value <= 0.3f)
        {
            if (ShopLightController.Instance != null)
            {
                ShopLightController.Instance.TriggerTemporaryPanic(3.0f);
            }
        }
    }

    public void OnCameraTurnedBack()
    {
        if (!isWaitingForOrder || currentData == null) return;
        if (itemsInBag.Count == 0) return;

        cameraTurnCount++;
        if (hasPendingTrickOrder && cameraTurnCount == 1)
        {
            isReadyToTalkTrick = true;
            if (trickBubbleButton != null) trickBubbleButton.SetActive(true);

            if (isBagOnTray)
            {
                isBagOnTray = false;
                hasBag = true;
                if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);
                if (activeBagUI != null) activeBagUI.SetActive(true);
                if (giveItemButton != null) giveItemButton.SetActive(false);
            }
        }
        else if (cameraTurnCount == 2 && currentData.isTrickOrder && currentData.forceAnomalyAfterTrick)
        {
            if (CustomerAnomalyVisual.CurrentActive != null)
            {
                if (Random.value > 0.5f) CustomerAnomalyVisual.CurrentActive.ForceRevealAnomaly();
                else CustomerAnomalyVisual.CurrentActive.TriggerAggressiveGlitch();
            }
        }
    }

    public void TriggerTrickDialogue()
    {
        if (trickBubbleButton != null) trickBubbleButton.SetActive(false);

        isReadyToTalkTrick = false;
        hasPendingTrickOrder = false;
        isWaitingForOrder = false;
        lockedBaseOrderItems = new List<string>(itemsInBag);

        if (currentData.additionalItems != null)
        {
            expectedItems.AddRange(currentData.additionalItems);
            maxBagCapacity = expectedItems.Count;
        }

        if (currentData.trickSentences != null && currentData.trickSentences.Length > 0)
        {
            StartDialogue(currentData.trickSentences);
        }
    }

    public void ToggleMinigameUI(bool isPlayingMinigame)
    {
        if (isPlayingMinigame)
        {
            if (startTalkButton != null) startTalkButton.SetActive(false);
            if (giveItemButton != null) giveItemButton.SetActive(false);
            if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        }
        else
        {
            if (isWaitingForOrder && !isBagOnTray)
            {
                ShowStartTalkButton();
            }
            else if (isBagOnTray)
            {
                if (giveItemButton != null) giveItemButton.SetActive(true);
            }

            if (isTimerRunning && patienceUIContainer != null)
            {
                patienceUIContainer.SetActive(true);
            }
        }
    }

    #region --- ระบบบทสนทนา (Dialogue System) ---

    public void BeginConversation()
    {
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (hintText != null) hintText.SetActive(true);

        isDialogueActive = true;
        isWaitingForChoice = false;

        if (isWaitingForOrder)
        {
            index = sentences.Length - 1;
        }
        else
        {
            index = 0;
        }
        StartTyping(sentences[index]);
    }

    public void StartDialogue(string[] newSentences)
    {
        sentences = newSentences;
        index = 0;

        isDialogueActive = true;
        isWaitingForChoice = false;

        if (choicePanel != null) choicePanel.SetActive(false);
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (hintText != null) hintText.SetActive(true);
        if (dialogueText != null) dialogueText.text = "";

        StartTyping(sentences[index]);
    }

    public void ShowStartTalkButton()
    {
        if (startTalkButton != null)
        {
            startTalkButton.SetActive(true);
            if (startTalkBtnText != null) startTalkBtnText.text = "คุยกับลูกค้า";
        }
    }

    private void StartTyping(string sentence)
    {
        StopTyping();
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    private void StopTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = sentence;
        dialogueText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        for (int visibleCount = 0; visibleCount <= totalVisibleCharacters; visibleCount++)
        {
            dialogueText.maxVisibleCharacters = visibleCount;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;
            StartTyping(sentences[index]);
        }
        else
        {
            isWaitingForChoice = true;

            if (choicePanel != null) choicePanel.SetActive(true);
            if (acceptOrderButton != null) acceptOrderButton.SetActive(true);
            if (acceptButtonTextUI != null && currentData != null)
            {
                string btnText = string.IsNullOrEmpty(currentData.acceptButtonText) ? "ตกลง" : currentData.acceptButtonText;
                acceptButtonTextUI.text = btnText;
            }
        }
    }

    private void PreviousSentence()
    {
        isWaitingForChoice = false;
        index--;
        if (isWaitingForOrder) isWaitingForOrder = false;

        StartTyping(sentences[index]);
    }

    private void CloseDialogueBox()
    {
        StopTyping();
        isDialogueActive = false;
        isWaitingForChoice = false;
        if (dialogueText != null) dialogueText.text = "";
        if (choicePanel != null) choicePanel.SetActive(false);
        if (hintText != null) hintText.SetActive(false);
    }

    public void AcceptOrderOnClick()
    {
        isWaitingForChoice = false;
        isWaitingForOrder = true;

        ShowStartTalkButton();
        CloseDialogueBox();

        if (!hasRolledLightEvent)
        {
            hasRolledLightEvent = true;

            float rollDice = Random.value;
            if (rollDice <= 1f)
            {
                if (ShopLightController.Instance != null)
                {
                    ShopLightController.Instance.TriggerLightBreakdown();
                }
            }
        }

        if (currentData != null)
        {
            customerMaxPatience = currentData.maxPatienceTime;
            currentPatience = customerMaxPatience;
            isTimerRunning = true;

            if (patienceUIContainer != null) patienceUIContainer.SetActive(true);
        }
    }

    private void TriggerCustomerTimeout()
    {
        isTimerRunning = false;

        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);

        WarningManager.Show("ลูกค้ารอนานเกินไป!");

        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.ApplyPenalty(SanityManager.Instance.penaltyNormalWrong);
        }

        EndCustomerInteraction();
        if (customerWalker != null) customerWalker.WalkAway();
    }

    private void ResetAllUI()
    {
        CloseDialogueBox();
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (giveItemButton != null) giveItemButton.SetActive(false);
        if (activeBagUI != null) activeBagUI.SetActive(false);
        if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);
        if (activeTalismanUI != null) activeTalismanUI.SetActive(false);
        if (trickBubbleButton != null) trickBubbleButton.SetActive(false);
    }

    private void EndCustomerInteraction()
    {
        ResetAllUI();
        hasBag = false;
        isBagOnTray = false;
        itemsInBag.Clear();
        isWaitingForOrder = false;
        ClearTalismanInHand();
    }

    #endregion

    #region --- ระบบถุงและสินค้า (Bag & Item System) ---

    public void ShowBagResetConfirmation()
    {
        if (isConfirmingBagReset) return;
        isConfirmingBagReset = true;

        if (bagConfirmPanel != null) bagConfirmPanel.SetActive(true);
    }

    public void ConfirmBagResetYes()
    {
        isConfirmingBagReset = false;
        if (bagConfirmPanel != null) bagConfirmPanel.SetActive(false);

        ResetBagItems();
    }

    public void ConfirmBagResetNo()
    {
        isConfirmingBagReset = false;
        if (bagConfirmPanel != null) bagConfirmPanel.SetActive(false);
    }


    public void PickUpBag()
    {
        if (!isWaitingForOrder) { WarningManager.Show("ยังไม่มีออเดอร์!"); return; }
        if (isBagOnTray) { WarningManager.Show("มีถุงวางอยู่บนถาดแล้ว!"); return; }
        if (isHoldingTalisman) { WarningManager.Show("มือไม่ว่าง! กำลังถือยันต์อยู่"); return; }

        if (hasBag)
        {
            return;
        }

        hasBag = true;
        if (activeBagUI != null) activeBagUI.SetActive(true);
    }

    public void ResetBagItems()
    {
        if (hasBag)
        {
            itemsInBag.Clear();
            if (lockedBaseOrderItems.Count > 0)
            {
                itemsInBag.AddRange(lockedBaseOrderItems);
                WarningManager.Show("ออเดอร์แรกอยู่ในถุงแล้ว!");
            }
            else
            {
                hasBag = false;
                if (activeBagUI != null) activeBagUI.SetActive(false);
            }
        }
    }

    public bool PickUpItem(string itemName)
    {
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            WarningManager.Show("ไฟดับ! ต้องซ่อมไฟก่อน!");
            return false;
        }

        if (!isWaitingForOrder || !hasBag) return false;
        if (itemsInBag.Count >= maxBagCapacity)
        {
            WarningManager.Show("ถุงเต็มแล้ว!");
            return false;
        }
        itemsInBag.Add(itemName);
        return true;
    }

    public void PlaceBagOnTray()
    {
        if (!isWaitingForOrder || !hasBag || itemsInBag.Count == 0 || isBagOnTray) return;
        if (isReadyToTalkTrick)
        {
            WarningManager.Show("เหมือนลูกค้าจะสั่งอะไรเพิ่มนะ?");
            return;
        }
        if (itemsInBag.Count < maxBagCapacity)
        {
            WarningManager.Show("มีออเดอร์ใหม่ ของยังไม่ครบ!");
            return;
        }

        isBagOnTray = true;
        hasBag = false;

        CloseDialogueBox();
        if (activeBagUI != null) activeBagUI.SetActive(false);
        if (bagOnTrayUI != null) bagOnTrayUI.SetActive(true);
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (giveItemButton != null) giveItemButton.SetActive(true);
    }

    public void GiveItemToCustomer()
    {
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            WarningManager.Show("มืดขนาดนี้ จะขายของยังไง! ซ่อมไฟก่อน!");
            return;
        }

        if (!isBagOnTray) return;

        bool isExactlyCorrect = true;
        if (itemsInBag.Count != expectedItems.Count)
        {
            isExactlyCorrect = false;
        }
        else
        {
            List<string> tempExpected = new List<string>(expectedItems);
            foreach (string item in itemsInBag)
            {
                if (tempExpected.Contains(item)) tempExpected.Remove(item);
                else { isExactlyCorrect = false; break; }
            }
        }

        if (customerHasAnomaly)
        {
            Debug.Log("พลาด! ขายของให้สิ่งผิดปกติ");
            if (SanityManager.Instance != null) SanityManager.Instance.ApplyPenalty(SanityManager.Instance.penaltyAnomalyWrong);
        }
        else if (isExactlyCorrect)
        {
            Debug.Log("ขายถูกต้อง");
            if (SanityManager.Instance != null) SanityManager.Instance.ApplyReward(SanityManager.Instance.rewardCorrectSell);
        }
        else
        {
            Debug.Log("ขายของผิดพลาด");
            if (SanityManager.Instance != null) SanityManager.Instance.ApplyPenalty(SanityManager.Instance.penaltyNormalWrong);
        }

        EndCustomerInteraction();
        if (customerWalker != null) customerWalker.WalkAway();
        isTimerRunning = false;
        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
    }

    #endregion

    #region --- ระบบยันต์ (Talisman System) ---

    public void SetupAnomalyData(bool hasAnomaly, TalismanType weakness)
    {
        customerHasAnomaly = hasAnomaly;
        customerWeakness = weakness;
    }

    public void PickUpTalisman(TalismanType type, GameObject talismanObj, Sprite talismanSprite)
    {
        if (!isWaitingForOrder) { WarningManager.Show("ยังไม่จำเป็นต้องใช้ตอนนี้"); return; }
        if (hasBag) { WarningManager.Show("มือไม่ว่าง! กำลังถือถุงอยู่"); return; }

        if (isHoldingTalisman && heldTalisman == type)
        {
            ClearTalismanInHand();
            return;
        }

        isHoldingTalisman = true;
        heldTalisman = type;
        activeTalismanItem = talismanObj;

        if (activeTalismanUI != null)
        {
            activeTalismanUI.SetActive(true);
            if (centerTalismanSprite != null && talismanSprite != null)
            {
                centerTalismanSprite.sprite = talismanSprite;
            }
        }
    }

    public void UseTalismanOnCustomer()
    {
        if (!isFacingFront) return;

        if (!isHoldingTalisman || !isWaitingForOrder) return;

        if (activeTalismanItem != null) activeTalismanItem.SetActive(false);

        if (customerHasAnomaly && heldTalisman == customerWeakness)
        {
            Debug.Log("แปะยันต์ถูกต้อง! ขับไล่สำเร็จ");
            if (SanityManager.Instance != null) SanityManager.Instance.ApplyReward(SanityManager.Instance.rewardCorrectBanish);

            customerHasAnomaly = false;
            EndCustomerInteraction();
            if (customerWalker != null) customerWalker.Vanish();
        }
        else
        {
            Debug.Log("แปะยันต์ผิด! ลูกค้าโกรธเดินหนี");
            if (SanityManager.Instance != null) SanityManager.Instance.ApplyPenalty(SanityManager.Instance.penaltyTalismanWrong);

            EndCustomerInteraction();
            if (customerWalker != null) customerWalker.WalkAway();
            isTimerRunning = false;
            if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        }
    }

    private void ClearTalismanInHand()
    {
        isHoldingTalisman = false;
        heldTalisman = TalismanType.None;
        activeTalismanItem = null;
        if (activeTalismanUI != null) activeTalismanUI.SetActive(false);
    }

    #endregion
}