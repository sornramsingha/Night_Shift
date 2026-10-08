using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class CustomerInteract : MonoBehaviour
{
    public DialogueManager dialogueManager;

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
        if (dialogueManager != null && dialogueManager.isHoldingTalisman)
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

            if (dialogueManager != null)
            {
                dialogueManager.ShowWarning("");
            }
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

            if (dialogueManager != null)
            {
                dialogueManager.ShowWarning("");
            }

            yield return null;
        }

        isHolding = false;
        ResetProgressUI();

        if (dialogueManager != null)
        {
            dialogueManager.UseTalismanOnCustomer();
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