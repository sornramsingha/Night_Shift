using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("ตำแหน่งกล้อง")]
    public Transform frontViewPosition; 
    public Transform backViewPosition;  

    [Header("ความเร็วตอนหัน")]
    public float panSpeed = 15f; 

    private Vector3 targetPosition; 

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            targetPosition = new Vector3(frontViewPosition.position.x, frontViewPosition.position.y, transform.position.z);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            targetPosition = new Vector3(backViewPosition.position.x, backViewPosition.position.y, transform.position.z);
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
    }
}