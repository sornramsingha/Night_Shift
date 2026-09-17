using UnityEngine;
using System.Collections;

public class CameraTurnManager : MonoBehaviour
{
    [Header("ลากจุดตั้งกล้องมาใส่เรียงตามลำดับ (ซ้าย -> หน้า -> ขวา -> หลัง)")]
    public Transform[] roomPositions;

    [Header("ความเร็วในการหันกล้อง")]
    public float turnSpeed = 25f;

    [Header("ตั้งค่าดีเลย์ (วินาที)")]
    public float turnCooldown = 0.5f;

    private int currentRoom = 0;
    private bool isTurning = false;

    void Start()
    {
        if (roomPositions.Length > 0)
        {
            transform.position = new Vector3(roomPositions[currentRoom].position.x, roomPositions[currentRoom].position.y, transform.position.z);
        }
    }

    void Update()
    {
        if (isTurning || roomPositions.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.Q) && currentRoom > 0)
        {
            StartCoroutine(WhipPanToRoom(currentRoom - 1));
        }

        if (Input.GetKeyDown(KeyCode.E) && currentRoom < roomPositions.Length - 1)
        {
            StartCoroutine(WhipPanToRoom(currentRoom + 1));
        }
    }

    IEnumerator WhipPanToRoom(int targetIndex)
    {
        isTurning = true;
        currentRoom = targetIndex;

        Vector3 targetPosition = new Vector3(
            roomPositions[currentRoom].position.x,
            roomPositions[currentRoom].position.y,
            transform.position.z
        );

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, turnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        yield return new WaitForSeconds(turnCooldown);

        isTurning = false;
    }
}