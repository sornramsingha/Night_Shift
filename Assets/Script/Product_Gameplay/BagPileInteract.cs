using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class BagPileInteract : MonoBehaviour
{
    private int lastClickFrame = -1;

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Time.frameCount == lastClickFrame) return;
        lastClickFrame = Time.frameCount;

        if (DialogueManager.Instance == null) return;

        if (DialogueManager.Instance.isConfirmingBagReset) return;

        if (!DialogueManager.Instance.hasBag)
        {
            DialogueManager.Instance.PickUpBag();
        }
        else
        {
            DialogueManager.Instance.ShowBagResetConfirmation();
        }
    }
}