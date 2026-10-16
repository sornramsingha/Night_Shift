using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomerAnomalyVisual : MonoBehaviour
{
    [Header("สถานะความผิดปกติ")]
    public bool hasAnomaly = false;

    [HideInInspector] public Sprite normalSprite;
    [HideInInspector] public Sprite anomalySprite;

    [Header("ตั้งค่าการกระพริบ")]
    public float minFlickerWait = 4.0f;
    public float maxFlickerWait = 8.0f;
    public float flickerDuration = 0.15f;

    private SpriteRenderer sr;
    private bool isFullyRevealed = false;
    private Coroutine glitchCoroutine;

    public void SetupAnomaly(Sprite normal, Sprite anomaly, bool isGhost)
    {
        sr = GetComponent<SpriteRenderer>();

        normalSprite = normal;
        anomalySprite = anomaly;
        hasAnomaly = isGhost;

        if (normalSprite != null) sr.sprite = normalSprite;
        if (hasAnomaly)
        {
            if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
            glitchCoroutine = StartCoroutine(AnomalyFlickerRoutine());
        }
    }

    IEnumerator AnomalyFlickerRoutine()
    {
        while (!isFullyRevealed)
        {
            float waitTime = Random.Range(minFlickerWait, maxFlickerWait);
            yield return new WaitForSeconds(waitTime);

            if (!isFullyRevealed && anomalySprite != null)
            {
                sr.sprite = anomalySprite;
                yield return new WaitForSeconds(flickerDuration);
                if (!isFullyRevealed) sr.sprite = normalSprite;
            }
        }
    }

    public void RevealAnomaly()
    {
        if (hasAnomaly && !isFullyRevealed)
        {
            isFullyRevealed = true;
            if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
            if (anomalySprite != null) sr.sprite = anomalySprite;
        }
    }
}