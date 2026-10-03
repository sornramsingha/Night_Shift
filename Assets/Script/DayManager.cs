using UnityEngine;
using TMPro;

[System.Serializable]
public class CustomerDialogue
{

    [Header("รูปลูกค้า")]
    public Sprite customerSprite;

    [TextArea(2, 4)]
    public string[] sentences;

    [Header("สินค้าที่ต้องการ")]
    public string requiredItem;
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

    [Header("ข้อความบอกวันที่")]
    public TextMeshProUGUI inGameDayText;

    [Header("Panel วันเริ่มงาน")]
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
            if (inGameDayText != null) inGameDayText.text = "จบ";
            return;
        }

        dayTransitionPanel.SetActive(true);
        dayText.text = "วันที่ " + (currentDayIndex + 1);
        if (inGameDayText != null)
        {
            inGameDayText.text = "วันที่ " + (currentDayIndex + 1);
        }
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
            CustomerDialogue currentCustomerData = null;

            if (customerServedToday < today.dialogues.Length)
            {
                currentCustomerData = today.dialogues[customerServedToday];

                dialogueManager.sentences = currentCustomerData.sentences;
                dialogueManager.expectedItem = currentCustomerData.requiredItem;
            }
            else
            {
                Debug.LogWarning("ลืมใส่บทพูด " + (customerServedToday + 1));
            }

            customerServedToday++;
            Debug.Log("เรียกลูกค้าคนที่: " + customerServedToday);

            if (customer != null)
            {
                if (currentCustomerData != null && currentCustomerData.customerSprite != null)
                {
                    customer.ChangeSprite(currentCustomerData.customerSprite);
                }

                customer.ResetAndWalk();
            }
        }
    }
}