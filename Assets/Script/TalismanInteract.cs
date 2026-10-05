using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TalismanInteract : MonoBehaviour
{
    public TalismanType myType;
    public DialogueManager dialogueManager;

    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    private Color originalColor = Color.white;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null && dialogueManager != null && !dialogueManager.isHoldingTalisman)
        {
            spriteRenderer.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    void OnMouseDown()
    {
        if (dialogueManager != null && !dialogueManager.isHoldingTalisman)
        {
            dialogueManager.PickUpTalisman(myType, this.gameObject, spriteRenderer.sprite);
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
        else if (dialogueManager != null && dialogueManager.isHoldingTalisman)
        {
            dialogueManager.ShowWarning("กำลังถือยันต์อยู่!");
        }
    }
}