using UnityEngine;
using TMPro;

[System.Serializable]
public class CustomerDialogue
{
    [TextArea(2, 4)]
    public string[] sentences;
}

[System.Serializable]
public class DaySetting
{
    public string dayName = "Day";
    public int totalCustomers = 2;

    [Header("บทสนทนาของลูกค้าเรียงตามคิว")]
    public CustomerDialogue[] dialogues;
}

public class DayManager : MonoBehaviour
{
    [Header("ตั้งค่ารายวัน 1-7")]
    public DaySetting[] days;
    public int currentDayIndex = 0;

    [Header("UI เปลี่ยนวัน")]
    public GameObject dayTransitionPanel;
    public TextMeshProUGUI dayText;

    [Header("ระบบอื่นๆ")]
    public CustomerWalker customer;
    public DialogueManager dialogueManager;

    private int customerServedToday = 0;

    void Start()
    {
        ShowDayTransition();
    }

    public void ShowDayTransition()
    {
        if (currentDayIndex >= days.Length)
        {
            dayText.text = "คุณรอดชีวิตครบ 7 วัน";
            dayTransitionPanel.SetActive(true);
            return;
        }

        dayTransitionPanel.SetActive(true);
        dayText.text = "วันที่ " + (currentDayIndex + 1);
    }

    public void StartShift()
    {
        dayTransitionPanel.SetActive(false);
        customerServedToday = 0;
        SpawnNextCustomer();
    }

    public void SpawnNextCustomer()
    {
        if (customerServedToday >= days[currentDayIndex].totalCustomers)
        {
            Debug.Log("จบวันที่ " + (currentDayIndex + 1));
            currentDayIndex++;
            ShowDayTransition();
        }
        else
        {
            DaySetting today = days[currentDayIndex];
            if (customerServedToday < today.dialogues.Length)
            {
                string[] nextDialogue = today.dialogues[customerServedToday].sentences;
                dialogueManager.sentences = nextDialogue;
            }
            else
            {
                Debug.LogWarning("ลืมใส่บทพูดให้ลูกค้าคิวที่ " + (customerServedToday + 1) + " ในวันที่ " + (currentDayIndex + 1));
            }

            customerServedToday++;
            Debug.Log("เรียกลูกค้าคนที่: " + customerServedToday);

            if (customer != null)
            {
                customer.ResetAndWalk();
            }
        }
    }
}