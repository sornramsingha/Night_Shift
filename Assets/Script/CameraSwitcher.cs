using UnityEngine;
using System.Collections;

public class CameraTurnManager : MonoBehaviour
{
    [Header("จุดตั้งกล้องในแต่ละห้อง (เรียงจากซ้ายไปขวา)")]
    public Transform[] roomPositions;

    [Header("ความเร็วในการเลื่อนกล้อง")]
    [Tooltip("ค่าน้อย = เลื่อนช้าๆ / ค่ามาก = เลื่อนฟึ่บฟั่บ")]
    public float panSpeed = 4f;

    [Header("เวลาหน่วงหลังเลื่อนเสร็จ (กันผู้เล่นกดรัว)")]
    public float cooldownTime = 0.2f;

    private int currentRoomIndex = 0;
    private bool isPanning = false;

    void Start()
    {
        // พอเริ่มเกม ให้ดึงกล้องไปอยู่ตรงกลางของห้องแรกทันที
        if (roomPositions.Length > 0)
        {
            transform.position = new Vector3(
                roomPositions[currentRoomIndex].position.x,
                roomPositions[currentRoomIndex].position.y,
                transform.position.z // คงค่าแกน Z ของกล้องไว้เพื่อไม่ให้ภาพหาย
            );
        }
    }

    void Update()
    {
        // ถ้าย้ายห้องอยู่ หรือยังไม่ได้ลากจุดตั้งกล้องมาใส่ ให้ข้ามคำสั่งกดปุ่มไปเลย
        if (isPanning || roomPositions.Length == 0) return;

        // กด Q (หันซ้าย)
        if (Input.GetKeyDown(KeyCode.Q) && currentRoomIndex > 0)
        {
            StartCoroutine(SmoothPanToRoom(currentRoomIndex - 1));
        }

        // กด E (หันขวา)
        if (Input.GetKeyDown(KeyCode.E) && currentRoomIndex < roomPositions.Length - 1)
        {
            StartCoroutine(SmoothPanToRoom(currentRoomIndex + 1));
        }
    }

    IEnumerator SmoothPanToRoom(int targetIndex)
    {
        isPanning = true;
        currentRoomIndex = targetIndex;

        // คำนวณจุดหมายปลายทางที่กล้องต้องไป
        Vector3 targetPosition = new Vector3(
            roomPositions[currentRoomIndex].position.x,
            roomPositions[currentRoomIndex].position.y,
            transform.position.z
        );

        // วนลูปเลื่อนตำแหน่งกล้องไปเรื่อยๆ จนกว่าจะเข้าใกล้จุดหมายมากๆ (ระยะห่างน้อยกว่า 0.01)
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // Vector3.Lerp คือสมการคณิตศาสตร์ที่ช่วยให้ภาพเลื่อนไหลแบบค่อยเป็นค่อยไป
            transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
            yield return null; // รอให้ขึ้นเฟรมใหม่แล้วค่อยคำนวณลูปต่อ (ป้องกันเกมค้าง)
        }

        // เมื่อถึงที่หมายแล้ว บังคับล็อคตำแหน่งให้เป๊ะ เพื่อไม่ให้ภาพเบี้ยวหรือเลยขอบ
        transform.position = targetPosition;

        // ติดคูลดาวน์แป๊บนึง
        yield return new WaitForSeconds(cooldownTime);

        isPanning = false; // ปลดล็อคให้กดปุ่มไปต่อได้
    }
}