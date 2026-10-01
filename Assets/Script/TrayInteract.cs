using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TrayInteract : MonoBehaviour
{
    public DialogueManager dialogueManager;

    void OnMouseDown()
    {
        if (dialogueManager != null)
        {
            dialogueManager.PlaceBagOnTray();
        }
    }
}