using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TrayInteract : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("ตั้งค่าสีตอนเมาส์ชี้")]
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    private Color originalColor = Color.white;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null && dialogueManager != null)
        {
            if (dialogueManager.hasBag && dialogueManager.itemsInBag.Count > 0 && !dialogueManager.isBagOnTray)
            {
                spriteRenderer.color = hoverColor;
            }
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnMouseDown()
    {
        if (dialogueManager != null)
        {
            dialogueManager.PlaceBagOnTray();
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
    }
}