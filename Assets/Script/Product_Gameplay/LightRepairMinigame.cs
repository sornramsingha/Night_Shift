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

    [Header("ตั้งค่าแรงสั่นตอนเคาะ")]
    public float shakeDuration = 0.15f;
    public float shakePosForce = 1f;
    public float shakeRotForce = 100f;

    [HideInInspector] public bool isPlaying = false;
    private int currentKnocks = 0;
    private bool isTestingLight = false;

    private Vector3 originalBulbPos;
    private Vector3 originalBulbRot;
    private Coroutine activeShake;

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

    public void StartMinigame()
    {
        if (isPlaying) return;

        Debug.Log("<color=yellow>[มินิเกม] เริ่มซ่อมไฟ!</color>");
        isPlaying = true;
        currentKnocks = 0;
        isTestingLight = false;

        if (realBulbTransform != null)
        {
            originalBulbPos = realBulbTransform.localPosition;
            originalBulbRot = realBulbTransform.localEulerAngles;

            // หา Light2D ทั้งในตัวมันเองและลูกๆ
            realLight2D = realBulbTransform.GetComponentInChildren<Light2D>();
        }

        if (DialogueManager.Instance != null) DialogueManager.Instance.ToggleMinigameUI(true);
        if (CameraSwitcher.Instance != null) CameraSwitcher.Instance.SetFocusLight(true);
    }

    public void OnBulbClicked()
    {
        if (!isPlaying)
        {
            StartMinigame();
        }

        // ถ้ากำลังเทสไฟกระพริบอยู่ จะล็อกไม่ให้กด
        if (isTestingLight)
        {
            Debug.Log("<color=red>[มินิเกม] กำลังเทสไฟอยู่ เคาะไม่ได้!</color>");
            return;
        }

        currentKnocks++;
        Debug.Log($"<color=lime>[มินิเกม] เคาะครั้งที่ {currentKnocks}/{requiredKnocks}</color>");

        if (activeShake != null)
        {
            StopCoroutine(activeShake);
            if (realBulbTransform != null)
            {
                realBulbTransform.localPosition = originalBulbPos;
                realBulbTransform.localEulerAngles = originalBulbRot;
            }
        }

        activeShake = StartCoroutine(ShakeBulbRoutine());

        if (currentKnocks >= requiredKnocks)
        {
            Debug.Log("<color=cyan>[มินิเกม] เคาะครบแล้ว! เริ่มทดสอบไฟกระพริบ...</color>");
            StartCoroutine(TestLightRoutine());
        }
    }

    private IEnumerator ShakeBulbRoutine()
    {
        float elapsed = 0f;
        Color originalColor = Color.white;

        if (realBulbSprite != null)
        {
            originalColor = realBulbSprite.color;
            realBulbSprite.color = new Color(0.4f, 0.4f, 0.4f); // ดรอปสีให้รู้ว่าโดนเคาะชัดๆ
        }

        while (elapsed < shakeDuration)
        {
            float xPos = Random.Range(-shakePosForce, shakePosForce);
            float yPos = Random.Range(-shakePosForce, shakePosForce);
            float zRot = Random.Range(-shakeRotForce, shakeRotForce);

            if (realBulbTransform != null)
            {
                realBulbTransform.localPosition = originalBulbPos + new Vector3(xPos, yPos, 0);
                realBulbTransform.localEulerAngles = originalBulbRot + new Vector3(0, 0, zRot);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // คืนค่ากลับที่เดิม
        if (realBulbTransform != null)
        {
            realBulbTransform.localPosition = originalBulbPos;
            realBulbTransform.localEulerAngles = originalBulbRot;
        }
        if (realBulbSprite != null) realBulbSprite.color = originalColor;
    }

    private IEnumerator TestLightRoutine()
    {
        isTestingLight = true;

        float baseIntens = 1.0f;
        if (ShopLightController.Instance != null) baseIntens = ShopLightController.Instance.baseIntensity;

        // กระพริบเช็คผล
        if (realLight2D != null)
        {
            realLight2D.intensity = baseIntens * 0.8f;
            yield return new WaitForSeconds(0.15f);
            realLight2D.intensity = 0f;
            yield return new WaitForSeconds(0.15f);
            realLight2D.intensity = baseIntens * 0.5f;
            yield return new WaitForSeconds(0.1f);
            realLight2D.intensity = 0f;
        }
        else if (realBulbSprite != null)
        {
            // ถ้าไม่เจอ Light2D ให้กระพริบรูป Sprite สีเหลืองแก้ขัดไปก่อน
            realBulbSprite.color = Color.yellow;
            yield return new WaitForSeconds(0.15f);
            realBulbSprite.color = Color.black;
            yield return new WaitForSeconds(0.15f);
            realBulbSprite.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            realBulbSprite.color = Color.black;
        }

        yield return new WaitForSeconds(0.5f);

        float roll = Random.value;
        if (roll <= successChance)
        {
            Debug.Log("<color=white>[มินิเกม] ซ่อมสำเร็จ!</color>");
            FinishMinigame();
        }
        else
        {
            Debug.Log("<color=orange>[มินิเกม] ซ่อมพลาด! รีเซ็ตให้เคาะใหม่</color>");
            currentKnocks = 0;
            isTestingLight = false;

            if (realLight2D != null) realLight2D.intensity = 0f;
            if (realBulbSprite != null) realBulbSprite.color = Color.white;

            string dialogue = failDialogues[Random.Range(0, failDialogues.Length)];
            WarningManager.Show(dialogue);
        }
    }

    private void FinishMinigame()
    {
        isPlaying = false;

        if (ShopLightController.Instance != null) ShopLightController.Instance.EndLightBreakdown();
        if (DialogueManager.Instance != null) DialogueManager.Instance.ToggleMinigameUI(false);
        if (CameraSwitcher.Instance != null) CameraSwitcher.Instance.SetFocusLight(false);
    }
}