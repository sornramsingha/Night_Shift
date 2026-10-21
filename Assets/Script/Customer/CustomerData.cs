using UnityEngine;
using System.Collections.Generic;

public enum TalismanType
{
    None,
    Ghost,
    Vampire,
    SkinWalker,
    Type4,
    Type5
}

[System.Serializable]
public class CustomerDialogue
{
    [Header("รูปลูกค้า")]
    public Sprite customerSprite;
    [TextArea(2, 4)]
    public string[] sentences;
    [Header("สินค้าที่ต้องการ")]
    public string requiredItem;

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

    [Header("กองสุ่มลูกค้าทั้งหมด")]
    public CustomerDialogue[] possibleCustomersPool;

    [HideInInspector]
    public List<CustomerDialogue> randomizedQueue = new List<CustomerDialogue>();
}