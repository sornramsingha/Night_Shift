using UnityEngine;

public class CustomerWalker : MonoBehaviour
{
    [Header("จุดเดินต่างๆ")]
    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint; 

    [Header("ตั้งค่าแอนิเมชันเดิน")]
    public float walkSpeed = 5f;
    public float bobbingHeight = 0.5f;
    public float bobbingSpeed = 15f;

    [Header("เชื่อมต่อระบบอื่น")]
    public DialogueManager dialogueManager;
    public DayManager dayManager;

    private bool isWalkingIn = false;
    private bool isWalkingOut = false;

    void Start()
    {
        transform.position = spawnPoint.position;
    }

    void Update()
    {
        if (isWalkingIn)
        {
            float step = walkSpeed * Time.deltaTime;
            float newX = Mathf.MoveTowards(transform.position.x, counterPoint.position.x, step);
            float newY = counterPoint.position.y + (Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight);
            transform.position = new Vector3(newX, newY, transform.position.z);

            if (Mathf.Abs(transform.position.x - counterPoint.position.x) < 0.05f)
            {
                transform.position = new Vector3(counterPoint.position.x, counterPoint.position.y, transform.position.z);
                isWalkingIn = false;
                if (dialogueManager != null) dialogueManager.ShowStartTalkButton();
            }
        }
        else if (isWalkingOut)
        {
            float step = walkSpeed * Time.deltaTime;
            float newX = Mathf.MoveTowards(transform.position.x, exitPoint.position.x, step);
            float newY = exitPoint.position.y + (Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight);
            transform.position = new Vector3(newX, newY, transform.position.z);

            if (Mathf.Abs(transform.position.x - exitPoint.position.x) < 0.05f)
            {
                isWalkingOut = false;
                if (dayManager != null) dayManager.SpawnNextCustomer();
            }
        }
    }

    public void ResetAndWalk()
    {
        transform.position = spawnPoint.position;
        isWalkingIn = true;
        isWalkingOut = false;
    }

    public void WalkAway()
    {
        isWalkingIn = false;
        isWalkingOut = true;
    }
}