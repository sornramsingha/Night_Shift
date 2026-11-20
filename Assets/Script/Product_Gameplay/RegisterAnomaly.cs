using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum RegisterAnomalyType
{
    None,
    SwapButtons, 
    HideButtons, 
    Both         
}

public class RegisterAnomaly : MonoBehaviour
{
    public static RegisterAnomaly Instance;

    [Header("1.ปุ่มเงินทอนที่จะให้สุ่มรวน")]
    public List<RectTransform> moneyButtons = new List<RectTransform>();

    [Tooltip("ปุ่มที่ห้ามหายเด็ดขาดเพื่อกันผู้เล่นทอนเงินไม่ได้")]
    public List<RectTransform> protectedFromHide = new List<RectTransform>();

    [Header("2.ตั้งค่าโอกาสและความโหด")]
    [Range(0f, 1f)] public float anomalyChance = 0.35f;
    public int swapPairsCount = 2;       
    public int minHiddenButtons = 1;     
    public int maxHiddenButtons = 2;     

    [Header("3.ระบบปุ่มรีเซ็ตเครื่อง")]
    public float rebootDuration = 4.0f;  
    public GameObject rebootOverlay;     
    public Image rebootProgressBar;
    public TextMeshProUGUI rebootText;

    [Header("4.ระบบเสียง")]
    public AudioSource audioSource;
    public AudioClip sparkSound;         
    public AudioClip rebootStartSound;
    public AudioClip rebootDoneSound; 

    [HideInInspector] public bool hasActiveAnomaly = false;
    [HideInInspector] public bool isRebooting = false;

    private RegisterAnomalyType currentType = RegisterAnomalyType.None;
    private bool hasAppliedVisualForCurrentBill = false;
    private float rebootTimer = 0f;

    private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();
    private Dictionary<RectTransform, int> originalSiblingIndices = new Dictionary<RectTransform, int>();

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        CacheOriginalButtonLayout();
        if (rebootOverlay != null) rebootOverlay.SetActive(false);
    }

    private void CacheOriginalButtonLayout()
    {
        originalPositions.Clear();
        originalSiblingIndices.Clear();

        foreach (var btn in moneyButtons)
        {
            if (btn == null) continue;
            originalPositions[btn] = btn.anchoredPosition;
            originalSiblingIndices[btn] = btn.GetSiblingIndex();
        }
    }

    void Update()
    {
        if (!isRebooting) return;

        rebootTimer -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(rebootTimer / rebootDuration);

        if (rebootProgressBar != null) rebootProgressBar.fillAmount = progress;
        if (rebootText != null) rebootText.text = $"กำลังรีเซ็ตระบบ... {Mathf.CeilToInt(Mathf.Max(0f, rebootTimer))}s";

        if (rebootTimer <= 0f)
        {
            FinishReboot();
        }
    }

    public void RollAnomalyForCustomer()
    {
        ResetAnomalyState();
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken) return;

        if (Random.value <= anomalyChance)
        {
            hasActiveAnomaly = true;
            int roll = Random.Range(0, 3);
            if (roll == 0) currentType = RegisterAnomalyType.SwapButtons;
            else if (roll == 1) currentType = RegisterAnomalyType.HideButtons;
            else currentType = RegisterAnomalyType.Both;
        }
    }

    public void OnRegisterOpened()
    {
        if (isRebooting)
        {
            if (rebootOverlay != null) rebootOverlay.SetActive(true);
            return;
        }

        if (hasActiveAnomaly && !hasAppliedVisualForCurrentBill)
        {
            hasAppliedVisualForCurrentBill = true;
            ApplyAnomalyEffects();

            if (audioSource != null && sparkSound != null)
            {
                audioSource.PlayOneShot(sparkSound);
            }
        }
    }

    private void ApplyAnomalyEffects()
    {
        if (currentType == RegisterAnomalyType.SwapButtons || currentType == RegisterAnomalyType.Both)
        {
            ApplyButtonSwap();
        }

        if (currentType == RegisterAnomalyType.HideButtons || currentType == RegisterAnomalyType.Both)
        {
            ApplyButtonHide();
        }
    }

    private void ApplyButtonSwap()
    {
        if (moneyButtons.Count < 2) return;

        List<int> indices = new List<int>();
        for (int i = 0; i < moneyButtons.Count; i++)
        {
            if (moneyButtons[i] != null) indices.Add(i);
        }

        ShuffleList(indices);

        int pairs = Mathf.Min(swapPairsCount, indices.Count / 2);
        for (int i = 0; i < pairs; i++)
        {
            RectTransform btnA = moneyButtons[indices[i * 2]];
            RectTransform btnB = moneyButtons[indices[i * 2 + 1]];
            bool hasLayoutGroup = btnA.parent != null && btnA.parent.GetComponent<LayoutGroup>() != null;

            if (hasLayoutGroup)
            {
                int indexA = btnA.GetSiblingIndex();
                int indexB = btnB.GetSiblingIndex();
                btnA.SetSiblingIndex(indexB);
                btnB.SetSiblingIndex(indexA);
            }
            else
            {
                Vector2 posA = btnA.anchoredPosition;
                btnA.anchoredPosition = btnB.anchoredPosition;
                btnB.anchoredPosition = posA;
            }
        }
    }

    private void ApplyButtonHide()
    {
        List<RectTransform> candidates = new List<RectTransform>();
        foreach (var btn in moneyButtons)
        {
            if (btn != null && !protectedFromHide.Contains(btn))
            {
                candidates.Add(btn);
            }
        }

        if (candidates.Count == 0) return;

        ShuffleList(candidates);
        int hideCount = Mathf.Clamp(Random.Range(minHiddenButtons, maxHiddenButtons + 1), 1, candidates.Count);

        for (int i = 0; i < hideCount; i++)
        {
            candidates[i].gameObject.SetActive(false);
        }
    }

    public void StartReboot()
    {
        if (isRebooting || !hasActiveAnomaly) return;

        isRebooting = true;
        rebootTimer = rebootDuration;

        if (rebootOverlay != null) rebootOverlay.SetActive(true);
        if (rebootProgressBar != null) rebootProgressBar.fillAmount = 0f;

        if (audioSource != null && rebootStartSound != null)
        {
            audioSource.PlayOneShot(rebootStartSound);
        }
    }

    private void FinishReboot()
    {
        isRebooting = false;
        hasActiveAnomaly = false;
        currentType = RegisterAnomalyType.None;

        RestoreAllButtons();

        if (rebootOverlay != null) rebootOverlay.SetActive(false);

        if (audioSource != null && rebootDoneSound != null)
        {
            audioSource.PlayOneShot(rebootDoneSound);
        }
    }

    public void ResetAnomalyState()
    {
        isRebooting = false;
        hasActiveAnomaly = false;
        hasAppliedVisualForCurrentBill = false;
        currentType = RegisterAnomalyType.None;
        rebootTimer = 0f;

        if (rebootOverlay != null) rebootOverlay.SetActive(false);
        RestoreAllButtons();
    }

    private void RestoreAllButtons()
    {
        foreach (var btn in moneyButtons)
        {
            if (btn == null) continue;

            btn.gameObject.SetActive(true);

            if (originalPositions.ContainsKey(btn))
                btn.anchoredPosition = originalPositions[btn];

            if (originalSiblingIndices.ContainsKey(btn))
                btn.SetSiblingIndex(originalSiblingIndices[btn]);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}