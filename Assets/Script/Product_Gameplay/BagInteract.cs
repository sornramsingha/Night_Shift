using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BagInteract : MonoBehaviour
{
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
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.PickUpBag();

            if (spriteRenderer != null && originalSprite != null)
            {
                spriteRenderer.sprite = originalSprite;
            }
        }
    }
}