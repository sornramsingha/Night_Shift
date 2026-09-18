using UnityEngine;
using TMPro;

[System.Serializable]
public class DaySetting
{
    public string dayName = "Day 1";
    public int totalCustomers = 5;
}

public class DayManager : MonoBehaviour
{
    [Header("ตั้งค่ารายวัน (1-7)")]
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
        dayText.text = days[currentDayIndex].dayName;
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
            Debug.Log("จบวันที่ " + days[currentDayIndex].dayName);
            currentDayIndex++;
            ShowDayTransition();
        }
        else
        {
            customerServedToday++;
            Debug.Log("เรียกลูกค้าคนที่: " + customerServedToday);

            if (customer != null)
            {
                customer.ResetAndWalk();
            }
        }
    }
}