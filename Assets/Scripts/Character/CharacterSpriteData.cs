using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterSpriteData", menuName = "Blazed Odyssey/Character Sprite Data")]
public class CharacterSpriteData : ScriptableObject
{
    [Header("Base Sprites")]
    public Sprite[] maleBaseSprites;
    public Sprite[] femaleBaseSprites;
    
    [Header("Hair Sprites")]
    public Sprite[] maleHairSprites;
    public Sprite[] femaleHairSprites;
    
    [Header("Equipment Sprites")]
    public ClassEquipmentSprites[] classEquipment;
    
    [Header("Color Palettes")]
    public Color[] skinColors;
    public Color[] hairColors;
    public Color[] eyeColors;
    
    [System.Serializable]
    public class ClassEquipmentSprites
    {
        public string className;
        public Sprite[] maleEquipment;
        public Sprite[] femaleEquipment;
        public Color[] equipmentColors;
    }
}
