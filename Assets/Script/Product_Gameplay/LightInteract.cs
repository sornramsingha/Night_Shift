using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class LightInteract : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("คลิกโดนหลอดไฟแล้ว!");
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            if (LightRepairMinigame.Instance != null && !LightRepairMinigame.Instance.isPlaying)
            {
                LightRepairMinigame.Instance.StartMinigame();
            }
        }
    }
}