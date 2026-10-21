using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FridgeDoorInteract : MonoBehaviour
{
    [Header("สถานะ")]
    public bool isOpen = false;

    [Header("รูปภาพประตู")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("ระบบอ้างอิง")]
    public DialogueManager dialogueManager;

    [Header("สินค้าในตู้แช่")]
    public Collider2D[] itemCollidersInFridge;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateDoorState(true);
    }

    void OnMouseDown()
    {
        if (dialogueManager != null && !dialogueManager.isWaitingForOrder)
        {
            dialogueManager.ShowWarning("ต้องคุยกับลูกค้าให้เรียบร้อยก่อน!");
            return;
        }

        isOpen = !isOpen;
        UpdateDoorState(false);

    }

    void UpdateDoorState(bool isInitialSetup)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
        }
        if (itemCollidersInFridge != null)
        {
            foreach (Collider2D col in itemCollidersInFridge)
            {
                if (col != null) col.enabled = isOpen;
            }
        }
        if (!isInitialSetup && dialogueManager != null)
        {
            dialogueManager.ShowWarning(isOpen ? "" : "");
        }
    }
}