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
    public GameObject agreeButton;
    public GameObject disagreeButton;

    [Header("ระบบหลัก")]
    public CustomerWalker customerWalker;

    [Header("ระบบหยิบสินค้า & ถุง")]
    public List<string> expectedItems = new List<string>();
    public List<string> itemsInBag = new List<string>();
    public int maxBagCapacity = 2;

    [HideInInspector] public bool hasBag = false;
    [HideInInspector] public bool isBagOnTray = false;
    [HideInInspector] public bool isWaitingForOrder = false;

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

    void Awake() { Instance = this; }

    void Start()
    {
        if (customerWalker == null) customerWalker = FindFirstObjectByType<CustomerWalker>();
        if (startTalkButton != null) startTalkBtnText = startTalkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (centerTalismanSprite == null && activeTalismanUI != null) centerTalismanSprite = activeTalismanUI.GetComponent<SpriteRenderer>();

        ResetAllUI();
    }

    void Update()
    {
        if (!isDialogueActive) return;

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

    public void SetupNewCustomer(CustomerDialogue data)
    {
        currentData = data;
        sentences = data.sentences;

        expectedItems.Clear();
        if (data.requiredItems != null) expectedItems.AddRange(data.requiredItems);

        customerHasAnomaly = data.hasAnomaly;
        customerWeakness = data.weaknessType;
        cameraTurnCount = 0;
    }

    public void OnCameraTurnedBack()
    {
        if (!isWaitingForOrder || currentData == null) return;
        if (!hasBag || itemsInBag.Count == 0) return;

        List<string> originalExpectedItems = new List<string>(currentData.requiredItems);
        bool hasAllOriginalItems = true;

        foreach (string required in originalExpectedItems)
        {
            if (!itemsInBag.Contains(required))
            {
                hasAllOriginalItems = false;
                break;
            }
        }

        if (!hasAllOriginalItems) return;

        cameraTurnCount++;

        if (currentData.isTrickOrder)
        {
            if (cameraTurnCount == 1)
            {
                isWaitingForOrder = false;

                if (currentData.additionalItems != null)
                    expectedItems.AddRange(currentData.additionalItems);

                if (currentData.trickSentences != null && currentData.trickSentences.Length > 0)
                {
                    StartDialogue(currentData.trickSentences);
                }
            }
            else if (cameraTurnCount == 2 && currentData.forceAnomalyAfterTrick)
            {
                if (CustomerAnomalyVisual.CurrentActive != null)
                {
                    if (Random.value > 0.5f)
                    {
                        CustomerAnomalyVisual.CurrentActive.ForceRevealAnomaly();
                    }
                    else
                    {
                        CustomerAnomalyVisual.CurrentActive.TriggerAggressiveGlitch();
                    }
                }
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
            if (disagreeButton != null) disagreeButton.SetActive(false);
        }
        else
        {
            index = 0;
            if (disagreeButton != null) disagreeButton.SetActive(true);
            if (agreeButton != null) agreeButton.SetActive(true);
        }
        StartTyping(sentences[index]);
    }

    public void StartDialogue(string[] newSentences)
    {
        sentences = newSentences;
        index = 0;
        if (choicePanel != null) choicePanel.SetActive(false);
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (hintText != null) hintText.SetActive(true);

        isDialogueActive = true;
        isWaitingForChoice = false;

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
            if (startTalkButton != null) startTalkButton.SetActive(false);
        }
    }

    private void PreviousSentence()
    {
        isWaitingForChoice = false;
        if (choicePanel != null) choicePanel.SetActive(false);
        index--;
        StartTyping(sentences[index]);
    }

    public void ChooseAgree()
    {
        CloseDialogueBox();
        ShowStartTalkButton();
        isWaitingForOrder = true;
    }

    public void ChooseDisagree()
    {
        EndCustomerInteraction();
        if (customerWalker != null) customerWalker.WalkAway();
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

    private void ResetAllUI()
    {
        CloseDialogueBox();
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (giveItemButton != null) giveItemButton.SetActive(false);
        if (activeBagUI != null) activeBagUI.SetActive(false);
        if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);
        if (activeTalismanUI != null) activeTalismanUI.SetActive(false);
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

    public void PickUpBag()
    {
        if (!isWaitingForOrder) { WarningManager.Show("ยังไม่มีออเดอร์!"); return; }
        if (isBagOnTray) { WarningManager.Show("มีถุงวางอยู่บนถาดแล้ว!"); return; }
        if (isHoldingTalisman) { WarningManager.Show("มือไม่ว่าง! กำลังถือยันต์อยู่"); return; }

        if (hasBag)
        {
            hasBag = false;
            itemsInBag.Clear();
            if (activeBagUI != null) activeBagUI.SetActive(false);
            return;
        }

        hasBag = true;
        if (activeBagUI != null) activeBagUI.SetActive(true);
    }

    public void PickUpItem(string itemName)
    {
        if (!isWaitingForOrder || !hasBag) return;
        if (itemsInBag.Count >= maxBagCapacity)
        {
            WarningManager.Show("ถุงเต็มแล้ว!");
            return;
        }
        itemsInBag.Add(itemName);
    }

    public void PlaceBagOnTray()
    {
        if (!isWaitingForOrder || !hasBag || itemsInBag.Count == 0 || isBagOnTray) return;

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