using UnityEngine;
using System.Collections.Generic;

public enum TalismanType { None, Ghost, Vampire, SkinWalker, Type4, Type5 }

[System.Serializable]
public class CustomerDialogue
{
    [Header("รูปลูกค้า")]
    public Sprite customerSprite;
    [TextArea(2, 4)]
    public string[] sentences;

    [Header("สินค้าที่ต้องการ (ใส่ได้หลายชิ้น)")]
    public string[] requiredItems;

    [Header("ระบบออเดอร์หลอก")]
    public bool isTrickOrder = false;
    [TextArea(2, 4)]
    [Tooltip("ประโยคตอนหันกลับมา")]
    public string[] trickSentences;
    [Tooltip("สินค้าที่ขอเพิ่ม")]
    public string[] additionalItems;
    [Tooltip("ถ้าติ๊กถูก: หันกลับมารอบ 2 จะกลายเป็นผีโชว์ถาวร")]
    public bool forceAnomalyAfterTrick = false;

    [Header("ระบบสิ่งผิดปกติ (ยันต์)")]
    public bool hasAnomaly = false;
    public bool canGlitch = true;
    public TalismanType weaknessType = TalismanType.None;
    public Sprite glitchSprite;
    public Sprite permanentAnomalySprite;
}

[System.Serializable]
public class DaySetting
{
    public string dayName = "Day";

    [Header("จำนวนโควต้าลูกค้าที่จะมาในวันนี้")]
    public int customersPerDay = 3;

    [Header("NPC พิเศษ")]
    public bool hasGuideNPC = false;
    public CustomerDialogue guideNPC;

    [Header("กองสุ่มลูกค้าทั้งหมด")]
    public CustomerDialogue[] possibleCustomersPool;
    [HideInInspector]
    public List<CustomerDialogue> randomizedQueue = new List<CustomerDialogue>();
}