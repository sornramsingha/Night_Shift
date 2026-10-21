using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

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
    private CustomerAnomalyVisual customerVisual;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (customer != null)
        {
            customerVisual = customer.GetComponent<CustomerAnomalyVisual>();
        }

        ShowDayTransition();
    }

    public void ShowDayTransition()
    {
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(true);

        if (currentDayIndex >= days.Length)
        {
            if (dayText != null) dayText.text = "คุณรอดชีวิตครบ 7 วัน";
            if (inGameDayText != null) inGameDayText.text = "จบ";
            return;
        }

        string currentDayString = "วันที่ " + (currentDayIndex + 1);
        if (dayText != null) dayText.text = currentDayString;
        if (inGameDayText != null) inGameDayText.text = currentDayString;
    }

    public void StartShift()
    {
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        customerServedToday = 0;
        SpawnNextCustomer();
    }

    public void SpawnNextCustomer()
    {
        if (currentDayIndex >= days.Length) return;

        DaySetting today = days[currentDayIndex];
        if (today.dialogues == null || customerServedToday >= today.dialogues.Length)
        {
            Debug.Log("จบวันที่ " + (currentDayIndex + 1));
            currentDayIndex++;
            ShowDayTransition();
            return;
        }

        CustomerDialogue currentCustomerData = today.dialogues[customerServedToday];
        customerServedToday++;

        ApplyCustomerData(currentCustomerData);
    }

    private void ApplyCustomerData(CustomerDialogue data)
    {
        if (data == null) return;
        if (dialogueManager != null)
        {
            dialogueManager.sentences = data.sentences;
            dialogueManager.expectedItem = data.requiredItem;
            dialogueManager.SetupAnomalyData(data.hasAnomaly, data.weaknessType);
        }
        if (customer != null)
        {
            customer.SetupCustomer(data.customerSprite, data.hasAnomaly, data.permanentAnomalySprite);

            if (customerVisual != null)
            {
                customerVisual.SetupAnomaly(
                    data.customerSprite,
                    data.glitchSprite,
                    data.permanentAnomalySprite,
                    data.hasAnomaly,
                    data.canGlitch
                );
            }

            customer.ResetAndWalk();
        }
    }
}