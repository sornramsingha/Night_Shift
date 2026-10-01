using UnityEngine;

public class BagInteract : MonoBehaviour
{
    public DialogueManager dialogueManager;

    void OnMouseDown()
    {
        if (dialogueManager != null)
        {
            dialogueManager.PickUpBag();
        }
    }
}