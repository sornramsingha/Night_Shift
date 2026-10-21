using UnityEngine;
using TMPro;
using System.Collections;

public class WarningManager : MonoBehaviour
{
    public static WarningManager Instance;

    [Header("ตั้งค่า UI แจ้งเตือน")]
    public TextMeshProUGUI warningText;
    public float warningDuration = 2f;

    private Coroutine warningCoroutine;

    void Awake()
    {
        Instance = this;
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    public static void Show(string message)
    {
        if (Instance != null)
        {
            Instance.ShowMessage(message);
        }
    }

    public void ShowMessage(string message)
    {
        if (warningText == null) return;

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }
        if (string.IsNullOrEmpty(message))
        {
            warningText.gameObject.SetActive(false);
            return;
        }

        warningCoroutine = StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningDuration);
        warningText.gameObject.SetActive(false);
    }
}