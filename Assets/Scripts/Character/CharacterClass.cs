using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "BlazedOdyssey/Character Class")]
public class CharacterClass : ScriptableObject
{
    [Header("Basic Information")]
    public string className;
    [TextArea(2, 4)]
    public string description;
    public Sprite classIcon; // For UI display

    [Header("Base Stats at Level 1")]
    public int baseStrength = 10;
    public int baseIntelligence = 10;
    public int baseDexterity = 10;
    public int baseConstitution = 10;
    public int baseWisdom = 10;

    [Header("Stat Growth Per Level")]
    public float strengthPerLevel = 1.0f;
    public float intelligencePerLevel = 1.0f;
    public float dexterityPerLevel = 1.0f;
    public float constitutionPerLevel = 1.0f;
    public float wisdomPerLevel = 1.0f;

    [Header("Equipment Restrictions")]
    public List<string> allowedWeaponTypes = new List<string>(); // e.g., "Sword", "Staff", "Bow"
    public List<string> allowedArmorTypes = new List<string>();  // e.g., "Heavy", "Light", "Cloth"

    [Header("Starting Spells")]
    public List<string> startingSpellIds = new List<string>(); // IDs of spells this class starts with

    // Methods to calculate stats at a given level
    public int GetStrengthAtLevel(int level) => baseStrength + Mathf.FloorToInt(strengthPerLevel * (level - 1));
    public int GetIntelligenceAtLevel(int level) => baseIntelligence + Mathf.FloorToInt(intelligencePerLevel * (level - 1));
    public int GetDexterityAtLevel(int level) => baseDexterity + Mathf.FloorToInt(dexterityPerLevel * (level - 1));
    public int GetConstitutionAtLevel(int level) => baseConstitution + Mathf.FloorToInt(constitutionPerLevel * (level - 1));
    public int GetWisdomAtLevel(int level) => baseWisdom + Mathf.FloorToInt(wisdomPerLevel * (level - 1));
    
    // Helper method to get all stats at once
    public CharacterStats GetStatsAtLevel(int level)
    {
        return new CharacterStats
        {
            strength = GetStrengthAtLevel(level),
            intelligence = GetIntelligenceAtLevel(level),
            dexterity = GetDexterityAtLevel(level),
            constitution = GetConstitutionAtLevel(level),
            wisdom = GetWisdomAtLevel(level)
        };
    }
}

[System.Serializable]
public struct CharacterStats
{
    public int strength;
    public int intelligence;
    public int dexterity;
    public int constitution;
    public int wisdom;
}
