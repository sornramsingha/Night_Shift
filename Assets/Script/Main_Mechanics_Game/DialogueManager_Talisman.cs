using UnityEngine;

public partial class DialogueManager
{
    public void SetupAnomalyData(bool hasAnomaly, TalismanType weakness)
    {
        customerHasAnomaly = hasAnomaly;
        customerWeakness = weakness;
    }

    public void PickUpTalisman(TalismanType type, GameObject talismanObj, Sprite talismanSprite)
    {
        if (!isWaitingForOrder) { WarningManager.Show("ยังไม่จำเป็นต้องใช้ตอนนี้"); return; }
        if (hasBag) { WarningManager.Show("มือไม่ว่าง! กำลังถือถุงอยู่"); return; }

        if (isHoldingTalisman && heldTalisman == type)
        {
            ClearTalismanInHand();
            return;
        }

        isHoldingTalisman = true;
        heldTalisman = type;
        activeTalismanItem = talismanObj;

        if (activeTalismanUI != null)
        {
            activeTalismanUI.SetActive(true);
            if (centerTalismanSprite != null && talismanSprite != null)
            {
                centerTalismanSprite.sprite = talismanSprite;
            }
        }
    }

    public void UseTalismanOnCustomer()
    {
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            WarningManager.Show("มืดขนาดนี้ มองไม่เห็น ต้องซ่อมไฟก่อน!");
            return;
        }
        if (!isFacingFront) return;
        if (!isHoldingTalisman || !isWaitingForOrder) return;

        if (activeTalismanItem != null) activeTalismanItem.SetActive(false);

        if (customerHasAnomaly && heldTalisman == customerWeakness)
        {
            Debug.Log("แปะยันต์ถูกต้อง! ขับไล่สำเร็จ");
            if (SanityManager.Instance != null)
                SanityManager.Instance.ApplyReward(SanityManager.Instance.rewardCorrectBanish);

            customerHasAnomaly = false;
            EndCustomerInteraction();
            if (customerWalker != null) customerWalker.Vanish();
        }
        else
        {
            Debug.Log("แปะยันต์ผิด! ลูกค้าโกรธเดินหนี");
            if (SanityManager.Instance != null)
                SanityManager.Instance.ApplyPenalty(SanityManager.Instance.penaltyTalismanWrong);

            EndCustomerInteraction();
            if (customerWalker != null) customerWalker.WalkAway();
            isTimerRunning = false;
            if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
        }
    }

    private void ClearTalismanInHand()
    {
        isHoldingTalisman = false;
        heldTalisman = TalismanType.None;
        activeTalismanItem = null;
        if (activeTalismanUI != null) activeTalismanUI.SetActive(false);
    }
}