using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(Light2D))]
public class ShopLightController : MonoBehaviour
{
    public static ShopLightController Instance;

    private Light2D myLight;

    [Header("การตั้งค่าแสงปกติ")]
    public float baseIntensity = 1.0f;
    public float minNormalFlicker = 0.7f;

    [Header("การตั้งค่าตอนผิดปกติ")]
    public bool isAggressive = false;
    public float minGhostFlicker = 0.1f;
    public float maxGhostFlicker = 0.5f;

    void Awake()
    {
        Instance = this;
        myLight = GetComponent<Light2D>();
        myLight.intensity = baseIntensity;
    }

    void Start()
    {
        StartCoroutine(FlickerRoutine());
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

    public void SetAggressiveFlicker(bool state)
    {
        isAggressive = state;
    }
}