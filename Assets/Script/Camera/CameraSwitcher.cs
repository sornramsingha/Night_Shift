using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    public Transform[] roomPositions;
    public float panSpeed = 4f;
    public float cooldownTime = 0.2f;

    [Header("ระบบ UI")]
    public GameObject dialogueUIGroup;
    public GameObject bagUIGroup;

    private int currentRoomIndex = 0;
    private bool isPanning = false;

    void Start()
    {
        if (roomPositions.Length > 0)
        {
            transform.position = new Vector3(roomPositions[currentRoomIndex].position.x, roomPositions[currentRoomIndex].position.y, transform.position.z);
        }
        UpdateUIVisibility();
    }

    void Update()
    {
        if (isPanning || roomPositions.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.Q)) TurnLeft();
        if (Input.GetKeyDown(KeyCode.E)) TurnRight();
    }

    public void TurnLeft()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.isFacingFront = true;
        if (currentRoomIndex > 0 && !isPanning)
            StartCoroutine(SmoothPanToRoom(currentRoomIndex - 1));
    }

    public void TurnRight()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.isFacingFront = false;
        if (currentRoomIndex < roomPositions.Length - 1 && !isPanning)
            StartCoroutine(SmoothPanToRoom(currentRoomIndex + 1));
    }

    IEnumerator SmoothPanToRoom(int targetIndex)
    {
        isPanning = true;

        if (FridgeDoorInteract.Instance != null)
        {
            FridgeDoorInteract.Instance.ForceCloseDoor();
        }

        if (targetIndex == 0 && currentRoomIndex != 0)
        {
            if (CustomerAnomalyVisual.CurrentActive != null)
                CustomerAnomalyVisual.CurrentActive.ToggleAnomalyOnTurn();

            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnCameraTurnedBack();
        }

        currentRoomIndex = targetIndex;
        UpdateUIVisibility();

        Vector3 targetPosition = new Vector3(
            roomPositions[currentRoomIndex].position.x,
            roomPositions[currentRoomIndex].position.y,
            transform.position.z
        );

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        yield return new WaitForSeconds(cooldownTime);
        isPanning = false;
    }

    void UpdateUIVisibility()
    {
        if (dialogueUIGroup != null) dialogueUIGroup.SetActive(currentRoomIndex == 0);
        if (bagUIGroup != null && DialogueManager.Instance != null)
        {
            bool shouldShowBag = DialogueManager.Instance.hasBag && (currentRoomIndex == 0 || currentRoomIndex == 1);
            bagUIGroup.SetActive(shouldShowBag);
        }
    }
}