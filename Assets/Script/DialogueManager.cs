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

    public DayManager dayManager;
    public CustomerWalker customerWalker;

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
        hintText.SetActive(true);
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
        if (startTalkButton != null) startTalkButton.SetActive(true);
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
        Debug.Log("ผู้เล่นเลือก: ตกลง / ขายของ");
        EndCustomerInteraction();
        customerWalker.WalkAway();
    }

    public void ChooseDisagree()
    {
        Debug.Log("ผู้เล่นเลือก: ไม่ตกลง / ปฏิเสธ");
        EndCustomerInteraction();
        customerWalker.WalkAway();
    }

    void EndCustomerInteraction()
    {
        choicePanel.SetActive(false);
        hintText.SetActive(false);
        isWaitingForChoice = false;
        isDialogueActive = false;
        dialogueText.text = "";
    }
}