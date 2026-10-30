using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Light2D))]
public class ShopLightController : MonoBehaviour
{
    public static List<ShopLightController> AllLights = new List<ShopLightController>();
    public static ShopLightController Instance;
    [HideInInspector] public bool isBroken = false;

    private Light2D myLight;
    private Coroutine flickerCoroutine;
    private Coroutine panicCoroutine;

    [Header("การตั้งค่าแสงปกติ")]
    public float baseIntensity = 1.0f;
    public float minNormalFlicker = 0.1f;

    [Header("การตั้งค่าตอนผิดปกติ")]
    public bool isAggressive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        myLight = GetComponent<Light2D>();
        myLight.intensity = baseIntensity;
    }

    void OnEnable()
    {
        if (!AllLights.Contains(this)) AllLights.Add(this);
        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    void OnDisable()
    {
        if (AllLights.Contains(this)) AllLights.Remove(this);
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        if (panicCoroutine != null) StopCoroutine(panicCoroutine);
    }

    public void TriggerTemporaryPanic(float duration)
    {
        foreach (ShopLightController light in AllLights)
        {
            if (light == null || !light.gameObject.activeInHierarchy) continue;
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
        myLight.intensity = baseIntensity;

        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    public void SetAggressiveFlicker(bool state)
    {
        foreach (ShopLightController light in AllLights)
        {
            if (light == null) continue;
            if (light.panicCoroutine != null)
            {
                light.StopCoroutine(light.panicCoroutine);
                light.panicCoroutine = null;
            }

            if (light.isAggressive == state) continue;

            light.isAggressive = state;

            if (light.flickerCoroutine != null) light.StopCoroutine(light.flickerCoroutine);
            if (!state) light.myLight.intensity = light.baseIntensity;

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
                myLight.intensity = Random.Range(0f, 0.1f);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));

                myLight.intensity = baseIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
            else
            {
                myLight.intensity = baseIntensity;
                float waitTime = Random.Range(3.0f, 8.0f);
                yield return new WaitForSeconds(waitTime);

                myLight.intensity = minNormalFlicker;
                yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));

                myLight.intensity = baseIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

                if (Random.value > 0.4f)
                {
                    myLight.intensity = minNormalFlicker;
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                }

                myLight.intensity = baseIntensity;
            }
        }
    }

    public void TriggerLightBreakdown()
    {
        isBroken = true;
        foreach (ShopLightController light in AllLights)
        {
            if (light.flickerCoroutine != null) light.StopCoroutine(light.flickerCoroutine);
            light.myLight.intensity = 0f;
        }
    }

    public void EndLightBreakdown()
    {
        isBroken = false;
        foreach (ShopLightController light in AllLights)
        {
            light.myLight.intensity = light.baseIntensity;
            light.flickerCoroutine = light.StartCoroutine(light.FlickerRoutine());
        }
    }
}