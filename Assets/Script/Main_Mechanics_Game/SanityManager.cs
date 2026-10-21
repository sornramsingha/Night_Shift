using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance;

    [Header("ตั้งค่าค่าสติ")]
    public float maxSanity = 100f;
    public float currentSanity;

    [Header("ค่าสติที่ลดลง")]
    public float penaltyNormalWrong = 10f;
    public float penaltyAnomalyWrong = 30f;
    public float penaltyTalismanWrong = 20f;

    [Header("ค่าสติที่เพิ่มขึ้น")]
    public float rewardCorrectSell = 5f;
    public float rewardCorrectBanish = 15f;

    [Header("UI ขอบจอมืด (Vignette)")]
    public Image vignetteOverlay;
    public Sprite[] vignetteStages;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentSanity = maxSanity;
        UpdateVignetteUI();
    }

    public void ApplyPenalty(float amount)
    {
        currentSanity -= amount;
        if (currentSanity <= 0)
        {
            currentSanity = 0;
            UpdateVignetteUI();
            GameOver();
        }
        else
        {
            UpdateVignetteUI();
        }
    }

    public void ApplyReward(float amount)
    {
        currentSanity += amount;
        if (currentSanity > maxSanity)
        {
            currentSanity = maxSanity;
        }
        UpdateVignetteUI();
    }

    void UpdateVignetteUI()
    {
        if (vignetteOverlay != null && vignetteStages != null && vignetteStages.Length > 0)
        {
            float damagePercent = 1f - (currentSanity / maxSanity);
            int spriteIndex = Mathf.Clamp(Mathf.FloorToInt(damagePercent * vignetteStages.Length), 0, vignetteStages.Length - 1);

            vignetteOverlay.sprite = vignetteStages[spriteIndex];
            Color vignetteColor = vignetteOverlay.color;
            vignetteColor.a = 1f;
            vignetteOverlay.color = vignetteColor;
        }
    }

    void GameOver()
    {
        Debug.Log("ค่าสติหมด! สติแตกกลับไปวันแรก");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}