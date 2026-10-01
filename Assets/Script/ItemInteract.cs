using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class ItemInteract : MonoBehaviour
{
    [Header("ข้อมูลสินค้า")]
    public string itemName;

    [Header("ตั้งค่าเส้นขอบ (Outline)")]
    public Color outlineColor = Color.yellow;
    public float outlineThickness = 0.05f;

    [Header("ตั้งค่าอนิเมชั่น (แบบ Figma)")]
    public float flyDuration = 0.5f;
    public float jumpHeight = 1.5f;
    public float cloneScaleMultiplier = 1.5f;

    [Header("ระบบจัดการ")]
    public DialogueManager dialogueManager;

    private SpriteRenderer mainSprite;
    private GameObject[] outlineObjects = new GameObject[4];

    void Start()
    {
        mainSprite = GetComponent<SpriteRenderer>();
        CreateOutlineSprites();
        ToggleOutline(false);
    }

    void CreateOutlineSprites()
    {
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(outlineThickness, 0, 0),
            new Vector3(-outlineThickness, 0, 0),
            new Vector3(0, outlineThickness, 0),
            new Vector3(0, -outlineThickness, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject outlineObj = new GameObject("OutlinePiece_" + i);
            outlineObj.transform.SetParent(transform);
            outlineObj.transform.localPosition = offsets[i];
            outlineObj.transform.localScale = Vector3.one;

            SpriteRenderer outlineSprite = outlineObj.AddComponent<SpriteRenderer>();
            outlineSprite.sprite = mainSprite.sprite;
            outlineSprite.color = outlineColor;
            outlineSprite.sortingLayerID = mainSprite.sortingLayerID;
            outlineSprite.sortingOrder = mainSprite.sortingOrder - 1;

            outlineObjects[i] = outlineObj;
        }
    }

    void ToggleOutline(bool isVisible)
    {
        foreach (GameObject obj in outlineObjects)
        {
            obj.SetActive(isVisible);
        }
    }

    void OnMouseEnter()
    {
        ToggleOutline(true);
    }

    void OnMouseExit()
    {
        ToggleOutline(false);
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