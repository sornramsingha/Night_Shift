using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Light2D))]
public class ShopLightController : MonoBehaviour
{
    public static List<ShopLightController> AllLights = new List<ShopLightController>();
    public static ShopLightController Instance;

    public static bool globalIsBroken = false;
    private static bool isBreakingDown = false;

    [HideInInspector]
    public bool isBroken
    {
        get { return globalIsBroken; }
        set { globalIsBroken = value; }
    }

    private Light2D myLight;
    private Coroutine flickerCoroutine;
    private Coroutine panicCoroutine;

    [Header("การตั้งค่าแสงปกติ")]
    public float baseIntensity = 1.0f;
    public float minNormalFlicker = 0.1f;

    [Header("การตั้งค่าตอนผิดปกติ")]
    public bool isAggressive = false;

    [Header("ระบบความมืดกลืนกิน (Vignette Creep)")]
    public RectTransform darkVignetteRect;
    public float maxDarknessTime = 15f;
    public float startVignetteScale = 1.2f;
    public float minVignetteScale = 0.35f;
    private float brokenElapsedTime = 0f;
    private bool isClearingDarkness = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        myLight = GetComponent<Light2D>();
        myLight.intensity = baseIntensity;
    }

    void OnEnable()
    {
        if (!AllLights.Contains(this)) AllLights.Add(this);

        if (globalIsBroken)
        {
            if (myLight != null) myLight.intensity = 0f;
            return;
        }

        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    void OnDisable()
    {
        if (AllLights.Contains(this)) AllLights.Remove(this);
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        if (panicCoroutine != null) StopCoroutine(panicCoroutine);
    }

    void Update()
    {
        if (Instance != this) return;

        if (globalIsBroken && !isClearingDarkness && darkVignetteRect != null)
        {
            brokenElapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(brokenElapsedTime / maxDarknessTime);

            float currentScale = Mathf.Lerp(startVignetteScale, minVignetteScale, progress);
            darkVignetteRect.localScale = new Vector3(currentScale, currentScale, 1f);
        }
    }

    public void TriggerTemporaryPanic(float duration)
    {
        foreach (ShopLightController light in AllLights)
        {
            if (light == null || !light.gameObject.activeInHierarchy) continue;
            if (globalIsBroken || isBreakingDown) continue;

            if (light.panicCoroutine != null) light.StopCoroutine(light.panicCoroutine);
            light.panicCoroutine = light.StartCoroutine(light.PanicRoutine(duration));
        }
    }

    private IEnumerator PanicRoutine(float duration)
    {
        isAggressive = true;

        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        flickerCoroutine = StartCoroutine(FlickerRoutine());

        yield return new WaitForSeconds(duration);

        isAggressive = false;

        if (!globalIsBroken && !isBreakingDown && myLight != null)
            myLight.intensity = baseIntensity;

        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        if (gameObject.activeInHierarchy && !globalIsBroken)
            flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    public void SetAggressiveFlicker(bool state)
    {
        foreach (ShopLightController light in AllLights)
        {
            if (light == null) continue;
            if (globalIsBroken || isBreakingDown) continue;

            if (light.panicCoroutine != null)
            {
                light.StopCoroutine(light.panicCoroutine);
                light.panicCoroutine = null;
            }

            if (light.isAggressive == state) continue;

            light.isAggressive = state;

            if (light.flickerCoroutine != null) light.StopCoroutine(light.flickerCoroutine);

            if (!state && !globalIsBroken && light.myLight != null)
                light.myLight.intensity = light.baseIntensity;

            if (light.gameObject.activeInHierarchy)
                light.flickerCoroutine = light.StartCoroutine(light.FlickerRoutine());
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (isAggressive)
            {
                if (myLight != null) myLight.intensity = Random.Range(0f, 0.1f);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));

                if (myLight != null) myLight.intensity = baseIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
            else
            {
                if (myLight != null) myLight.intensity = baseIntensity;
                float waitTime = Random.Range(3.0f, 8.0f);
                yield return new WaitForSeconds(waitTime);

                if (myLight != null) myLight.intensity = minNormalFlicker;
                yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));

                if (myLight != null) myLight.intensity = baseIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

                if (Random.value > 0.4f)
                {
                    if (myLight != null) myLight.intensity = minNormalFlicker;
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                }

                if (myLight != null) myLight.intensity = baseIntensity;
            }
        }
    }

    public void TriggerLightBreakdown()
    {
        if (globalIsBroken || isBreakingDown) return;

        if (Instance != null && Instance.gameObject.activeInHierarchy)
        {
            Instance.StartCoroutine(Instance.BreakdownWarningRoutine());
        }
        else
        {
            StartCoroutine(BreakdownWarningRoutine());
        }
    }

    private IEnumerator BreakdownWarningRoutine()
    {
        isBreakingDown = true;
        isClearingDarkness = false;

        foreach (ShopLightController light in AllLights)
        {
            if (light.flickerCoroutine != null) light.StopCoroutine(light.flickerCoroutine);
            if (light.panicCoroutine != null) light.StopCoroutine(light.panicCoroutine);
            light.isAggressive = false;
        }

        float[] offDurations = { 0.10f, 0.08f, 0.14f };
        float[] onDurations = { 0.12f, 0.09f, 0.07f };

        for (int i = 0; i < 3; i++)
        {
            SetAllLightsIntensity(0f);
            yield return new WaitForSeconds(offDurations[i]);

            SetAllLightsIntensity(baseIntensity * 0.75f);
            yield return new WaitForSeconds(onDurations[i]);
        }

        isBreakingDown = false;
        globalIsBroken = true;
        brokenElapsedTime = 0f;

        SetAllLightsIntensity(0f);

        if (darkVignetteRect != null)
        {
            darkVignetteRect.localScale = new Vector3(startVignetteScale, startVignetteScale, 1f);
            SetVignetteAlpha(1f);
            darkVignetteRect.gameObject.SetActive(true);
        }

        if (WarningManager.Instance != null)
        {
            WarningManager.Show("ไฟดับ! ต้องซ่อมหลอดไฟก่อน!");
        }
    }

    public void StartClearingDarkness(float duration)
    {
        if (Instance != null && Instance.gameObject.activeInHierarchy)
        {
            Instance.StartCoroutine(Instance.ClearDarknessRoutine(duration));
        }
    }

    public void FlashDarknessAlpha(float alpha)
    {
        if (isClearingDarkness || !globalIsBroken) return;
        SetVignetteAlpha(alpha);
    }

    private IEnumerator ClearDarknessRoutine(float duration)
    {
        isClearingDarkness = true;
        if (darkVignetteRect == null) yield break;

        Vector3 currentScale = darkVignetteRect.localScale;
        Vector3 targetScale = new Vector3(2.4f, 2.4f, 1f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            darkVignetteRect.localScale = Vector3.Lerp(currentScale, targetScale, t);
            SetVignetteAlpha(1f - t);
            yield return null;
        }

        darkVignetteRect.gameObject.SetActive(false);
    }

    private void SetVignetteAlpha(float alpha)
    {
        if (darkVignetteRect == null) return;
        Image img = darkVignetteRect.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    private void SetAllLightsIntensity(float intensity)
    {
        foreach (ShopLightController light in AllLights)
        {
            if (light != null && light.myLight != null)
            {
                light.myLight.intensity = intensity;
            }
        }
    }

    public void EndLightBreakdown()
    {
        globalIsBroken = false;
        isBreakingDown = false;
        isClearingDarkness = false;
        brokenElapsedTime = 0f;

        if (darkVignetteRect != null)
        {
            darkVignetteRect.localScale = new Vector3(startVignetteScale, startVignetteScale, 1f);
            SetVignetteAlpha(1f);
            darkVignetteRect.gameObject.SetActive(false);
        }

        foreach (ShopLightController light in AllLights)
        {
            if (light.myLight != null) light.myLight.intensity = light.baseIntensity;
            if (light.gameObject.activeInHierarchy)
            {
                light.flickerCoroutine = light.StartCoroutine(light.FlickerRoutine());
            }
        }
    }
}