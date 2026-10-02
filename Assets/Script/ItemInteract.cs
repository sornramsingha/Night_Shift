using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class ItemInteract : MonoBehaviour
{
    [Header("ข้อมูลสินค้า")]
    public string itemName;

    [Header("ตั้งค่าสีตอนเมาส์ชี้")]
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    private Color originalColor = Color.white;

    [Header("ตั้งค่าอนิเมชั่น")]
    public float flyDuration = 0.5f;
    public float jumpHeight = 1.5f;
    public float cloneScaleMultiplier = 1.5f;

    [Header("ระบบจัดการ")]
    public DialogueManager dialogueManager;

    private SpriteRenderer mainSprite;

    void Start()
    {
        mainSprite = GetComponent<SpriteRenderer>();
        if (mainSprite != null)
        {
            originalColor = mainSprite.color;
        }
    }

    void OnMouseEnter()
    {
        if (mainSprite != null)
        {
            mainSprite.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (mainSprite != null)
        {
            mainSprite.color = originalColor;
        }
    }

    void OnMouseDown()
    {
        if (dialogueManager != null)
        {
            if (dialogueManager.hasBag)
            {
                if (dialogueManager.itemsInBag.Count < dialogueManager.maxBagCapacity)
                {
                    StartCoroutine(FlyToBagRoutine());
                    if (mainSprite != null) mainSprite.color = originalColor;
                }
                else
                {
                    Debug.Log("ถุงเต็มแล้ว!");
                }
            }
            else
            {
                Debug.Log("ต้องหยิบถุงก่อน ถึงจะใส่ของได้!");
            }
        }
    }

    IEnumerator FlyToBagRoutine()
    {
        GameObject clone = new GameObject("ItemClone_" + itemName);
        clone.transform.position = transform.position;

        Vector3 startScale = transform.localScale;
        Vector3 peakScale = startScale * cloneScaleMultiplier;

        SpriteRenderer cloneSprite = clone.AddComponent<SpriteRenderer>();
        cloneSprite.sprite = mainSprite.sprite;
        cloneSprite.sortingOrder = 100;

        Vector3 startPos = transform.position;
        Vector3 endPos = dialogueManager.bagDropTarget != null ? dialogueManager.bagDropTarget.position : startPos;
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

        dialogueManager.PickUpItem(itemName);
    }
}