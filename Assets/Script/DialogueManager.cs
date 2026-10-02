using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI และข้อความ")]
    public TextMeshProUGUI dialogueText;
    public GameObject choicePanel;
    public GameObject startTalkButton;
    public GameObject hintText;
    public GameObject agreeButton;
    public GameObject disagreeButton;

    public DayManager dayManager;
    public CustomerWalker customerWalker;

    [Header("ระบบหยิบสินค้า & ถุง")]
    public string expectedItem;

    [HideInInspector]
    public bool hasBag = false;
    [HideInInspector]
    public bool isBagOnTray = false;

    public List<string> itemsInBag = new List<string>();
    public int maxBagCapacity = 1;

    [Header("UI ถุงกลางจอ")]
    public GameObject activeBagUI;
    public Transform bagDropTarget;
    public GameObject giveItemButton;
    public GameObject bagOnTrayUI;

    [HideInInspector]
    public bool isWaitingForOrder = false;

    [Header("ตั้งค่า")]
    public float typingSpeed = 0.05f;
    [TextArea(3, 5)]
    public string[] sentences;

    private int index;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;
    private bool isDialogueActive = false;

    void Start()
    {
        choicePanel.SetActive(false);
        hintText.SetActive(false);
        dialogueText.text = "";

        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (giveItemButton != null) giveItemButton.SetActive(false);
        if (activeBagUI != null) activeBagUI.SetActive(false);
        if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);
    }

    void Update()
    {
        if (!isDialogueActive) return;
        if (Input.GetMouseButtonDown(1))
        {
            if (index > 0 && !isWaitingForOrder) PreviousSentence();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (isWaitingForChoice) return;

            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
                isTyping = false;
            }
            else
            {
                NextSentence();
            }
        }
    }

    public void BeginConversation()
    {
        startTalkButton.SetActive(false);
        choicePanel.SetActive(false);
        hintText.SetActive(true);
        isDialogueActive = true;

        if (isWaitingForOrder)
        {
            index = sentences.Length - 1;
            isWaitingForChoice = false;
            if (disagreeButton != null) disagreeButton.SetActive(false);

            StartCoroutine(TypeSentence(sentences[index]));
        }
        else
        {
            index = 0;
            isWaitingForChoice = false;
            if (disagreeButton != null) disagreeButton.SetActive(true);
            if (agreeButton != null) agreeButton.SetActive(true);

            StartCoroutine(TypeSentence(sentences[index]));
        }
    }

    public void StartDialogue(string[] newSentences)
    {
        sentences = newSentences;
        index = 0;
        choicePanel.SetActive(false);
        isWaitingForChoice = false;

        StartCoroutine(TypeSentence(sentences[index]));
    }

    public void ShowStartTalkButton()
    {
        if (startTalkButton != null)
        {
            startTalkButton.SetActive(true);
            TextMeshProUGUI btnText = startTalkButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = "คุยกับลูกค้า";
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = sentence;
        dialogueText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        while (visibleCount <= totalVisibleCharacters)
        {
            dialogueText.maxVisibleCharacters = visibleCount;
            visibleCount++;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;
            StartCoroutine(TypeSentence(sentences[index]));
        }
        else
        {
            isWaitingForChoice = true;
            choicePanel.SetActive(true);
            startTalkButton.SetActive(false);
        }
    }

    void PreviousSentence()
    {
        StopAllCoroutines();
        isTyping = false;
        isWaitingForChoice = false;
        choicePanel.SetActive(false);
        index--;
        StartCoroutine(TypeSentence(sentences[index]));
    }

    public void ChooseAgree()
    {
        choicePanel.SetActive(false);
        hintText.SetActive(false);
        isWaitingForChoice = false;
        isDialogueActive = false;
        dialogueText.text = "";

        startTalkButton.SetActive(true);
        TextMeshProUGUI btnText = startTalkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.text = "คุยกับลูกค้า";

        isWaitingForOrder = true;
    }

    public void ChooseDisagree()
    {
        EndCustomerInteraction();
        startTalkButton.SetActive(false);
        customerWalker.WalkAway();
    }

    void EndCustomerInteraction()
    {
        StopAllCoroutines();
        choicePanel.SetActive(false);
        hintText.SetActive(false);
        isWaitingForChoice = false;
        isDialogueActive = false;
        dialogueText.text = "";

        if (giveItemButton != null) giveItemButton.SetActive(false);
        if (activeBagUI != null) activeBagUI.SetActive(false);
        if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);

        hasBag = false;
        isBagOnTray = false;
        itemsInBag.Clear();
        isWaitingForOrder = false;
    }

    public void PickUpBag()
    {
        if (!isWaitingForOrder) return;

        hasBag = true;
        if (activeBagUI != null) activeBagUI.SetActive(true);
    }

    public void PickUpItem(string itemName)
    {
        if (!isWaitingForOrder || !hasBag) return;
        if (itemsInBag.Count >= maxBagCapacity)
        {
            Debug.Log("ถุงเต็ม!");
            return;
        }

        itemsInBag.Add(itemName);
    }

    public void PlaceBagOnTray()
    {
        if (!isWaitingForOrder || !hasBag || itemsInBag.Count == 0 || isBagOnTray) return;

        isBagOnTray = true;
        Debug.Log("วางถุงที่ถาดแล้ว");

        if (activeBagUI != null) activeBagUI.SetActive(false);
        if (bagOnTrayUI != null) bagOnTrayUI.SetActive(true);

        if (startTalkButton != null) startTalkButton.SetActive(false);
        StopAllCoroutines();
        isTyping = false;
        dialogueText.text = "";
        isDialogueActive = false;
        isWaitingForChoice = false;

        if (choicePanel != null) choicePanel.SetActive(false);
        if (hintText != null) hintText.SetActive(false);
        if (giveItemButton != null) giveItemButton.SetActive(true);
    }

    public void GiveItemToCustomer()
    {
        if (!isBagOnTray) return;

        bool isCorrect = itemsInBag.Contains(expectedItem);
        bool hasExtraWrongItems = itemsInBag.Count > 1 || (!isCorrect && itemsInBag.Count == 1);

        if (isCorrect && !hasExtraWrongItems) Debug.Log("ถูกเป๊ะ");
        else if (isCorrect && hasExtraWrongItems) Debug.Log("ถูกแต่มั่วปนมา");
        else Debug.Log("ผิดทั้งหมด");

        EndCustomerInteraction();
        customerWalker.WalkAway();
    }
}