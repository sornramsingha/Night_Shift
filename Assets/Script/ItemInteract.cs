using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ItemInteract : MonoBehaviour
{
    [Header("ข้อมูลสินค้า")]
    public string itemName;

    [Header("ตั้งค่าเส้นขอบ (Outline)")]
    public Color outlineColor = Color.yellow;
    public float outlineThickness = 0.05f;

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
        Debug.Log("กำลังหยิบ: " + itemName + " ลงถุง!");
    }
}