using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI และข้อความ")]
    public TextMeshProUGUI dialogueText;
    public GameObject choicePanel;
    public GameObject startTalkButton;
    public GameObject hintText;
    public GameObject hideheldText;
    public GameObject GiveItem;

    public DayManager dayManager;
    public CustomerWalker customerWalker;

    [Header("ระบบหยิบสินค้า")]
    public string expectedItem;
    public string heldItem = "";
    public TextMeshProUGUI handItemText;
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
        if (handItemText != null) handItemText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isDialogueActive) return;
        if (Input.GetMouseButtonDown(1))
        {
            if (index > 0)
            {
                PreviousSentence();
            }
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
        isWaitingForChoice = false;
        isDialogueActive = true;

        StartDialogue(sentences);
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
        Debug.Log("ตกลงขาย");

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
        Debug.Log("ไม่ขาย");
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
        if (handItemText != null) handItemText.gameObject.SetActive(false);
    }

    public void PickUpItem(string itemName)
    {
        heldItem = itemName;
        if (handItemText != null)
        {
            handItemText.gameObject.SetActive(true);
            handItemText.text = "กำลังถือ: " + heldItem;
        }
        Debug.Log("ผู้เล่นหยิบ: " + heldItem);
        if (isWaitingForOrder && giveItemButton != null)
        {
            giveItemButton.SetActive(true);
        }
    }

    public void GiveItemToCustomer()
    {
        if (heldItem == expectedItem)
        {
            Debug.Log("ขายสินค้าถูก");
            // TODO: **ใส่เอฟเฟกต์หรือเสียงตอนจบวันตรงนี้**
        }
        else
        {
            Debug.Log("หยิบของผิด!");
            // TODO: **ทำระบบ Game Over / Time Loop ตรงนี้ในอนาคต**
        }

        heldItem = "";
        if (handItemText != null) handItemText.text = "กำลังถือ: ไม่มี";
        giveItemButton.SetActive(false);
        isWaitingForOrder = false;
        startTalkButton.SetActive(false);
        if (handItemText != null) handItemText.gameObject.SetActive(false);

        customerWalker.WalkAway();
    }
}