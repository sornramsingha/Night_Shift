using UnityEngine;
using System.Collections;

public class CustomerAnomalyVisual : MonoBehaviour
{
    public static CustomerAnomalyVisual CurrentActive;

    [Header("เป้าหมายการเปลี่ยนรูป")]
    public SpriteRenderer targetSprite;

    [Header("สถานะความผิดปกติ")]
    public bool hasAnomaly = false;
    public bool isCurrentlyShowingAnomaly = false;

    [HideInInspector] public Sprite normalSprite;
    [HideInInspector] public Sprite glitchSprite;
    [HideInInspector] public Sprite permanentAnomalySprite;

    [Header("ตั้งค่าการกระพริบ & การสลับร่าง")]
    public bool canGlitch = true;
    public bool revertToNormalOnTurn = false;
    public float minFlickerWait = 4.0f;
    public float maxFlickerWait = 8.0f;
    public float flickerDuration = 0.15f;

    private Coroutine glitchCoroutine;

    void Awake()
    {
        if (targetSprite == null)
        {
            targetSprite = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void OnEnable()
    {
        CurrentActive = this;
    }

    void OnDisable()
    {
        StopGlitchCoroutine();
    }

    public void SetupAnomaly(Sprite normal, Sprite glitch, Sprite permanent, bool isGhost, bool enableGlitch)
    {
        StopGlitchCoroutine();

        normalSprite = normal;
        glitchSprite = glitch;
        permanentAnomalySprite = permanent;
        hasAnomaly = isGhost;
        canGlitch = enableGlitch;

        isCurrentlyShowingAnomaly = false;

        if (targetSprite != null && normalSprite != null)
        {
            targetSprite.sprite = normalSprite;
        }

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
                if (targetSprite != null) targetSprite.sprite = glitchSprite;

                yield return new WaitForSeconds(flickerDuration);

                if (!isCurrentlyShowingAnomaly && targetSprite != null)
                {
                    targetSprite.sprite = normalSprite;
                }
            }
        }
    }

    public void ToggleAnomalyOnTurn()
    {
        if (!hasAnomaly || targetSprite == null) return;

        if (DialogueManager.Instance == null || !DialogueManager.Instance.isWaitingForOrder)
        {
            return;
        }
        if (!isCurrentlyShowingAnomaly)
        {
            if (permanentAnomalySprite != null)
            {
                isCurrentlyShowingAnomaly = true;
                targetSprite.sprite = permanentAnomalySprite;
            }
        }
        else if (revertToNormalOnTurn && normalSprite != null)
        {
            isCurrentlyShowingAnomaly = false;
            targetSprite.sprite = normalSprite;
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