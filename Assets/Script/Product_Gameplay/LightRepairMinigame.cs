using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class LightRepairMinigame : MonoBehaviour
{
    public static LightRepairMinigame Instance;

    [Header("อ้างอิงหลอดไฟจริงในฉาก")]
    public Transform realBulbTransform;
    public SpriteRenderer realBulbSprite;
    private Light2D realLight2D;

    [Header("ตั้งค่าเกมเพลย์")]
    public int requiredKnocks = 3;
    public float successChance = 0.7f;

    [Header("ตั้งค่าไฟสปาร์คตอนเคาะ (ครั้งที่ 1-2)")]
    [Range(0.1f, 1.5f)] public float sparkIntensity = 0.85f;
    public float sparkDuration = 0.2f;

    [Header("ตั้งค่าการหน่วงเวลา (Cooldown)")]
    public float knockCooldown = 0.6f;
    private float nextKnockTime = 0f;

    [Header("ตั้งค่าแรงแกว่งตอนเคาะ (Swing)")]
    public float swingDuration = 0.5f;
    public float maxSwingAngle = 45f;
    public float swingSpeed = 25f;

    [Header("ระบบเสียง")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioClip knockClip;
    public AudioClip heartbeatClip;

    [HideInInspector] public bool isPlaying = false;
    private int currentKnocks = 0;
    private bool isTestingLight = false;

    private Vector3 originalBulbPos;
    private Vector3 originalBulbRot;
    private Coroutine activeSwing;
    private Coroutine activeSpark;

    private string[] failDialogues = {
        "ไฟไม่ติด... เป็นบ้าอะไรเนี่ย!",
        "เฮงซวยเอ๊ย ติดสิวะ!",
        "ทำไมซวยแบบนี้เนี่ย... เอาใหม่ๆ",
        "สายไฟขาดหรือไงวะ ติดสิโว้ย!"
    };

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isPlaying && !isTestingLight)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (Time.unscaledTime >= nextKnockTime)
                {
                    RegisterKnock();
                    nextKnockTime = Time.unscaledTime + knockCooldown;
                }
            }
        }
    }

    public void StartMinigame()
    {
        if (isPlaying) return;

        if (DialogueManager.Instance != null && !DialogueManager.Instance.isFacingFront) return;
        if (CameraSwitcher.Instance != null && CameraSwitcher.Instance.isTransitioning) return;

        isPlaying = true;
        currentKnocks = 0;
        nextKnockTime = 0f;

        if (realBulbTransform != null)
        {
            originalBulbPos = realBulbTransform.localPosition;
            originalBulbRot = realBulbTransform.localEulerAngles;
            realLight2D = realBulbTransform.GetComponentInChildren<Light2D>();
        }

        if (bgmSource != null && heartbeatClip != null)
        {
            bgmSource.clip = heartbeatClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        StartCoroutine(PreventInitialSpam());

        try
        {
            if (DialogueManager.Instance != null) DialogueManager.Instance.ToggleMinigameUI(true);
            if (CameraSwitcher.Instance != null) CameraSwitcher.Instance.SetFocusLight(true);
        }
        catch { }
    }

    private IEnumerator PreventInitialSpam()
    {
        isTestingLight = true;
        yield return new WaitForSecondsRealtime(0.2f);
        isTestingLight = false;
    }

    private void RegisterKnock()
    {
        currentKnocks++;

        if (sfxSource != null && knockClip != null)
        {
            sfxSource.PlayOneShot(knockClip);
        }

        if (activeSwing != null)
        {
            StopCoroutine(activeSwing);
            ResetBulbPosition();
        }
        activeSwing = StartCoroutine(SwingBulbRoutine());

        if (currentKnocks >= requiredKnocks)
        {
            if (activeSpark != null) StopCoroutine(activeSpark);
            StartCoroutine(TestLightRoutine());
        }
        else
        {
            if (activeSpark != null) StopCoroutine(activeSpark);
            activeSpark = StartCoroutine(KnockSparkRoutine());
        }
    }

    private IEnumerator KnockSparkRoutine()
    {
        float baseIntens = (ShopLightController.Instance != null) ? ShopLightController.Instance.baseIntensity : 1.0f;

        if (realLight2D != null)
        {
            realLight2D.intensity = baseIntens * sparkIntensity;
            if (ShopLightController.Instance != null) ShopLightController.Instance.FlashDarknessAlpha(0.1f);
            yield return new WaitForSecondsRealtime(sparkDuration);

            realLight2D.intensity = 0f;
            if (ShopLightController.Instance != null) ShopLightController.Instance.FlashDarknessAlpha(1f);
            yield return new WaitForSecondsRealtime(0.05f);

            realLight2D.intensity = baseIntens * (sparkIntensity * 0.6f);
            if (ShopLightController.Instance != null) ShopLightController.Instance.FlashDarknessAlpha(0.35f);
            yield return new WaitForSecondsRealtime(sparkDuration * 0.6f);

            realLight2D.intensity = 0f;
            if (ShopLightController.Instance != null) ShopLightController.Instance.FlashDarknessAlpha(1f);
        }
    }

    private void ResetBulbPosition()
    {
        if (realBulbTransform != null)
        {
            realBulbTransform.localPosition = originalBulbPos;
            realBulbTransform.localEulerAngles = originalBulbRot;
        }
    }

    private IEnumerator SwingBulbRoutine()
    {
        float elapsed = 0f;
        Color originalColor = Color.white;

        if (realBulbSprite != null)
        {
            originalColor = realBulbSprite.color;
            realBulbSprite.color = new Color(0.7f, 0.7f, 0.7f);
        }

        while (elapsed < swingDuration)
        {
            float damping = 1f - (elapsed / swingDuration);
            float zRot = Mathf.Sin(elapsed * swingSpeed) * maxSwingAngle * damping;

            if (realBulbTransform != null)
            {
                realBulbTransform.localEulerAngles = originalBulbRot + new Vector3(0, 0, zRot);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ResetBulbPosition();
        if (realBulbSprite != null) realBulbSprite.color = originalColor;
    }

    private IEnumerator TestLightRoutine()
    {
        isTestingLight = true;
        float baseIntens = 1.0f;
        if (ShopLightController.Instance != null)
        {
            baseIntens = ShopLightController.Instance.baseIntensity;
            ShopLightController.Instance.FlashDarknessAlpha(0.9f);
        }

        if (bgmSource != null) bgmSource.Stop();
        if (realLight2D != null)
        {
            realLight2D.intensity = baseIntens * 0.5f;
            if (ShopLightController.Instance != null) ShopLightController.Instance.FlashDarknessAlpha(1f);
            yield return new WaitForSecondsRealtime(0.12f);

            realLight2D.intensity = 0.5f;
            if (ShopLightController.Instance != null) ShopLightController.Instance.FlashDarknessAlpha(1f);
            yield return new WaitForSecondsRealtime(0.15f);
        }

        yield return new WaitForSecondsRealtime(0.4f);

        float roll = Random.value;
        if (roll <= successChance)
        {
            yield return StartCoroutine(RepairSuccessRoutine(baseIntens));
            FinishMinigame();
        }
        else
        {
            currentKnocks = 0;
            isTestingLight = false;

            if (realLight2D != null) realLight2D.intensity = 0f;
            if (ShopLightController.Instance != null) ShopLightController.Instance.FlashDarknessAlpha(1f);
            if (bgmSource != null && heartbeatClip != null) bgmSource.Play();

            string dialogue = failDialogues[Random.Range(0, failDialogues.Length)];
            WarningManager.Show(dialogue);
        }
    }

    private IEnumerator RepairSuccessRoutine(float baseIntens)
    {
        if (ShopLightController.Instance != null)
        {
            ShopLightController.Instance.StartClearingDarkness(0.65f);
        }

        if (realLight2D != null)
        {
            realLight2D.intensity = baseIntens * 0.7f;
            yield return new WaitForSecondsRealtime(0.08f);
            realLight2D.intensity = 0f;
            yield return new WaitForSecondsRealtime(0.08f);

            realLight2D.intensity = baseIntens * 0.9f;
            yield return new WaitForSecondsRealtime(0.08f);
            realLight2D.intensity = 0f;
            yield return new WaitForSecondsRealtime(0.10f);

            realLight2D.intensity = baseIntens;
        }

        yield return new WaitForSecondsRealtime(0.65f);
    }

    private void FinishMinigame()
    {
        isPlaying = false;
        isTestingLight = false;

        if (bgmSource != null) bgmSource.Stop();
        if (ShopLightController.Instance != null) ShopLightController.Instance.EndLightBreakdown();

        try
        {
            if (DialogueManager.Instance != null) DialogueManager.Instance.ToggleMinigameUI(false);
            if (CameraSwitcher.Instance != null) CameraSwitcher.Instance.SetFocusLight(false);
        }
        catch { }
    }
}