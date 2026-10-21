using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class ItemInteract : MonoBehaviour
{
    [Header("ข้อมูลสินค้า")]
    public string itemName;

    [Header("ตั้งค่ารูปตอนเมาส์ชี้")]
    public Sprite outlineSprite;
    private Sprite originalSprite;
    private Vector3 originalScale;

    [Header("ตั้งค่าอนิเมชั่น")]
    public float flyDuration = 0.5f;
    public float jumpHeight = 1.5f;
    public float cloneScaleMultiplier = 1.5f;

    private SpriteRenderer mainSprite;

    void Start()
    {
        mainSprite = GetComponent<SpriteRenderer>();
        if (mainSprite != null)
        {
            originalSprite = mainSprite.sprite;
            originalScale = transform.localScale;
        }
    }

    void OnMouseEnter()
    {
        if (mainSprite != null && outlineSprite != null)
        {
            mainSprite.sprite = outlineSprite;
            transform.localScale = originalScale * 1.1f;
        }
    }

    void OnMouseExit()
    {
        if (mainSprite != null && originalSprite != null)
        {
            mainSprite.sprite = originalSprite;
            transform.localScale = originalScale;
        }
    }

    void OnMouseDown()
    {
        if (DialogueManager.Instance == null) return;
        if (!DialogueManager.Instance.hasBag)
        {
            WarningManager.Show("ต้องหยิบถุงก่อนสิ!!");
            return;
        }
        if (DialogueManager.Instance.itemsInBag.Count >= DialogueManager.Instance.maxBagCapacity)
        {
            WarningManager.Show("ถุงเต็มแล้ว!");
            return;
        }
        if (mainSprite != null && originalSprite != null)
        {
            mainSprite.sprite = originalSprite;
        }

        StartCoroutine(FlyToBagRoutine());
    }

    IEnumerator FlyToBagRoutine()
    {
        GameObject clone = new GameObject("ItemClone_" + itemName);
        clone.transform.position = transform.position;

        Vector3 startScale = transform.localScale;
        Vector3 peakScale = startScale * cloneScaleMultiplier;

        SpriteRenderer cloneSprite = clone.AddComponent<SpriteRenderer>();
        cloneSprite.sprite = originalSprite != null ? originalSprite : mainSprite.sprite;
        cloneSprite.sortingOrder = 100;

        Vector3 startPos = transform.position;
        Vector3 endPos = DialogueManager.Instance.bagDropTarget != null ? DialogueManager.Instance.bagDropTarget.position : startPos;
        Vector3 peakPos = startPos + (endPos - startPos) / 2f + Vector3.up * jumpHeight;

        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime / flyDuration;

            float easedTime = time * time * (3f - 2f * time);
            Vector3 m1 = Vector3.Lerp(startPos, peakPos, easedTime);
            Vector3 m2 = Vector3.Lerp(peakPos, endPos, easedTime);
            clone.transform.position = Vector3.Lerp(m1, m2, easedTime);

            if (easedTime < 0.5f)
            {
                float scaleTime = easedTime * 2f;
                clone.transform.localScale = Vector3.Lerp(startScale, peakScale, scaleTime);
            }
            else
            {
                float scaleTime = (easedTime - 0.5f) * 2f;
                clone.transform.localScale = Vector3.Lerp(peakScale, Vector3.zero, scaleTime);
            }

            yield return null;
        }

        Destroy(clone);
        DialogueManager.Instance.PickUpItem(itemName);
    }
}