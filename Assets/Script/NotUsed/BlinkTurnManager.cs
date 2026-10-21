using UnityEngine;
using System.Collections;

public class BlinkTurnManager : MonoBehaviour
{
    [Header("จุดตั้งกล้อง")]
    public Transform frontViewPosition;
    public Transform backViewPosition;

    [Header("UI เปลือกตา")]
    public RectTransform topEyelid;
    public RectTransform bottomEyelid;

    [Header("ตั้งค่าความเร็วหลับตา")]
    public float blinkSpeed = 0.15f; 
    public float darkTime = 0.1f;
    public float turnCooldown = 0.5f;

    private bool isFront = true;
    private bool isTurning = false;

    private float topOpenY = 2000f;
    private float bottomOpenY = -2000f;
    private float topClosedY = 500f;
    private float bottomClosedY = -500f;

    void Start()
    {
        if (topEyelid != null) topEyelid.anchoredPosition = new Vector2(0, topOpenY);
        if (bottomEyelid != null) bottomEyelid.anchoredPosition = new Vector2(0, bottomOpenY);

        if (frontViewPosition != null) MoveCameraTo(frontViewPosition);
    }

    void Update()
    {
        if (isTurning) return;

        if (Input.GetKeyDown(KeyCode.E) && isFront)
        {
            StartCoroutine(BlinkAndTurn(backViewPosition, false));
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isFront)
        {
            StartCoroutine(BlinkAndTurn(frontViewPosition, true));
        }
    }

    IEnumerator BlinkAndTurn(Transform targetPosition, bool goingToFront)
    {
        isTurning = true;

        yield return StartCoroutine(MoveEyelids(topClosedY, bottomClosedY, blinkSpeed));
        MoveCameraTo(targetPosition);
        isFront = goingToFront;

        yield return new WaitForSeconds(darkTime);
        yield return StartCoroutine(MoveEyelids(topOpenY, bottomOpenY, blinkSpeed));
        yield return new WaitForSeconds(turnCooldown);

        isTurning = false;
    }

    IEnumerator MoveEyelids(float targetTopY, float targetBottomY, float duration)
    {
        if (topEyelid == null || bottomEyelid == null) yield break;

        Vector2 startTop = topEyelid.anchoredPosition;
        Vector2 startBottom = bottomEyelid.anchoredPosition;

        Vector2 endTop = new Vector2(0, targetTopY);
        Vector2 endBottom = new Vector2(0, targetBottomY);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            topEyelid.anchoredPosition = Vector2.Lerp(startTop, endTop, smoothStep);
            bottomEyelid.anchoredPosition = Vector2.Lerp(startBottom, endBottom, smoothStep);

            yield return null;
        }
        topEyelid.anchoredPosition = endTop;
        bottomEyelid.anchoredPosition = endBottom;
    }

    void MoveCameraTo(Transform target)
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}