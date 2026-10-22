using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FridgeDoorInteract : MonoBehaviour
{
    public static FridgeDoorInteract Instance;

    [Header("สถานะ")]
    public bool isOpen = false;

    [Header("รูปภาพประตู")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("สินค้าในตู้แช่")]
    public Collider2D[] itemCollidersInFridge;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateDoorState(true);
    }

    void OnMouseDown()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isWaitingForOrder)
        {
            WarningManager.Show("ต้องคุยกับลูกค้าให้เรียบร้อยก่อน!");
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
    }

    public void ForceCloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            UpdateDoorState(true);
        }
    }
}