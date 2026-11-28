using UnityEngine;
using System.Collections.Generic;

public partial class DialogueManager
{
    public void OnCameraTurnedBack()
    {
        if (!isWaitingForOrder || currentData == null) return;
        if (itemsInBag.Count == 0) return;

        cameraTurnCount++;
        if (hasPendingTrickOrder && cameraTurnCount == 1)
        {
            isReadyToTalkTrick = true;
            if (trickBubbleButton != null) trickBubbleButton.SetActive(true);

            if (isBagOnTray)
            {
                isBagOnTray = false;
                hasBag = true;
                if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);
                if (activeBagUI != null) activeBagUI.SetActive(true);
                if (giveItemButton != null) giveItemButton.SetActive(false);
            }
        }
        else if (cameraTurnCount == 2 && currentData.isTrickOrder && currentData.forceAnomalyAfterTrick)
        {
            if (CustomerAnomalyVisual.CurrentActive != null)
            {
                if (Random.value > 0.5f) CustomerAnomalyVisual.CurrentActive.ForceRevealAnomaly();
                else CustomerAnomalyVisual.CurrentActive.TriggerAggressiveGlitch();
            }
        }
    }

    public void TriggerTrickDialogue()
    {
        if (trickBubbleButton != null) trickBubbleButton.SetActive(false);

        isReadyToTalkTrick = false;
        hasPendingTrickOrder = false;
        isWaitingForOrder = false;

        if (itemsInBag.Count > 0)
        {
            hasBag = true;
            isBagOnTray = false;
            if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);
            if (activeBagUI != null) activeBagUI.SetActive(true);
            if (giveItemButton != null) giveItemButton.SetActive(false);
        }

        if (currentData.additionalItems != null)
        {
            expectedItems.AddRange(currentData.additionalItems);
            maxBagCapacity = expectedItems.Count;
        }

        if (currentData.trickSentences != null && currentData.trickSentences.Length > 0)
        {
            StartDialogue(currentData.trickSentences);
        }
    }

    public void ShowBagResetConfirmation()
    {
        if (isConfirmingBagReset) return;
        isConfirmingBagReset = true;
        if (bagConfirmPanel != null) bagConfirmPanel.SetActive(true);
    }

    public void ConfirmBagResetYes()
    {
        isConfirmingBagReset = false;
        if (bagConfirmPanel != null) bagConfirmPanel.SetActive(false);
        ResetBagItems();
    }

    public void ConfirmBagResetNo()
    {
        isConfirmingBagReset = false;
        if (bagConfirmPanel != null) bagConfirmPanel.SetActive(false);
    }

    public void PickUpBag()
    {
        if (isDialogueActive) { WarningManager.Show("ฟังออเดอร์ให้จบก่อน!"); return; }
        if (!isWaitingForOrder) { WarningManager.Show("ยังไม่มีออเดอร์!"); return; }
        if (isBagOnTray) { WarningManager.Show("มีถุงวางอยู่บนถาดแล้ว!"); return; }
        if (isHoldingTalisman) { WarningManager.Show("มือไม่ว่าง! กำลังถือยันต์อยู่"); return; }
        if (hasBag) return;

        hasBag = true;
        if (activeBagUI != null) activeBagUI.SetActive(true);
    }

    public void ResetBagItems()
    {
        if (CheckoutManager.Instance != null && CheckoutManager.Instance.hasReceivedMoney)
        {
            WarningManager.Show("รับเงินลูกค้ามาแล้ว เปลี่ยนถุงไม่ได้!");
            return;
        }

        if (hasBag || isBagOnTray)
        {
            itemsInBag.Clear();
            hasBag = false;
            isBagOnTray = false;

            if (activeBagUI != null) activeBagUI.SetActive(false);
            if (bagOnTrayUI != null) bagOnTrayUI.SetActive(false);
            if (giveItemButton != null) giveItemButton.SetActive(false);
            if (CheckoutManager.Instance != null) CheckoutManager.Instance.HideHandMoney();

            WarningManager.Show("เปลี่ยนถุงใหม่ ต้องหยิบใหม่ทั้งหมด");
        }
    }

    public bool PickUpItem(string itemName)
    {
        if (ShopLightController.Instance != null && ShopLightController.Instance.isBroken)
        {
            WarningManager.Show("ไฟดับ! ต้องซ่อมไฟก่อน!");
            return false;
        }
        if (isDialogueActive)
        {
            WarningManager.Show("ลูกค้ายังพูดไม่จบ!");
            return false;
        }
        if (!isWaitingForOrder)
        {
            WarningManager.Show("ยังไม่ได้กดรับออเดอร์เลย!");
            return false;
        }
        if (!hasBag) return false;

        if (itemsInBag.Count >= maxBagCapacity)
        {
            WarningManager.Show("ถุงเต็มแล้ว!");
            return false;
        }

        itemsInBag.Add(itemName);
        return true;
    }

    public void PlaceBagOnTray()
    {
        if (!isWaitingForOrder)
        {
            WarningManager.Show("ต้องรับออเดอร์จากลูกค้าก่อน!");
            return;
        }
        if (!hasBag || itemsInBag.Count == 0 || isBagOnTray) return;

        if (isReadyToTalkTrick)
        {
            WarningManager.Show("เหมือนลูกค้าจะสั่งอะไรเพิ่มนะ?");
            return;
        }

        if (itemsInBag.Count < maxBagCapacity)
        {
            WarningManager.Show($"ของในถุงยังไม่ครบ! ({itemsInBag.Count}/{maxBagCapacity})");
            return;
        }

        isBagOnTray = true;
        hasBag = false;

        CloseDialogueBox();
        if (activeBagUI != null) activeBagUI.SetActive(false);
        if (bagOnTrayUI != null) bagOnTrayUI.SetActive(true);
        if (startTalkButton != null) startTalkButton.SetActive(false);
        if (giveItemButton != null) giveItemButton.SetActive(false);

        if (CheckoutManager.Instance != null)
        {
            CheckoutManager.Instance.PrepareCustomerPayment(expectedItems, customerHasAnomaly);
        }
    }

    public void CompleteSaleWithChange(int actualTotal, int paidAmount, int givenChange, bool wasGhostMoney)
    {
        bool isExactlyCorrect = true;
        if (itemsInBag.Count != expectedItems.Count)
        {
            isExactlyCorrect = false;
        }
        else
        {
            List<string> tempExpected = new List<string>(expectedItems);
            foreach (string item in itemsInBag)
            {
                if (tempExpected.Contains(item)) tempExpected.Remove(item);
                else { isExactlyCorrect = false; break; }
            }
        }

        int requiredChange = Mathf.Max(0, paidAmount - actualTotal);
        bool isChangeCorrect = (givenChange == requiredChange);

        if (customerHasAnomaly || wasGhostMoney)
        {
            WarningManager.Show(wasGhostMoney ? "พลาดแล้ว! นั่นมันเงินผี!" : "พลาดแล้ว! ขายของให้สิ่งผิดปกติ!");
            if (SanityManager.Instance != null)
                SanityManager.Instance.ApplyPenalty(SanityManager.Instance.penaltyAnomalyWrong);
        }
        else if (!isExactlyCorrect)
        {
            WarningManager.Show("หยิบสินค้าให้ลูกค้าผิด!");
        }
        else if (isChangeCorrect)
        {
            if (MoneyManager.Instance != null) MoneyManager.Instance.AddMoney(actualTotal);
            WarningManager.Show($"ทอนเงินถูกต้อง! (+{actualTotal} บาท)");
        }
        else
        {
            int reducedMoney = Mathf.RoundToInt(actualTotal * 0.5f);
            if (MoneyManager.Instance != null) MoneyManager.Instance.AddMoney(reducedMoney);
            WarningManager.Show($"ทอนเงินเกิน! ได้รับเงินเพียง 50% (+{reducedMoney} ฿)");
        }

        EndCustomerInteraction();
        if (customerWalker != null) customerWalker.WalkAway();
        isTimerRunning = false;
        if (patienceUIContainer != null) patienceUIContainer.SetActive(false);
    }
}