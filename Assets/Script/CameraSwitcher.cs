using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    [Header("จุดตั้งกล้องในแต่ละห้อง")]
    public Transform[] roomPositions;

    [Header("ความเร็วในการเลื่อนกล้อง")]
    public float panSpeed = 4f;
    public float cooldownTime = 0.2f;

    [Header("ระบบ UI")]
    public GameObject dialogueUIGroup;

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

        if (Input.GetKeyDown(KeyCode.Q) && currentRoomIndex > 0)
        {
            StartCoroutine(SmoothPanToRoom(currentRoomIndex - 1));
        }

        if (Input.GetKeyDown(KeyCode.E) && currentRoomIndex < roomPositions.Length - 1)
        {
            StartCoroutine(SmoothPanToRoom(currentRoomIndex + 1));
        }
    }

    IEnumerator SmoothPanToRoom(int targetIndex)
    {
        isPanning = true;
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
        if (dialogueUIGroup != null)
        {
            dialogueUIGroup.SetActive(currentRoomIndex == 0);
        }
    }
}