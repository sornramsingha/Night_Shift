using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TalismanInteract : MonoBehaviour
{
    public TalismanType myType;

    [Header("ตั้งค่ารูปตอนเมาส์ชี้")]
    public Sprite outlineSprite;
    private Sprite originalSprite;
    private Vector3 originalScale;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
            originalScale = transform.localScale;
        }
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null && outlineSprite != null)
        {
            spriteRenderer.sprite = outlineSprite;
            transform.localScale = originalScale * 1.05f;
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
            transform.localScale = originalScale;
        }
    }

    void OnMouseDown()
    {
        if (DialogueManager.Instance != null)
        {
            Sprite spriteToPass = originalSprite != null ? originalSprite : spriteRenderer.sprite;

            DialogueManager.Instance.PickUpTalisman(myType, this.gameObject, spriteToPass);
            if (spriteRenderer != null && originalSprite != null)
            {
                spriteRenderer.sprite = originalSprite;
                transform.localScale = originalScale;
            }
        }
    }
}