using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [Header("ตั้งค่าเวลาออกของลูกค้า")]
    public float minCustomerDelay = 3.0f;
    public float maxCustomerDelay = 5.0f;

    [Header("ระบบอื่นๆ")]
    public CustomerWalker customer;

    private int customerServedToday = 0;
    private CustomerAnomalyVisual customerVisual;
    private CustomerDialogue currentActiveData;

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

        DaySetting today = days[currentDayIndex];
        today.randomizedQueue = new List<CustomerDialogue>();
        List<CustomerDialogue> pool = new List<CustomerDialogue>(today.possibleCustomersPool);

        int countToSpawn = Mathf.Min(today.customersPerDay, pool.Count);
        for (int i = 0; i < countToSpawn; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            today.randomizedQueue.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
        if (today.hasGuideNPC)
        {
            today.randomizedQueue.Insert(0, today.guideNPC);
        }
        SpawnNextCustomer();
    }

    public void SpawnNextCustomer()
    {
        StartCoroutine(WaitAndSpawnRoutine());
    }

    private IEnumerator WaitAndSpawnRoutine()
    {
        if (customerServedToday > 0)
        {
            float delay = Random.Range(minCustomerDelay, maxCustomerDelay);
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        if (currentDayIndex >= days.Length) yield break;

        DaySetting today = days[currentDayIndex];
        if (today.randomizedQueue == null || customerServedToday >= today.randomizedQueue.Count)
        {
            Debug.Log("จบวันที่ " + (currentDayIndex + 1));
            currentDayIndex++;
            ShowDayTransition();
            yield break;
        }

        CustomerDialogue currentCustomerData = today.randomizedQueue[customerServedToday];
        customerServedToday++;

        ApplyCustomerData(currentCustomerData);
    }


    private void ApplyCustomerData(CustomerDialogue data)
    {
        if (data == null) return;
        currentActiveData = data;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetupNewCustomer(data);
        }

        if (customer != null)
        {
            customer.SetupCustomer(data.customerSprite, data.hasAnomaly, data.permanentAnomalySprite);
            customer.ResetAndWalk();
        }
    }

    public void ApplyAnomalyVisuals()
    {
        if (customer != null && customerVisual != null && currentActiveData != null)
        {
            bool isTrick = currentActiveData.isTrickOrder && currentActiveData.forceAnomalyAfterTrick;
            customerVisual.SetupAnomaly(
                currentActiveData.customerSprite,
                currentActiveData.glitchSprite,
                currentActiveData.permanentAnomalySprite,
                currentActiveData.hasAnomaly,
                currentActiveData.canGlitch,
                isTrick
            );
        }
    }
}