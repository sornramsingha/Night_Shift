using UnityEngine;

public class CameraMouseSway : MonoBehaviour
{
    [Header("ระยะการส่ายของกล้อง")]
    public float swayAmountX = 0.5f;
    public float swayAmountY = 0.2f;

    [Header("ความนุ่มนวล")]
    public float smoothSpeed = 3f;

    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
        float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;

        Vector3 targetLocalPos = new Vector3(
            startLocalPos.x + (mouseX * swayAmountX),
            startLocalPos.y + (mouseY * swayAmountY),
            startLocalPos.z
        );
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, smoothSpeed * Time.deltaTime);
    }
}