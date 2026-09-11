using UnityEngine;

public class CustomerWalker : MonoBehaviour
{
    [Header("จุดเริ่มต้นและจุดสิ้นสุด")]
    public Transform spawnPoint;
    public Transform counterPoint;

    [Header("ตั้งค่าแอนิเมชันเดิน")]
    public float walkSpeed = 5f;
    public float bobbingHeight = 0.5f;
    public float bobbingSpeed = 15f;

    [Header("เชื่อมต่อระบบสนทนา")]
    public DialogueManager dialogueManager;

    private bool isWalking = false;

    void Start()
    {
        transform.position = spawnPoint.position;
        isWalking = true;
    }

    void Update()
    {
        if (!isWalking) return;
        float step = walkSpeed * Time.deltaTime;
        float newX = Mathf.MoveTowards(transform.position.x, counterPoint.position.x, step);
        float newY = counterPoint.position.y + (Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight);
        transform.position = new Vector3(newX, newY, transform.position.z);

        if (Mathf.Abs(transform.position.x - counterPoint.position.x) < 0.05f)
        {
            transform.position = new Vector3(counterPoint.position.x, counterPoint.position.y, transform.position.z);
            isWalking = false;
            if (dialogueManager != null)
            {
                dialogueManager.ShowStartTalkButton();
            }
        }
    }
}