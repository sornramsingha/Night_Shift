using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BagInteract : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("ตั้งค่ารูปตอนเมาส์ชี้")]
    public Sprite outlineSprite;
    private Sprite originalSprite;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null && outlineSprite != null)
        {
            spriteRenderer.sprite = outlineSprite;
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }
    }

    void OnMouseDown()
    {
        if (dialogueManager != null)
        {
            dialogueManager.PickUpBag();

            if (spriteRenderer != null && originalSprite != null)
            {
                spriteRenderer.sprite = originalSprite;
            }
        }
    }
}