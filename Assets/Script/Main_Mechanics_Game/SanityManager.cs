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
    [Tooltip("ยันต์ที่แปะไว้บนกำแพง")]
    public GameObject wardObject;
    [Tooltip("Animator ของยันต์")]
    public Animator wardAnimator;

    [Header("แอนิเมชันยันต์")]
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

    [Header("ระบบจั๊มสแกร์และแอนิเมชันผี")]
    [Tooltip("แผ่น UI สีดำทึบเต็มจอ")]
    public GameObject jumpscareBlackoutOverlay;
    public RectTransform jumpscareRect;                    
    public Image jumpscareImage;                        
    public Animator jumpscareAnimator;            
    public Sprite wanderingGhostSprite;
    public RuntimeAnimatorController wanderingGhostAnim;   
    public RuntimeAnimatorController defaultAnomalyAnim;

    [Header("จังหวะไฟดับและลูกเล่นตอนผีโผล่")]
    public float delayBeforeFlicker = 2.5f;     
    public float flickerBeforeDarkTime = 1.5f;  
    public float darkPauseBeforeJumpscare = 2f;
    public float jumpscareStartY = -900f;       
    public float jumpscareTargetY = -70f;         
    public float jumpscarePopDuration = 0.22f;

    [Space(5)]
    public float jumpscareShakeDuration = 0.45f; 
    public float jumpscareShakeAmount = 12f;    
    public float jumpscarePullBackScale = 0.85f;
    public float jumpscarePullBackDuration = 0.18f; 
    public float jumpscareLungeScale = 2.5f;    
    public float jumpscareLungeDuration = 0.16f;
    public Vector2 jumpscareLungeTargetPos = new Vector2(0f, -350f);
    public float jumpscareHoldTime = 0.8f; 

    [HideInInspector] public bool isGameOverSequenceActive = false;

    [Header("ค่าเก่าสำหรับเชื่อมระบบเดิม (ไม่ต้องปรับ)")]
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
        if (jumpscareBlackoutOverlay != null) jumpscareBlackoutOverlay.SetActive(false);
        InitializeAllWards();

        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.ResetLightToNormal();
        }
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

    public void BreakWard(WardBreakReason reason, Sprite customAnomalySprite = null, RuntimeAnimatorController customAnomalyAnim = null)
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
            StartCoroutine(TriggerJumpscareGameOver(reason, customAnomalySprite, customAnomalyAnim));
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
            WarningManager.Show("ยันต์อาคมเต็มครบ 3 อันแล้ว!");
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

    private IEnumerator TriggerJumpscareGameOver(WardBreakReason reason, Sprite customAnomalySprite, RuntimeAnimatorController customAnomalyAnim)
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
            ShopLightController.Instance.ForceBlackoutForJumpscare();
        }

        if (jumpscareBlackoutOverlay != null)
        {
            jumpscareBlackoutOverlay.SetActive(true);
        }

        if (audioSource != null && lightOutSound != null)
        {
            audioSource.PlayOneShot(lightOutSound);
        }

        yield return new WaitForSeconds(darkPauseBeforeJumpscare);

        Sprite chosenGhost = (reason == WardBreakReason.SoldToAnomaly && customAnomalySprite != null)
            ? customAnomalySprite
            : wanderingGhostSprite;

        RuntimeAnimatorController chosenAnim = (reason == WardBreakReason.SoldToAnomaly)
            ? (customAnomalyAnim != null ? customAnomalyAnim : defaultAnomalyAnim)
            : wanderingGhostAnim;

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

            if (jumpscareAnimator != null)
            {
                if (chosenAnim != null)
                {
                    jumpscareAnimator.runtimeAnimatorController = chosenAnim;
                    jumpscareAnimator.enabled = true;
                }
                else
                {
                    jumpscareAnimator.runtimeAnimatorController = null;
                    jumpscareAnimator.enabled = false;
                }
            }

            jumpscareRect.gameObject.SetActive(true);
            jumpscareRect.localScale = Vector3.one;

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

            elapsed = 0f;
            while (elapsed < jumpscareShakeDuration)
            {
                elapsed += Time.deltaTime;
                Vector2 offset = new Vector2(
                    Random.Range(-jumpscareShakeAmount, jumpscareShakeAmount),
                    Random.Range(-jumpscareShakeAmount, jumpscareShakeAmount)
                );
                jumpscareRect.anchoredPosition = targetPos + offset;
                yield return null;
            }

            elapsed = 0f;
            Vector3 normalScale = Vector3.one;
            Vector3 pullBackScaleVec = Vector3.one * jumpscarePullBackScale;
            while (elapsed < jumpscarePullBackDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / jumpscarePullBackDuration);
                float easeOut = 1f - Mathf.Pow(1f - t, 2f);
                jumpscareRect.localScale = Vector3.Lerp(normalScale, pullBackScaleVec, easeOut);

                Vector2 offset = new Vector2(
                    Random.Range(-jumpscareShakeAmount * 0.5f, jumpscareShakeAmount * 0.5f),
                    Random.Range(-jumpscareShakeAmount * 0.5f, jumpscareShakeAmount * 0.5f)
                );
                jumpscareRect.anchoredPosition = targetPos + offset;
                yield return null;
            }

            elapsed = 0f;
            Vector3 lungeScaleVec = Vector3.one * jumpscareLungeScale;
            while (elapsed < jumpscareLungeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / jumpscareLungeDuration);
                float easeIn = t * t * t;
                jumpscareRect.localScale = Vector3.Lerp(pullBackScaleVec, lungeScaleVec, easeIn);

                Vector2 currentPos = Vector2.Lerp(targetPos, jumpscareLungeTargetPos, easeIn);
                Vector2 offset = new Vector2(
                    Random.Range(-jumpscareShakeAmount, jumpscareShakeAmount),
                    Random.Range(-jumpscareShakeAmount, jumpscareShakeAmount)
                );
                jumpscareRect.anchoredPosition = currentPos + offset;
                yield return null;
            }

            jumpscareRect.localScale = lungeScaleVec;
            jumpscareRect.anchoredPosition = jumpscareLungeTargetPos;

            yield return new WaitForSeconds(jumpscareHoldTime);
            GameOver();
        }
    }

    public void ApplyPenalty(float amount)
    {
        if (Mathf.Approximately(amount, penaltyAnomalyWrong))
        {
            Sprite activeGhost = null;

            if (CustomerAnomalyVisual.CurrentActive != null)
            {
                activeGhost = (CustomerAnomalyVisual.CurrentActive.permanentAnomalySprite != null)
                    ? CustomerAnomalyVisual.CurrentActive.permanentAnomalySprite
                    : CustomerAnomalyVisual.CurrentActive.glitchSprite;
            }

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
        Debug.Log("ยันต์อาคมหมด! จบเกมกลับไปเริ่มใหม่");

        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.ResetLightToNormal();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}