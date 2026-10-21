using UnityEngine;

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

    [Header("บทสนทนาของลูกค้าเรียงตามคิว")]
    public CustomerDialogue[] dialogues;
}