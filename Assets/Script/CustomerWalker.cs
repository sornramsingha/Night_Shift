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

    [Header("ตั้งค่าเงาดำ (Fade)")]
    public Color shadowColor = Color.black;
    public Color normalColor = Color.white;

    [Tooltip("กราฟปรับความเข้มของสี")]
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("เชื่อมต่อระบบอื่น")]
    public DialogueManager dialogueManager;
    public DayManager dayManager;

    private bool isWalkingIn = false;
    private bool isWalkingOut = false;

    private SpriteRenderer spriteRenderer;
    private float totalDistanceIn;
    private float totalDistanceOut;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

            if (spriteRenderer != null && totalDistanceIn > 0)
            {
                float currentDist = Mathf.Abs(transform.position.x - counterPoint.position.x);
                float distancePercent = Mathf.Clamp01(1f - (currentDist / totalDistanceIn)); // 0 = ไกล, 1 = ถึงเคาน์เตอร์
                float curveValue = fadeCurve.Evaluate(distancePercent);
                spriteRenderer.color = Color.Lerp(shadowColor, normalColor, curveValue);
            }

            if (Mathf.Abs(transform.position.x - counterPoint.position.x) < 0.05f)
            {
                transform.position = new Vector3(counterPoint.position.x, counterPoint.position.y, transform.position.z);
                isWalkingIn = false;

                if (spriteRenderer != null) spriteRenderer.color = normalColor;
                if (dialogueManager != null) dialogueManager.ShowStartTalkButton();
            }
        }
        else if (isWalkingOut)
        {
            float step = walkSpeed * Time.deltaTime;
            float newX = Mathf.MoveTowards(transform.position.x, exitPoint.position.x, step);
            float newY = exitPoint.position.y + (Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight);
            transform.position = new Vector3(newX, newY, transform.position.z);

            if (spriteRenderer != null && totalDistanceOut > 0)
            {
                float currentDist = Mathf.Abs(transform.position.x - counterPoint.position.x);
                float distancePercent = Mathf.Clamp01(currentDist / totalDistanceOut);
                float curveValue = fadeCurve.Evaluate(distancePercent);
                spriteRenderer.color = Color.Lerp(normalColor, shadowColor, curveValue);
            }

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
        totalDistanceIn = Mathf.Abs(spawnPoint.position.x - counterPoint.position.x);
        if (spriteRenderer != null) spriteRenderer.color = shadowColor;
        isWalkingIn = true;
        isWalkingOut = false;
    }

    public void WalkAway()
    {
        totalDistanceOut = Mathf.Abs(exitPoint.position.x - counterPoint.position.x);
        isWalkingIn = false;
        isWalkingOut = true;
    }
}