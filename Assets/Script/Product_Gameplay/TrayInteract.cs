using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TrayInteract : MonoBehaviour
{

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
        if (spriteRenderer != null && DialogueManager.Instance != null)
        {
            if (DialogueManager.Instance.hasBag &&
                DialogueManager.Instance.itemsInBag.Count > 0 &&
                !DialogueManager.Instance.isBagOnTray)
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
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.PlaceBagOnTray();
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
    }
}