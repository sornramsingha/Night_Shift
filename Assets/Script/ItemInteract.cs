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

    [Header("ตั้งค่าอนิเมชั่น")]
    public float flyDuration = 0.4f;
    public float jumpHeight = 1.5f;

    [Header("ตั้งค่าชื่อสินค้า")]
    public string itemsName;
    public DialogueManager dialogueManager;

    private SpriteRenderer mainSprite;
    private SpriteRenderer spriteRenderer;
    private GameObject[] outlineObjects = new GameObject[4];

    void Start()
    {
        mainSprite = GetComponent<SpriteRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
                StartCoroutine(FlyToBagRoutine());
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
        clone.transform.localScale = transform.localScale;

        SpriteRenderer cloneSprite = clone.AddComponent<SpriteRenderer>();
        cloneSprite.sprite = spriteRenderer.sprite;
        cloneSprite.sortingOrder = 100;

        Vector3 startPos = transform.position;
        Vector3 endPos = dialogueManager.bagDropTarget != null ? dialogueManager.bagDropTarget.position : startPos;

        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime / flyDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, time);
            currentPos.y += Mathf.Sin(time * Mathf.PI) * jumpHeight;
            clone.transform.position = currentPos;
            clone.transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, time);

            yield return null;
        }
        Destroy(clone);
        dialogueManager.PickUpItem(itemName);
    }
}