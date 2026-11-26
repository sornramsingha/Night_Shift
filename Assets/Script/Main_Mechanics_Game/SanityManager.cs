using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum WardBreakReason
{
    SoldToAnomaly,  
    WrongTalismanOnNormal
}

[System.Serializable]
public class WardTalismanSlot
{
    [Tooltip("ยันต์อาคมแปะกำแพง")]
    public GameObject wardObject;
    [Tooltip("อนิเมเตอร์ของยันต์")]
    public Animator wardAnimator;

    [Header("อนิเมชั่นของยันต์")]
    public RuntimeAnimatorController idleController;  
    public RuntimeAnimatorController breakController; 
    public float breakAnimDuration = 1.0f;           
}

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance;

    [Header("ตั้งค่ายันต์อาคมบนกำแพง")]
    public WardTalismanSlot[] wallWards = new WardTalismanSlot[3];
    public int maxWards = 3;
    public int currentWards = 3;

    [Header("เสียงเอฟเฟกต์ยันต์ ไฟดับ และจั๊มสแกร์")]
    public AudioSource audioSource;
    public AudioClip wardBreakSound;       
    public AudioClip wardRestoreSound;     
    public AudioClip lightFlickerSound;    
    public AudioClip lightOutSound;     
    public AudioClip jumpscareSound; 
    [Range(0f, 1f)] public float jumpscareVolume = 1f;

    [Header("ระบบจั๊มสแกร์เมื่อยันต์หมด")]
    public RectTransform jumpscareRect;          
    public Image jumpscareImage;           
    public Sprite wanderingGhostSprite;
    public float delayBeforeFlicker = 2f;
    public float flickerBeforeDarkTime = 1.2f; 
    public float darkPauseBeforeJumpscare = 0.5f;
    public float jumpscarePopDuration = 0.25f;  
    public float jumpscareHoldTime = 1.5f;      
    public float jumpscareStartY = -900f;
    public float jumpscareTargetY = 0f;

    [HideInInspector] public bool isGameOverSequenceActive = false;

    [Header("สำหรับเชื่อมระบบเดิม(ไม่ต้องปรับ)")]
    public float penaltyNormalWrong = 10f;
    public float penaltyAnomalyWrong = 30f;
    public float penaltyTalismanWrong = 20f;
    public float rewardCorrectSell = 5f;
    public float rewardCorrectBanish = 15f;

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        maxWards = (wallWards != null && wallWards.Length > 0) ? wallWards.Length : 3;
        currentWards = maxWards;
        isGameOverSequenceActive = false;

        if (jumpscareRect != null) jumpscareRect.gameObject.SetActive(false);
        InitializeAllWards();
    }

    private void InitializeAllWards()
    {
        for (int i = 0; i < wallWards.Length; i++)
        {
            bool shouldBeActive = i < currentWards;
            SetWardIdleState(wallWards[i], shouldBeActive);
        }
    }

    private void SetWardIdleState(WardTalismanSlot slot, bool active)
    {
        if (slot == null || slot.wardObject == null) return;

        slot.wardObject.SetActive(active);

        if (active && slot.wardAnimator != null)
        {
            if (slot.idleController != null)
            {
                slot.wardAnimator.runtimeAnimatorController = slot.idleController;
                slot.wardAnimator.enabled = true;
            }
            else
            {
                slot.wardAnimator.enabled = false;
            }
        }
    }

    public void BreakWard(WardBreakReason reason, Sprite customAnomalySprite = null)
    {
        if (isGameOverSequenceActive || currentWards <= 0) return;

        currentWards--;
        int brokenIndex = currentWards;

        if (audioSource != null && wardBreakSound != null)
        {
            audioSource.PlayOneShot(wardBreakSound);
        }

        if (brokenIndex >= 0 && brokenIndex < wallWards.Length)
        {
            StartCoroutine(PlayWardBreakRoutine(wallWards[brokenIndex]));
        }

        if (currentWards <= 0)
        {
            currentWards = 0;
            isGameOverSequenceActive = true;
            StartCoroutine(TriggerJumpscareGameOver(reason, customAnomalySprite));
        }
    }

    private IEnumerator PlayWardBreakRoutine(WardTalismanSlot slot)
    {
        if (slot == null || slot.wardObject == null) yield break;
        if (slot.wardAnimator != null && slot.breakController != null)
        {
            slot.wardAnimator.runtimeAnimatorController = slot.breakController;
            slot.wardAnimator.enabled = true;
            yield return new WaitForSeconds(slot.breakAnimDuration);
        }

        slot.wardObject.SetActive(false);
    }

    public bool CanRestoreWard()
    {
        return currentWards < maxWards;
    }

    public bool RestoreWard(int amount = 1)
    {
        if (currentWards >= maxWards || amount <= 0) return false;

        int restored = 0;
        while (restored < amount && currentWards < maxWards)
        {
            int indexToRestore = currentWards;
            currentWards++;
            restored++;

            if (indexToRestore >= 0 && indexToRestore < wallWards.Length)
            {
                SetWardIdleState(wallWards[indexToRestore], true);
            }
        }

        if (audioSource != null && wardRestoreSound != null)
        {
            audioSource.PlayOneShot(wardRestoreSound);
        }
        return true;
    }

    public bool BuyWardFromShop(int price)
    {
        if (!CanRestoreWard())
        {
            WarningManager.Show("ยันต์อาคมครบ 3 อันแล้ว");
            return false;
        }

        if (MoneyManager.Instance != null && MoneyManager.Instance.SpendMoney(price))
        {
            RestoreWard(1);
            WarningManager.Show("แปะยันต์อาคมเพิ่มเรียบร้อยแล้ว");
            return true;
        }

        WarningManager.Show("เงินไม่พอซื้อยันต์อาคม!");
        return false;
    }

    private IEnumerator TriggerJumpscareGameOver(WardBreakReason reason, Sprite customAnomalySprite)
    {
        yield return new WaitForSeconds(delayBeforeFlicker);

        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.SetAggressiveFlicker(true);
        }

        if (audioSource != null && lightFlickerSound != null)
        {
            audioSource.clip = lightFlickerSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(flickerBeforeDarkTime);

        if (audioSource != null && audioSource.clip == lightFlickerSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.SetAggressiveFlicker(false);
            ShopLightController.Instance.TriggerLightBreakdown();
        }

        if (audioSource != null && lightOutSound != null)
        {
            audioSource.PlayOneShot(lightOutSound);
        }

        yield return new WaitForSeconds(darkPauseBeforeJumpscare);
        Sprite chosenGhost = (reason == WardBreakReason.SoldToAnomaly && customAnomalySprite != null)
            ? customAnomalySprite
            : wanderingGhostSprite;

        if (audioSource != null && jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound, jumpscareVolume);
        }

        if (jumpscareRect != null)
        {
            if (jumpscareImage != null && chosenGhost != null)
            {
                jumpscareImage.sprite = chosenGhost;
            }

            jumpscareRect.gameObject.SetActive(true);
            Vector2 startPos = new Vector2(0f, jumpscareStartY);
            Vector2 targetPos = new Vector2(0f, jumpscareTargetY);
            jumpscareRect.anchoredPosition = startPos;

            float elapsed = 0f;
            while (elapsed < jumpscarePopDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / jumpscarePopDuration);
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                jumpscareRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeOut);
                yield return null;
            }
            jumpscareRect.anchoredPosition = targetPos;
        }

        yield return new WaitForSeconds(jumpscareHoldTime);
        GameOver();
    }

    public void ApplyPenalty(float amount)
    {
        if (Mathf.Approximately(amount, penaltyAnomalyWrong))
        {
            Sprite activeGhost = (CustomerAnomalyVisual.CurrentActive != null)
                ? CustomerAnomalyVisual.CurrentActive.permanentAnomalySprite
                : null;
            BreakWard(WardBreakReason.SoldToAnomaly, activeGhost);
        }
        else if (Mathf.Approximately(amount, penaltyTalismanWrong))
        {
            BreakWard(WardBreakReason.WrongTalismanOnNormal);
        }
    }

    public void ApplyReward(float amount)
    {
    }

    void GameOver()
    {
        Debug.Log("จบเกมกลับไปเริ่มใหม่");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}