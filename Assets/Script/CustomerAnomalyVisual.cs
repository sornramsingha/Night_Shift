using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomerAnomalyVisual : MonoBehaviour
{
    [Header("สถานะความผิดปกติ")]
    public bool hasAnomaly = false;
    public bool isCurrentlyShowingAnomaly = false;

    [HideInInspector] public Sprite normalSprite;
    [HideInInspector] public Sprite glitchSprite;
    [HideInInspector] public Sprite permanentAnomalySprite;

    [Header("ตั้งค่าการกระพริบ (Glitch)")]
    public bool canGlitch = true;
    public float minFlickerWait = 4.0f;
    public float maxFlickerWait = 8.0f;
    public float flickerDuration = 0.15f;

    private SpriteRenderer sr;
    private Coroutine glitchCoroutine;
    private DialogueManager dialogueManager;
    private Animator anim;

    void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        anim = GetComponent<Animator>();
    }

    public void SetupAnomaly(Sprite normal, Sprite glitch, Sprite permanent, bool isGhost, bool enableGlitch)
    {
        sr = GetComponent<SpriteRenderer>();
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        anim = GetComponent<Animator>();

        normalSprite = normal;
        glitchSprite = glitch;
        permanentAnomalySprite = permanent;
        hasAnomaly = isGhost;
        canGlitch = enableGlitch;

        isCurrentlyShowingAnomaly = false;

        if (normalSprite != null) sr.sprite = normalSprite;
        if (anim != null) anim.enabled = true;

        if (hasAnomaly && canGlitch)
        {
            if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
            glitchCoroutine = StartCoroutine(AnomalyFlickerRoutine());
        }
    }

    IEnumerator AnomalyFlickerRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minFlickerWait, maxFlickerWait);
            yield return new WaitForSeconds(waitTime);

            // จะกระพริบได้ก็ต่อเมื่อ "รอรับออเดอร์แล้วเท่านั้น (คุยจบแล้ว)"
            bool isOrderReady = dialogueManager != null && dialogueManager.isWaitingForOrder;

            if (isOrderReady && hasAnomaly && !isCurrentlyShowingAnomaly && glitchSprite != null)
            {
                if (anim != null) anim.enabled = false;
                sr.sprite = glitchSprite;

                yield return new WaitForSeconds(flickerDuration);

                if (!isCurrentlyShowingAnomaly)
                {
                    sr.sprite = normalSprite;
                    if (anim != null) anim.enabled = true;
                }
            }
        }
    }

    public void ToggleAnomalyOnTurn()
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;
        }

        if (!hasAnomaly) return;

        // --- [หัวใจสำคัญ] ถ้ายังคุยไม่จบ โค้ดจะหยุดทำงานทันที ผีจะไม่โผล่ตอนกำลังเดินหรือกำลังคุย ---
        if (dialogueManager != null && !dialogueManager.isWaitingForOrder)
        {
            Debug.Log("ยังคุยไม่จบ! ห้ามสลับร่าง");
            return;
        }

        isCurrentlyShowingAnomaly = !isCurrentlyShowingAnomaly;

        if (isCurrentlyShowingAnomaly)
        {
            if (permanentAnomalySprite != null)
            {
                if (anim != null) anim.enabled = false; // ปิด Animator
                sr.sprite = permanentAnomalySprite;
                sr.color = new Color(1f, 0.7f, 0.7f); // เทส: เปลี่ยนสีแดงให้เห็นชัดๆ
                Debug.Log("✅ โชว์ผีสำเร็จ");
            }
            else
            {
                Debug.Log("❌ บัค: ไม่พบรูปผีถาวร! (ตัวแปรว่าง)");
            }
        }
        else
        {
            if (normalSprite != null)
            {
                sr.sprite = normalSprite;
                sr.color = Color.white;
                if (anim != null) anim.enabled = true;
                Debug.Log("✅ กลับร่างคนสำเร็จ");
            }
        }
    }
}