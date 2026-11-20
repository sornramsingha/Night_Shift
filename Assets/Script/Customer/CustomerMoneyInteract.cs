using UnityEngine;

public class CustomerMoneyInteract : MonoBehaviour
{
    void OnMouseDown()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isFacingFront) return;
        if (CheckoutManager.Instance == null || CheckoutManager.Instance.isCheckoutOpen) return;
        if (LightRepairMinigame.Instance != null && LightRepairMinigame.Instance.isPlaying) return;

        CheckoutManager.Instance.TakeCustomerMoney();
    }
}