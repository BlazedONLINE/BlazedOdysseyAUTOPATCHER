using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    [Header("Basic Info")]
    public string characterId;
    public string characterName;
    public string playerEmail; // Links to the player account
    
    [Header("Character Stats")]
    public CharacterClass characterClass;
    public int level;
    public int experience;
    public int experienceToNext;
    
    [Header("Combat Stats")]
    public int health;
    public int maxHealth;
    public int mana;
    public int maxMana;
    public int attack;
    public int defense;
    public int magic;
    
    [Header("Progression")]
    public string rank; // "Novice", "Veteran", "Elite", "Master", "Legend"
    public int rankStars; // 1-5 stars within rank
    public DateTime lastPlayed;
    public float totalPlayTime; // Hours played
    
    [Header("Location")]
    public string currentZone;
    public Vector3 lastPosition;
    
    [Header("Appearance")]
    public int skinColor;
    public int hairStyle;
    public int hairColor;
    public int eyeColor;
    
    [Header("Equipment")]
    public List<string> equippedItems;
    public List<string> inventory;
    
    [Header("Currency")]
    public int gold;
    public int gems;
    
    // Constructor
    public CharacterData(string name, CharacterClass charClass, string email)
    {
        characterId = System.Guid.NewGuid().ToString();
        characterName = name;
        characterClass = charClass;
        playerEmail = email;
        
        // Starting stats based on class
        level = 1;
        experience = 0;
        experienceToNext = 100;
        
        SetStartingStats(charClass);
        
        rank = "Novice";
        rankStars = 1;
        lastPlayed = DateTime.Now;
        totalPlayTime = 0f;
        
        currentZone = "Starting Village";
        lastPosition = Vector3.zero;
        
        // Default appearance
        skinColor = 0;
        hairStyle = 0;
        hairColor = 0;
        eyeColor = 0;
        
        equippedItems = new List<string>();
        inventory = new List<string>();
        
        gold = 100; // Starting gold
        gems = 0;
    }
    
    private void SetStartingStats(CharacterClass charClass)
    {
        switch (charClass.className)
        {
            case "Warrior":
                maxHealth = 120;
                maxMana = 30;
                attack = 15;
                defense = 12;
                magic = 5;
                break;
                
            case "Mage":
                maxHealth = 80;
                maxMana = 100;
                attack = 8;
                defense = 6;
                magic = 18;
                break;
                
            case "Archer":
                maxHealth = 100;
                maxMana = 60;
                attack = 14;
                defense = 8;
                magic = 10;
                break;
                
            case "Rogue":
                maxHealth = 90;
                maxMana = 50;
                attack = 16;
                defense = 7;
                magic = 8;
                break;
                
            default:
                maxHealth = 100;
                maxMana = 50;
                attack = 10;
                defense = 8;
                magic = 10;
                break;
        }
        
        health = maxHealth;
        mana = maxMana;
    }
    
    public string GetRankDisplay()
    {
        string stars = "";
        for (int i = 0; i < rankStars; i++)
        {
            stars += "⭐";
        }
        return $"{rank} {stars}";
    }
    
    public string GetLastPlayedDisplay()
    {
        TimeSpan timeDiff = DateTime.Now - lastPlayed;
        
        if (timeDiff.TotalDays >= 1)
            return $"{(int)timeDiff.TotalDays} days ago";
        else if (timeDiff.TotalHours >= 1)
            return $"{(int)timeDiff.TotalHours} hours ago";
        else if (timeDiff.TotalMinutes >= 1)
            return $"{(int)timeDiff.TotalMinutes} minutes ago";
        else
            return "Just now";
    }
    
    public Color GetRankColor()
    {
        switch (rank)
        {
            case "Novice": return new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
            case "Veteran": return new Color(0.4f, 0.8f, 0.4f, 1f); // Green
            case "Elite": return new Color(0.4f, 0.6f, 1f, 1f); // Blue
            case "Master": return new Color(0.8f, 0.4f, 1f, 1f); // Purple
            case "Legend": return new Color(1f, 0.8f, 0.2f, 1f); // Gold
            default: return Color.white;
        }
    }
}
