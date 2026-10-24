using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class FridgeDoorInteract : MonoBehaviour
{
    public static FridgeDoorInteract Instance;

    [Header("สถานะ")]
    public bool isOpen = false;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("รูปภาพประตู")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("สินค้าในตู้แช่")]
    public Collider2D[] itemCollidersInFridge;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        UpdateDoorState(true);

    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isWaitingForOrder)
        {
            WarningManager.Show("ต้องคุยกับลูกค้าให้เรียบร้อยก่อน!");
            return;
        }

        isOpen = !isOpen;
        if (audioSource != null)
        {
            if (isOpen && openSound != null) audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null) audioSource.PlayOneShot(closeSound);
        }

        UpdateDoorState(false);
    }

    void UpdateDoorState(bool isInitialSetup)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
        }

        if (itemCollidersInFridge != null)
        {
            foreach (Collider2D col in itemCollidersInFridge)
            {
                if (col != null) col.enabled = isOpen;
            }
        }
    }

    public void ForceCloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            if (audioSource != null && closeSound != null) audioSource.PlayOneShot(closeSound);
            UpdateDoorState(true);
        }
    }
}