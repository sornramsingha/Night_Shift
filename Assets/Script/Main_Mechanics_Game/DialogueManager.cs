using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public partial class DialogueManager : MonoBehaviour
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

    [Header("ตั้งค่ากรอบข้อความและการสั่น")]
    public RectTransform dialogueBoxRect;
    public float verticalPadding = 40f;
    [Tooltip("ความสูงตั้งต้นของกรอบ")]
    public float minBoxHeight = 0f;
    public float normalShakeAmount = 2.5f;  
    public float normalShakeSpeed = 6f;     
    public float anomalyShakeAmount = 6f;   

    private Vector2 originalBoxPos;
    private Vector2 originalBoxSize;
    private Vector2 originalTextPos;

    [Header("ระบบหลอดเวลา UI")]
    public GameObject patienceUIContainer;
    public UnityEngine.UI.Image patienceFillBar;
    public Gradient patienceColorGradient;

    [Header("ระบบอนิเมชั่นลูกค้า")]
    public Animator customerAnimatorComp;
    private SpriteRenderer customerSpriteRenderer;

    private float currentPatience;
    private float customerMaxPatience;
    private bool isTimerRunning = false;
    private int currentMoodStage = 0;

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
    [HideInInspector] public bool isDialogueActive = false;

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

        if (dialogueBoxRect != null)
        {
            originalBoxPos = dialogueBoxRect.anchoredPosition;
            originalBoxSize = dialogueBoxRect.sizeDelta;
            if (minBoxHeight <= 0f) minBoxHeight = originalBoxSize.y;
        }

        if (dialogueText != null)
        {
            originalTextPos = dialogueText.rectTransform.anchoredPosition;
        }

        if (centerTalismanSprite == null && activeTalismanUI != null) centerTalismanSprite = activeTalismanUI.GetComponent<SpriteRenderer>();

        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        if (trickBubbleButton != null) trickBubbleButton.SetActive(false);

        if (customerAnimatorComp != null)
        {
            customerSpriteRenderer = customerAnimatorComp.GetComponent<SpriteRenderer>();
            customerAnimatorComp.gameObject.SetActive(false);
        }

        ResetAllUI();
    }

    void Update()
    {
        UpdateDialogueInputAndShake();
        UpdatePatienceTimer();
    }

    private void UpdateDialogueInputAndShake()
    {
        if (!isDialogueActive) return;

        if (dialogueBoxRect != null)
        {
            float patienceRatio = (customerMaxPatience > 0f) ? (currentPatience / customerMaxPatience) : 1f;
            Vector2 shakeOffset;

            if (customerHasAnomaly && patienceRatio < 0.5f)
            {
                float anomalyProgress = (0.5f - patienceRatio) * 2f;
                float currentAnomalyShake = Mathf.Lerp(anomalyShakeAmount * 0.6f, anomalyShakeAmount, anomalyProgress);

                float rx = Random.Range(-currentAnomalyShake, currentAnomalyShake);
                float ry = Random.Range(-currentAnomalyShake, currentAnomalyShake);
                shakeOffset = new Vector2(rx, ry);
            }
            else
            {
                float ry = Mathf.Sin(Time.unscaledTime * normalShakeSpeed) * normalShakeAmount;
                shakeOffset = new Vector2(0f, ry);
            }

            dialogueBoxRect.anchoredPosition = originalBoxPos + shakeOffset;
            if (dialogueText != null && dialogueText.transform.IsChildOf(dialogueBoxRect))
            {
                dialogueText.rectTransform.anchoredPosition = originalTextPos - shakeOffset;
            }
        }

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

    private void AdjustDialogueBoxSize()
    {
        if (dialogueBoxRect == null || dialogueText == null) return;

        dialogueText.ForceMeshUpdate();
        float textHeight = dialogueText.preferredHeight;
        float targetHeight = Mathf.Max(minBoxHeight, textHeight + verticalPadding);

        dialogueBoxRect.sizeDelta = new Vector2(originalBoxSize.x, targetHeight);

        if (hintText != null)
        {
            RectTransform hintRect = hintText.GetComponent<RectTransform>();
            if (hintRect != null)
            {
                float bottomEdgeY = originalBoxPos.y - (targetHeight * dialogueBoxRect.pivot.y);
                float gapBelowBox = 40f;
                hintRect.anchoredPosition = new Vector2(hintRect.anchoredPosition.x, bottomEdgeY - gapBelowBox);
            }
        }
    }

    private void UpdatePatienceTimer()
    {
        if (!isTimerRunning) return;
        if (LightRepairMinigame.Instance != null && LightRepairMinigame.Instance.isPlaying) return;

        if (Time.timeScale == 0f)
        {
            if (patienceUIContainer != null && patienceUIContainer.activeSelf)
                patienceUIContainer.SetActive(false);
            return;
        }

        currentPatience -= Time.deltaTime;
        UpdateCustomerMoodVisual();

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

        if (customerAnimatorComp != null)
        {
            customerAnimatorComp.runtimeAnimatorController = null;
            customerAnimatorComp.enabled = false;
        }

        if (customerSpriteRenderer != null)
        {
            customerSpriteRenderer.sprite = data.customerSprite;
        }

        customerMaxPatience = data.maxPatienceTime;
        currentPatience = customerMaxPatience;
        isTimerRunning = false;
        currentMoodStage = 0;

        expectedItems.Clear();
        lockedBaseOrderItems.Clear();
        if (data.requiredItems != null)
        {
            expectedItems.AddRange(data.requiredItems);
            lockedBaseOrderItems.AddRange(data.requiredItems);
        }
        maxBagCapacity = expectedItems.Count;

        customerHasAnomaly = data.hasAnomaly;
        customerWeakness = data.weaknessType;
        cameraTurnCount = 0;

        hasPendingTrickOrder = data.isTrickOrder;
        isReadyToTalkTrick = false;
        if (trickBubbleButton != null) trickBubbleButton.SetActive(false);

        if (Random.value <= 0.3f && ShopLightController.Instance != null)
        {
            ShopLightController.Instance.TriggerTemporaryPanic(3.0f);
        }

        if (CheckoutManager.Instance != null)
        {
            CheckoutManager.Instance.ResetForNewCustomer();
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
            if (isWaitingForOrder && !isBagOnTray) ShowStartTalkButton();
            else if (isBagOnTray && giveItemButton != null) giveItemButton.SetActive(true);

            if (isTimerRunning && patienceUIContainer != null)
                patienceUIContainer.SetActive(true);
        }
    }

    public void BeginConversation()
    {
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (hintText != null) hintText.SetActive(true);

        isTimerRunning = false;
        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        if (dialogueBoxRect != null) dialogueBoxRect.gameObject.SetActive(true);

        if (currentData != null)
        {
            PlayCustomerAnimator(currentData.customerAnimator);
        }

        isDialogueActive = true;
        isWaitingForChoice = false;
        index = isWaitingForOrder ? sentences.Length - 1 : 0;

        StartTyping(sentences[index]);
    }

    public void StartDialogue(string[] newSentences)
    {
        sentences = newSentences;
        index = 0;

        isTimerRunning = false;
        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        if (dialogueBoxRect != null) dialogueBoxRect.gameObject.SetActive(true);

        if (currentData != null)
        {
            PlayCustomerAnimator(currentData.customerAnimator);
        }

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
        AdjustDialogueBoxSize();

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
                acceptButtonTextUI.text = string.IsNullOrEmpty(currentData.acceptButtonText) ? "ตกลง" : currentData.acceptButtonText;
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
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.rectTransform.anchoredPosition = originalTextPos;
        }
        if (choicePanel != null) choicePanel.SetActive(false);
        if (hintText != null) hintText.SetActive(false);

        if (dialogueBoxRect != null)
        {
            dialogueBoxRect.anchoredPosition = originalBoxPos;
            dialogueBoxRect.sizeDelta = originalBoxSize;
            dialogueBoxRect.gameObject.SetActive(false);
        }

        if (currentData != null && isWaitingForOrder)
        {
            UpdateCustomerMoodVisual(true);
        }
        else
        {
            SetCustomerStaticIdle();
        }
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
            if (Random.value <= 0.20f && ShopLightController.Instance != null)
            {
                ShopLightController.Instance.TriggerLightBreakdown();
            }
        }

        if (currentData != null)
        {
            isTimerRunning = true;
            if (patienceUIContainer != null) patienceUIContainer.SetActive(true);
        }
    }

    private void TriggerCustomerTimeout()
    {
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
        isTimerRunning = false;
        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        if (CheckoutManager.Instance != null) CheckoutManager.Instance.ResetForNewCustomer();

        ResetAllUI();
        hasBag = false;
        isBagOnTray = false;
        itemsInBag.Clear();
        isWaitingForOrder = false;
        if (dialogueBoxRect != null) dialogueBoxRect.gameObject.SetActive(false);
        ClearTalismanInHand();
    }

    private void UpdateCustomerMoodVisual(bool forceUpdate = false)
    {
        if (currentData == null || isDialogueActive) return;
        if (CustomerAnomalyVisual.CurrentActive != null && CustomerAnomalyVisual.CurrentActive.IsOverridingVisual) return;

        float ratio = (customerMaxPatience > 0f) ? (currentPatience / customerMaxPatience) : 1f;
        int targetStage = 0;

        if (ratio <= 0.3f) targetStage = 2;
        else if (ratio <= 0.6f) targetStage = 1;
        else targetStage = 0;

        if (!forceUpdate && targetStage == currentMoodStage) return;
        currentMoodStage = targetStage;

        if (targetStage == 2 && currentData.angryAnimator != null)
        {
            PlayCustomerAnimator(currentData.angryAnimator);
        }
        else if (targetStage == 1 && currentData.annoyedAnimator != null)
        {
            PlayCustomerAnimator(currentData.annoyedAnimator);
        }
        else
        {
            SetCustomerStaticIdle();
        }
    }

    public void RefreshCustomerVisual()
    {
        if (currentData == null) return;
        if (CustomerAnomalyVisual.CurrentActive != null && CustomerAnomalyVisual.CurrentActive.IsOverridingVisual) return;

        if (isDialogueActive && currentData.customerAnimator != null)
        {
            PlayCustomerAnimator(currentData.customerAnimator);
        }
        else if (isWaitingForOrder)
        {
            UpdateCustomerMoodVisual(true);
        }
        else
        {
            SetCustomerStaticIdle();
        }
    }

    private void PlayCustomerAnimator(RuntimeAnimatorController controller)
    {
        if (customerAnimatorComp == null || controller == null) return;
        if (CustomerAnomalyVisual.CurrentActive != null && CustomerAnomalyVisual.CurrentActive.IsOverridingVisual) return;

        customerAnimatorComp.gameObject.SetActive(true);
        customerAnimatorComp.runtimeAnimatorController = controller;
        customerAnimatorComp.enabled = true;
        customerAnimatorComp.speed = 1f;
    }

    private void SetCustomerStaticIdle()
    {
        if (CustomerAnomalyVisual.CurrentActive != null && CustomerAnomalyVisual.CurrentActive.IsOverridingVisual) return;

        if (customerAnimatorComp != null)
        {
            customerAnimatorComp.runtimeAnimatorController = null;
            customerAnimatorComp.enabled = false;
        }

        if (customerSpriteRenderer != null && currentData != null)
        {
            customerSpriteRenderer.sprite = currentData.customerSprite;
        }
    }
}