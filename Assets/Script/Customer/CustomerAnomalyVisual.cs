using UnityEngine;
using System.Collections;

public class CustomerAnomalyVisual : MonoBehaviour
{
    public static CustomerAnomalyVisual CurrentActive;

    [Header("เป้าหมายการเปลี่ยนรูป")]
    public SpriteRenderer targetSprite;
    public Animator targetAnimator;

    [Header("สถานะความผิดปกติ")]
    public bool hasAnomaly = false;
    public bool isCurrentlyShowingAnomaly = false;
    private bool isWaitingForTrickReveal = false;

    [HideInInspector] public Sprite normalSprite;
    [HideInInspector] public Sprite glitchSprite;
    [HideInInspector] public Sprite permanentAnomalySprite;

    [Header("ตั้งค่าการกระพริบ & การสลับร่าง")]
    public bool canGlitch = true;
    public bool revertToNormalOnTurn = false;
    public float minFlickerWait = 4.0f;
    public float maxFlickerWait = 8.0f;
    public float flickerDuration = 0.15f;

    private float defaultMinWait;
    private float defaultMaxWait;
    private float defaultDuration;

    private Coroutine glitchCoroutine;

    void Awake()
    {
        if (targetSprite == null) targetSprite = GetComponentInChildren<SpriteRenderer>();
        if (targetAnimator == null) targetAnimator = GetComponentInChildren<Animator>();

        defaultMinWait = minFlickerWait;
        defaultMaxWait = maxFlickerWait;
        defaultDuration = flickerDuration;
    }

    void OnEnable() { CurrentActive = this; }

    void OnDisable()
    {
        StopGlitchCoroutine();
        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.SetAggressiveFlicker(false);
        }
    }

    public void SetupAnomaly(Sprite normal, Sprite glitch, Sprite permanent, bool isGhost, bool enableGlitch, bool isTrickOrderWait)
    {
        StopGlitchCoroutine();

        minFlickerWait = defaultMinWait;
        maxFlickerWait = defaultMaxWait;
        flickerDuration = defaultDuration;

        normalSprite = normal;
        glitchSprite = glitch;
        permanentAnomalySprite = permanent;
        hasAnomaly = isGhost;
        canGlitch = enableGlitch;
        isWaitingForTrickReveal = isTrickOrderWait;

        isCurrentlyShowingAnomaly = false;
        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.SetAggressiveFlicker(false);
        }

        if (targetSprite != null && normalSprite != null) targetSprite.sprite = normalSprite;
        if (targetAnimator != null) targetAnimator.enabled = true;

        if (hasAnomaly && canGlitch)
        {
            glitchCoroutine = StartCoroutine(AnomalyFlickerRoutine());
        }
    }

    private IEnumerator AnomalyFlickerRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minFlickerWait, maxFlickerWait);
            yield return new WaitForSeconds(waitTime);

            bool isOrderReady = DialogueManager.Instance != null && DialogueManager.Instance.isWaitingForOrder;

            if (isOrderReady && hasAnomaly && !isCurrentlyShowingAnomaly && glitchSprite != null)
            {
                if (targetAnimator != null) targetAnimator.enabled = false;
                if (targetSprite != null) targetSprite.sprite = glitchSprite;

                yield return new WaitForSeconds(flickerDuration);

                if (!isCurrentlyShowingAnomaly && targetSprite != null)
                {
                    targetSprite.sprite = normalSprite;
                    if (targetAnimator != null) targetAnimator.enabled = true;
                }
            }
        }
    }

    public void ToggleAnomalyOnTurn()
    {
        if (!hasAnomaly || targetSprite == null) return;
        if (DialogueManager.Instance == null || !DialogueManager.Instance.isWaitingForOrder) return;

        if (isWaitingForTrickReveal) return;

        if (!isCurrentlyShowingAnomaly)
        {
            if (permanentAnomalySprite != null)
            {
                isCurrentlyShowingAnomaly = true;
                if (targetAnimator != null) targetAnimator.enabled = false;
                targetSprite.sprite = permanentAnomalySprite;

                FlexibleAudioPlayer audio = GetComponent<FlexibleAudioPlayer>();
                if (audio != null) audio.PlaySound();
                if (ShopLightController.Instance != null)
                {
                    ShopLightController.Instance.SetAggressiveFlicker(true);
                }
            }
        }
        else if (revertToNormalOnTurn && normalSprite != null)
        {
            isCurrentlyShowingAnomaly = false;
            targetSprite.sprite = normalSprite;
            if (targetAnimator != null) targetAnimator.enabled = true;
            if (ShopLightController.Instance != null)
            {
                ShopLightController.Instance.SetAggressiveFlicker(false);
            }
        }
    }

    public void ForceRevealAnomaly()
    {
        if (!hasAnomaly || targetSprite == null || permanentAnomalySprite == null) return;

        isWaitingForTrickReveal = false;
        isCurrentlyShowingAnomaly = true;

        if (targetAnimator != null) targetAnimator.enabled = false;
        targetSprite.sprite = permanentAnomalySprite;

        FlexibleAudioPlayer audio = GetComponent<FlexibleAudioPlayer>();
        if (audio != null) audio.PlaySound();

        Debug.Log("หันมารอบสอง: โชว์ร่างถาวรเลย!");

        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.SetAggressiveFlicker(true);
        }
    }

    public void TriggerAggressiveGlitch()
    {
        if (!hasAnomaly || targetSprite == null) return;

        isWaitingForTrickReveal = false;

        minFlickerWait = 0.5f;
        maxFlickerWait = 1.5f;
        flickerDuration = 0.3f;

        Debug.Log("หันมารอบสอง: กระพริบรัวขึ้นมาก!");

        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.SetAggressiveFlicker(true);
        }
    }

    public void StopGlitchCoroutine()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }
    }
}