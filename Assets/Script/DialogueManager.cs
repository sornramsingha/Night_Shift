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
    public bool hasBag = false;
    public List<string> itemsInBag = new List<string>();

    [Header("UI ถุงกลางจอ (ลากมาใส่ตรงนี้)")]
    public GameObject activeBagUI; // <--- รูปถุงที่จะโชว์กลางจอตอนหยิบแล้ว
    public Transform bagDropTarget; // <--- จุดที่ของจะลอยเข้าไป (ให้สร้าง Empty Object วางไว้ตรงกลางจอ)
    public GameObject giveItemButton;

    private bool isWaitingForOrder = false;

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
        if (activeBagUI != null) activeBagUI.SetActive(false); // ซ่อนถุงตอนเริ่มเกม
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
        choicePanel.SetActive(false);
        hintText.SetActive(false);
        isWaitingForChoice = false;
        isDialogueActive = false;
        dialogueText.text = "";

        if (giveItemButton != null) giveItemButton.SetActive(false);
        if (activeBagUI != null) activeBagUI.SetActive(false);

        hasBag = false;
        itemsInBag.Clear();
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

        itemsInBag.Add(itemName);

        if (itemsInBag.Count > 0 && giveItemButton != null)
        {
            giveItemButton.SetActive(true);
        }
    }

    public void GiveItemToCustomer()
    {
        bool isCorrect = itemsInBag.Contains(expectedItem);
        bool hasExtraWrongItems = itemsInBag.Count > 1 || (!isCorrect && itemsInBag.Count == 1);

        if (isCorrect && !hasExtraWrongItems) Debug.Log("ถูกเป๊ะ");
        else if (isCorrect && hasExtraWrongItems) Debug.Log("ถูกแต่มั่วปนมา");
        else Debug.Log("ผิดทั้งหมด");

        hasBag = false;
        itemsInBag.Clear();

        giveItemButton.SetActive(false);
        isWaitingForOrder = false;
        startTalkButton.SetActive(false);

        if (activeBagUI != null) activeBagUI.SetActive(false);

        customerWalker.WalkAway();
    }
}