using UnityEngine;

public class BagInteract : MonoBehaviour
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
        if (dialogueManager != null && spriteRenderer != null)
        {
            if (dialogueManager.isWaitingForOrder && !dialogueManager.hasBag)
            {
                spriteRenderer.color = hoverColor;
            }
        }
    }

    void OnMouseDown()
    {
        if (dialogueManager != null)
        {
            if (dialogueManager.isWaitingForOrder && !dialogueManager.hasBag)
            {
                dialogueManager.PickUpBag();
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
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
}