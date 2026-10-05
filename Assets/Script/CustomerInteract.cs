using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CustomerInteract : MonoBehaviour
{
    public DialogueManager dialogueManager;

    void OnMouseDown()
    {
        if (dialogueManager != null && dialogueManager.isHoldingTalisman)
        {
            dialogueManager.UseTalismanOnCustomer();
        }
    }
}