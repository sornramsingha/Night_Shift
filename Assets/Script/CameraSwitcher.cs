using UnityEngine;

public class CameraTurnManager : MonoBehaviour
{
    [Header("ลากจุดตั้งกล้องมาใส่เรียงตามลำดับ (ซ้าย -> หน้า -> ขวา -> หลัง)")]
    public Transform[] roomPositions;

    [Header("ความเร็วในการหันกล้อง")]
    public float turnSpeed = 5f;

    private int currentRoom = 1;
    private Vector3 targetPosition;

    void Start()
    {
        if (roomPositions.Length > 0)
        {
            targetPosition = transform.position;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentRoom > 0)
            {
                currentRoom--;
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentRoom < roomPositions.Length - 1)
            {
                currentRoom++;
            }
        }

        if (roomPositions.Length > 0)
        {
            targetPosition = new Vector3(roomPositions[currentRoom].position.x, roomPositions[currentRoom].position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, turnSpeed * Time.deltaTime);
        }
    }
}