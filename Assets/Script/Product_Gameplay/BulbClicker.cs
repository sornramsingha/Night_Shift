using UnityEngine;

public class BulbClicker : MonoBehaviour
{
    void OnMouseDown()
    {
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            if (LightRepairMinigame.Instance != null && !LightRepairMinigame.Instance.isPlaying)
            {
                LightRepairMinigame.Instance.StartMinigame();
            }
        }
    }
}