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

    [Header("ปุ่มเงินทอนแยกหมวด")]
    [Tooltip("ใส่ปุ่มช่องแบงก์")]
    public List<RectTransform> banknoteButtons = new List<RectTransform>();

    [Tooltip("ใส่ปุ่มช่องเหรียญ")]
    public List<RectTransform> coinButtons = new List<RectTransform>();

    [Tooltip("ปุ่มที่ห้ามหายเด็ดขาด")]
    public List<RectTransform> protectedFromHide = new List<RectTransform>();

    [Header("ตั้งค่าโอกาสและความโหด")]
    [Range(0f, 1f)] public float anomalyChance = 0.3f;
    [Tooltip("1 = สุ่มสลับ 1 หมวด (แบงก์หรือเหรียญ) | 2 = สลับทั้ง 2 หมวด (แบงก์ 1 คู่ + เหรียญ 1 คู่)")]
    public int swapPairsCount = 2;
    public int minHiddenButtons = 1;
    public int maxHiddenButtons = 2;

    [Header("ระบบปุ่มรีเซ็ตเครื่อง & ลิ้นชักเปิด-ปิด")]
    public float rebootDuration = 4.0f;
    public GameObject rebootOverlay;
    public Image rebootProgressBar;
    public TextMeshProUGUI rebootText;

    [Space(5)]
    [Tooltip("ลิ้นชักตอนเปิด")]
    public GameObject openDrawerObject;
    [Tooltip("ลิ้นชักตอนปิด")]
    public GameObject closedDrawerObject;

    [Tooltip("ลูกเล่นเลื่อนลิ้นชักเก็บเข้า-เด้งออก")]
    public bool useDrawerSlideEffect = true;
    public float drawerSlideDistance = 45f;
    public float drawerSlideDuration = 0.14f;

    [Header("ระบบเสียง")]
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

    private enum DrawerAnimState { None, Closing, Opening }
    private DrawerAnimState drawerAnimState = DrawerAnimState.None;
    private float drawerAnimTimer = 0f;
    private RectTransform openDrawerRect;
    private Vector2 originalOpenDrawerPos;
    private bool hasCachedDrawerPos = false;

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        CacheOriginalButtonLayout();
        CacheDrawerRect();

        if (rebootOverlay != null) rebootOverlay.SetActive(false);
        SetDrawerStateImmediate(false);
    }

    private void CacheDrawerRect()
    {
        if (openDrawerObject != null)
        {
            openDrawerRect = openDrawerObject.GetComponent<RectTransform>();
            if (openDrawerRect != null)
            {
                originalOpenDrawerPos = openDrawerRect.anchoredPosition;
                hasCachedDrawerPos = true;
            }
        }
    }

    private List<RectTransform> GetAllMoneyButtons()
    {
        List<RectTransform> all = new List<RectTransform>();
        foreach (var btn in banknoteButtons)
        {
            if (btn != null && !all.Contains(btn)) all.Add(btn);
        }
        foreach (var btn in coinButtons)
        {
            if (btn != null && !all.Contains(btn)) all.Add(btn);
        }
        return all;
    }

    private void CacheOriginalButtonLayout()
    {
        originalPositions.Clear();
        originalSiblingIndices.Clear();

        foreach (var btn in GetAllMoneyButtons())
        {
            originalPositions[btn] = btn.anchoredPosition;
            originalSiblingIndices[btn] = btn.GetSiblingIndex();
        }
    }

    void OnDisable()
    {
        if (drawerAnimState != DrawerAnimState.None)
        {
            drawerAnimState = DrawerAnimState.None;
            SetDrawerStateImmediate(isRebooting);
        }
    }

    void Update()
    {
        UpdateDrawerSlideAnimation();

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

    private void UpdateDrawerSlideAnimation()
    {
        if (drawerAnimState == DrawerAnimState.None || openDrawerRect == null || !hasCachedDrawerPos) return;

        drawerAnimTimer += Time.deltaTime;
        float t = Mathf.Clamp01(drawerAnimTimer / Mathf.Max(0.01f, drawerSlideDuration));
        Vector2 tuckedPos = originalOpenDrawerPos + Vector2.up * drawerSlideDistance;

        if (drawerAnimState == DrawerAnimState.Closing)
        {
            float easeIn = t * t;
            openDrawerRect.anchoredPosition = Vector2.Lerp(originalOpenDrawerPos, tuckedPos, easeIn);

            if (t >= 1f)
            {
                drawerAnimState = DrawerAnimState.None;
                openDrawerRect.anchoredPosition = originalOpenDrawerPos;
                if (openDrawerObject != null) openDrawerObject.SetActive(false);
                if (closedDrawerObject != null) closedDrawerObject.SetActive(true);
            }
        }
        else if (drawerAnimState == DrawerAnimState.Opening)
        {
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            openDrawerRect.anchoredPosition = Vector2.Lerp(tuckedPos, originalOpenDrawerPos, easeOut);

            if (t >= 1f)
            {
                drawerAnimState = DrawerAnimState.None;
                openDrawerRect.anchoredPosition = originalOpenDrawerPos;
            }
        }
    }

    private void SetDrawerStateImmediate(bool closed)
    {
        drawerAnimState = DrawerAnimState.None;
        if (openDrawerRect != null && hasCachedDrawerPos)
        {
            openDrawerRect.anchoredPosition = originalOpenDrawerPos;
        }

        if (openDrawerObject != null) openDrawerObject.SetActive(!closed);
        if (closedDrawerObject != null) closedDrawerObject.SetActive(closed);
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
            SetDrawerStateImmediate(true);
            return;
        }

        SetDrawerStateImmediate(false);

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
            ApplyCategoryButtonSwap();
        }

        if (currentType == RegisterAnomalyType.HideButtons || currentType == RegisterAnomalyType.Both)
        {
            ApplyButtonHide();
        }
    }

    private void ApplyCategoryButtonSwap()
    {
        if (swapPairsCount <= 0) return;

        List<List<RectTransform>> validCategories = new List<List<RectTransform>>();
        if (CountValidButtons(banknoteButtons) >= 2) validCategories.Add(banknoteButtons);
        if (CountValidButtons(coinButtons) >= 2) validCategories.Add(coinButtons);

        if (validCategories.Count == 0) return;

        ShuffleList(validCategories);

        if (swapPairsCount == 1)
        {
            SwapWithinCategory(validCategories[0], 1);
        }
        else
        {
            int remainingPairs = swapPairsCount;
            foreach (var category in validCategories)
            {
                if (remainingPairs <= 0) break;
                int swapped = SwapWithinCategory(category, 1);
                remainingPairs -= swapped;
            }
        }
    }

    private int CountValidButtons(List<RectTransform> list)
    {
        int count = 0;
        foreach (var btn in list)
        {
            if (btn != null) count++;
        }
        return count;
    }

    private int SwapWithinCategory(List<RectTransform> categoryButtons, int maxPairs)
    {
        List<RectTransform> valid = new List<RectTransform>();
        foreach (var btn in categoryButtons)
        {
            if (btn != null) valid.Add(btn);
        }

        if (valid.Count < 2) return 0;

        ShuffleList(valid);
        int pairs = Mathf.Min(maxPairs, valid.Count / 2);

        for (int i = 0; i < pairs; i++)
        {
            RectTransform btnA = valid[i * 2];
            RectTransform btnB = valid[i * 2 + 1];
            bool hasLayoutGroup = btnA.parent != null && btnA.parent == btnB.parent && btnA.parent.GetComponent<LayoutGroup>() != null;

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

        return pairs;
    }

    private void ApplyButtonHide()
    {
        List<RectTransform> candidates = new List<RectTransform>();
        foreach (var btn in GetAllMoneyButtons())
        {
            if (!protectedFromHide.Contains(btn))
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

        if (useDrawerSlideEffect && openDrawerRect != null && hasCachedDrawerPos && gameObject.activeInHierarchy)
        {
            drawerAnimState = DrawerAnimState.Closing;
            drawerAnimTimer = 0f;
        }
        else
        {
            SetDrawerStateImmediate(true);
        }

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

        if (useDrawerSlideEffect && openDrawerRect != null && hasCachedDrawerPos && gameObject.activeInHierarchy)
        {
            if (closedDrawerObject != null) closedDrawerObject.SetActive(false);
            if (openDrawerObject != null) openDrawerObject.SetActive(true);
            openDrawerRect.anchoredPosition = originalOpenDrawerPos + Vector2.up * drawerSlideDistance;
            drawerAnimState = DrawerAnimState.Opening;
            drawerAnimTimer = 0f;
        }
        else
        {
            SetDrawerStateImmediate(false);
        }

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
        SetDrawerStateImmediate(false);
        RestoreAllButtons();
    }

    private void RestoreAllButtons()
    {
        List<RectTransform> allButtons = GetAllMoneyButtons();
        allButtons.Sort((a, b) =>
        {
            int idxA = originalSiblingIndices.ContainsKey(a) ? originalSiblingIndices[a] : 0;
            int idxB = originalSiblingIndices.ContainsKey(b) ? originalSiblingIndices[b] : 0;
            return idxA.CompareTo(idxB);
        });

        foreach (var btn in allButtons)
        {
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