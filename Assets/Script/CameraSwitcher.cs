using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera Position")]
    public Transform frontViewPosition; 
    public Transform backViewPosition;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.position = new Vector3(frontViewPosition.position.x, frontViewPosition.position.y, transform.position.z);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            transform.position = new Vector3(backViewPosition.position.x, backViewPosition.position.y, transform.position.z);
        }
    }
}