using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SanityManager : MonoBehaviour
{
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

    [Header("UI ขอบจอมืด")]
    public Image vignetteOverlay;

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
        if (vignetteOverlay != null)
        {
            float darknessFactor = 1f - (currentSanity / maxSanity);

            Color vignetteColor = vignetteOverlay.color;
            vignetteColor.a = darknessFactor;
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