using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightRepairMinigame : MonoBehaviour
{
    public static LightRepairMinigame Instance;

    [Header("UI Elements")]
    public GameObject minigameUI;
    public RectTransform targetZone;
    public RectTransform cursor;

    [Header("Settings")]
    public float cursorSpeed = 350f;
    public float maxCursorDistance = 100f;
    public float initialTargetWidth = 200f;
    public float widthDecreasePerSuccess = 50f;

    [HideInInspector] public bool isPlaying = false;

    private float currentCursorSpeed;
    private int currentSuccesses = 0;
    private int moveDirection = 1;

    void Awake()
    {
        Instance = this;
        if (minigameUI != null) minigameUI.SetActive(false);
    }

    public void StartMinigame()
    {
        if (DialogueManager.Instance != null) DialogueManager.Instance.ToggleMinigameUI(true);
        if (isPlaying) return;

        isPlaying = true;
        currentSuccesses = 0;
        currentCursorSpeed = cursorSpeed;

        targetZone.sizeDelta = new Vector2(initialTargetWidth, targetZone.sizeDelta.y);

        cursor.anchoredPosition = Vector2.zero;
        moveDirection = Random.value > 0.5f ? 1 : -1;

        minigameUI.SetActive(true);

        if (CameraSwitcher.Instance != null)
        {
            CameraSwitcher.Instance.SetFocusLight(true);
        }
    }

    void Update()
    {
        if (!isPlaying) return;
        cursor.anchoredPosition += Vector2.right * moveDirection * currentCursorSpeed * Time.deltaTime;

        if (cursor.anchoredPosition.x >= maxCursorDistance)
        {
            cursor.anchoredPosition = new Vector2(maxCursorDistance, cursor.anchoredPosition.y);
            moveDirection = -1;
        }
        else if (cursor.anchoredPosition.x <= -maxCursorDistance)
        {
            cursor.anchoredPosition = new Vector2(-maxCursorDistance, cursor.anchoredPosition.y);
            moveDirection = 1;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }
    }

    private void CheckHit()
    {
        float targetWidth = targetZone.sizeDelta.x;
        float currentPos = cursor.anchoredPosition.x;
        float targetPos = targetZone.anchoredPosition.x;

        if (Mathf.Abs(currentPos - targetPos) <= targetWidth / 2f)
        {
            currentSuccesses++;
            Debug.Log("ซ่อมสำเร็จครั้งที่: " + currentSuccesses);

            if (currentSuccesses >= 3)
            {
                FinishMinigame();
            }
            else
            {
                targetZone.sizeDelta = new Vector2(targetZone.sizeDelta.x - widthDecreasePerSuccess, targetZone.sizeDelta.y);
                currentCursorSpeed += 50f;
            }
        }
        else
        {
            Debug.Log("พลาด! ลองใหม่");
        }
    }

    private void FinishMinigame()
    {
        if (DialogueManager.Instance != null) DialogueManager.Instance.ToggleMinigameUI(false);
        isPlaying = false;
        minigameUI.SetActive(false);

        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.SetAggressiveFlicker(false);
            ShopLightController.Instance.EndLightBreakdown();
        }

        if (CameraSwitcher.Instance != null)
        {
            CameraSwitcher.Instance.SetFocusLight(false);
        }
    }
}