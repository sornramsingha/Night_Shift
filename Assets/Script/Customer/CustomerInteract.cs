using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class CustomerInteract : MonoBehaviour
{
    [Header("ตั้งค่าเวลากด")]
    public float holdDuration = 1.0f;

    [Header("หลอดเวลาการใช้ยันต์")]
    public Image progressCircleImage;

    private Coroutine holdCoroutine;
    private bool isHolding = false;

    void Start()
    {
        ResetProgressUI();
    }

    void OnMouseDown()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isFacingFront) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.isHoldingTalisman)
        {
            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            holdCoroutine = StartCoroutine(HoldToUseTalismanRoutine());
        }
    }

    void OnMouseUp()
    {
        if (isHolding)
        {
            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            isHolding = false;
            ResetProgressUI();
        }
    }

    IEnumerator HoldToUseTalismanRoutine()
    {
        isHolding = true;
        float timer = 0f;

        if (progressCircleImage != null)
        {
            progressCircleImage.gameObject.SetActive(true);
            progressCircleImage.fillAmount = 0f;
        }

        while (timer < holdDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / holdDuration;

            if (progressCircleImage != null)
            {
                progressCircleImage.fillAmount = progress;
            }

            yield return null;
        }

        isHolding = false;
        ResetProgressUI();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.UseTalismanOnCustomer();
        }
    }

    void ResetProgressUI()
    {
        if (progressCircleImage != null)
        {
            progressCircleImage.fillAmount = 0f;
            progressCircleImage.gameObject.SetActive(false);
        }
    }
}