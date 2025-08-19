using UnityEngine;
using System;

/// <summary>
/// NexusTK-inspired experience and level progression system
/// Features level 1-99 progression with stat points after 99
/// </summary>
public class PlayerExperience : MonoBehaviour
{
    [Header("Level Progression")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private long currentExperience = 0;
    [SerializeField] private long experienceToNext = 100;
    [SerializeField] private int maxLevel = 99;
    
    [Header("Stats (NexusTK Style)")]
    [SerializeField] private int strength = 10;
    [SerializeField] private int magic = 10;
    [SerializeField] private int constitution = 10;
    [SerializeField] private int dexterity = 10;
    [SerializeField] private int wisdom = 10;
    
    [Header("Post-99 Stat Points")]
    [SerializeField] private int availableStatPoints = 0;
    [SerializeField] private long experienceForStatPoint = 10000; // Experience needed for 1 stat point
    
    [Header("Class System")]
    [SerializeField] private PlayerClass playerClass = PlayerClass.Warrior;
    [SerializeField] private bool hasChosenClass = false;
    
    // Events for UI updates
    public Action<int, long, long> OnExperienceChanged; // (level, currentExp, expToNext)
    public Action<int> OnLevelUp; // (newLevel)
    public Action<int> OnStatPointsChanged; // (availablePoints)
    public Action<PlayerStats> OnStatsChanged; // (stats)
    
    // References
    private BlazedOdyssey.UI.PlayerHUD playerHUD;
    private PlayerHealth playerHealth;
    
    // Experience curve (NexusTK style - exponential)
    private readonly long[] experienceTable = GenerateExperienceTable();
    
    public enum PlayerClass
    {
        Warrior,    // High STR/CON, melee focused
        Rogue,      // High DEX, stealth/speed focused  
        Mage,       // High MAG/WIS, magic focused
        Poet        // Balanced stats, support focused
    }
    
    public struct PlayerStats
    {
        public int strength;
        public int magic;
        public int constitution;
        public int dexterity;
        public int wisdom;
        public int level;
        public long experience;
        public int statPoints;
        
        public PlayerStats(int str, int mag, int con, int dex, int wis, int lvl, long exp, int statPts)
        {
            strength = str; magic = mag; constitution = con; 
            dexterity = dex; wisdom = wis; level = lvl; 
            experience = exp; statPoints = statPts;
        }
    }
    
    void Start()
    {
        // Find references
        playerHUD = UnityEngine.Object.FindFirstObjectByType<BlazedOdyssey.UI.PlayerHUD>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // Initialize with starting stats based on class
        if (!hasChosenClass)
        {
            ApplyClassStartingStats();
        }
        
        // Update UI initially
        UpdateExperienceDisplay();
        UpdateStatsDisplay();
        
        Debug.Log($"🎯 PlayerExperience initialized: Level {currentLevel}, Class {playerClass}");
    }
    
    static long[] GenerateExperienceTable()
    {
        long[] table = new long[100]; // Level 1-99
        table[0] = 0; // Level 1
        
        for (int i = 1; i < 100; i++)
        {
            // NexusTK-style exponential curve
            table[i] = (long)(100 * Math.Pow(1.1, i) + (i * i * 50));
        }
        
        return table;
    }
    
    void ApplyClassStartingStats()
    {
        // Reset to base stats
        strength = magic = constitution = dexterity = wisdom = 10;
        
        // Apply class bonuses (NexusTK style)
        switch (playerClass)
        {
            case PlayerClass.Warrior:
                strength += 5;
                constitution += 3;
                break;
            case PlayerClass.Rogue:
                dexterity += 5;
                strength += 2;
                constitution += 1;
                break;
            case PlayerClass.Mage:
                magic += 5;
                wisdom += 3;
                break;
            case PlayerClass.Poet:
                wisdom += 3;
                magic += 2;
                constitution += 2;
                dexterity += 1;
                break;
        }
        
        hasChosenClass = true;
        Debug.Log($"🎯 Applied {playerClass} starting stats");
    }
    
    public void AddExperience(long amount)
    {
        if (amount <= 0) return;
        
        currentExperience += amount;
        Debug.Log($"📈 Gained {amount} experience! Total: {currentExperience}");
        
        // Check for level ups
        bool leveledUp = false;
        while (currentLevel < maxLevel && currentExperience >= GetExperienceForLevel(currentLevel + 1))
        {
            LevelUp();
            leveledUp = true;
        }
        
        // Handle post-99 stat points
        if (currentLevel >= maxLevel)
        {
            CheckForStatPoints();
        }
        
        // Update displays
        UpdateExperienceDisplay();
        if (leveledUp)
        {
            UpdateStatsDisplay();
        }
    }
    
    void LevelUp()
    {
        currentLevel++;
        Debug.Log($"🎉 LEVEL UP! Now level {currentLevel}");
        
        // Apply level-up bonuses (NexusTK style)
        ApplyLevelUpStats();
        
        // Heal player on level up
        if (playerHealth != null)
        {
            playerHealth.HealToFull();
        }
        
        // Notify systems
        OnLevelUp?.Invoke(currentLevel);
    }
    
    void ApplyLevelUpStats()
    {
        // Class-based stat gains per level (NexusTK style)
        switch (playerClass)
        {
            case PlayerClass.Warrior:
                if (currentLevel % 2 == 0) strength++;
                if (currentLevel % 3 == 0) constitution++;
                break;
            case PlayerClass.Rogue:
                if (currentLevel % 2 == 0) dexterity++;
                if (currentLevel % 4 == 0) strength++;
                break;
            case PlayerClass.Mage:
                if (currentLevel % 2 == 0) magic++;
                if (currentLevel % 3 == 0) wisdom++;
                break;
            case PlayerClass.Poet:
                if (currentLevel % 3 == 0) wisdom++;
                if (currentLevel % 4 == 0) magic++;
                break;
        }
        
        Debug.Log($"🎯 Level {currentLevel} stat gains applied");
    }
    
    void CheckForStatPoints()
    {
        // Calculate how many stat points we should have earned
        long excessExp = currentExperience - GetExperienceForLevel(maxLevel);
        int earnedStatPoints = (int)(excessExp / experienceForStatPoint);
        
        if (earnedStatPoints > availableStatPoints)
        {
            int newPoints = earnedStatPoints - availableStatPoints;
            availableStatPoints = earnedStatPoints;
            
            Debug.Log($"🌟 Earned {newPoints} stat points! Total available: {availableStatPoints}");
            OnStatPointsChanged?.Invoke(availableStatPoints);
        }
    }
    
    public bool SpendStatPoint(string statName)
    {
        if (availableStatPoints <= 0) return false;
        
        switch (statName.ToLower())
        {
            case "strength":
            case "str":
                strength++;
                break;
            case "magic":
            case "mag":
                magic++;
                break;
            case "constitution":
            case "con":
                constitution++;
                break;
            case "dexterity":
            case "dex":
                dexterity++;
                break;
            case "wisdom":
            case "wis":
                wisdom++;
                break;
            default:
                return false;
        }
        
        availableStatPoints--;
        Debug.Log($"🎯 Spent stat point on {statName}. Remaining: {availableStatPoints}");
        
        OnStatPointsChanged?.Invoke(availableStatPoints);
        OnStatsChanged?.Invoke(GetCurrentStats());
        return true;
    }
    
    long GetExperienceForLevel(int level)
    {
        if (level <= 0 || level > experienceTable.Length) return 0;
        return experienceTable[level - 1];
    }
    
    void UpdateExperienceDisplay()
    {
        long expForNext = currentLevel >= maxLevel ? 
            experienceForStatPoint : 
            GetExperienceForLevel(currentLevel + 1);
        
        OnExperienceChanged?.Invoke(currentLevel, currentExperience, expForNext);
        
        // Update HUD if available
        if (playerHUD != null && playerHUD.xpBar != null)
        {
            if (currentLevel >= maxLevel)
            {
                // Show progress toward next stat point
                long excessExp = currentExperience - GetExperienceForLevel(maxLevel);
                long expIntoStatPoint = excessExp % experienceForStatPoint;
                playerHUD.xpBar.Configure((int)expIntoStatPoint, (int)experienceForStatPoint);
            }
            else
            {
                long expForCurrentLevel = GetExperienceForLevel(currentLevel);
                long expForNextLevel = GetExperienceForLevel(currentLevel + 1);
                long expIntoLevel = currentExperience - expForCurrentLevel;
                long expNeededForLevel = expForNextLevel - expForCurrentLevel;
                
                playerHUD.xpBar.Configure((int)expIntoLevel, (int)expNeededForLevel);
            }
        }
        
        // Update level text
        if (playerHUD != null && playerHUD.levelText != null)
        {
            playerHUD.levelText.text = $"Lv {currentLevel}";
            if (availableStatPoints > 0)
            {
                playerHUD.levelText.text += $" (+{availableStatPoints})";
            }
        }
    }
    
    void UpdateStatsDisplay()
    {
        OnStatsChanged?.Invoke(GetCurrentStats());
    }
    
    public PlayerStats GetCurrentStats()
    {
        return new PlayerStats(strength, magic, constitution, dexterity, wisdom, 
                             currentLevel, currentExperience, availableStatPoints);
    }
    
    // Public getters for combat calculations
    public int GetStrength() => strength;
    public int GetMagic() => magic;
    public int GetConstitution() => constitution;
    public int GetDexterity() => dexterity;
    public int GetWisdom() => wisdom;
    public int GetLevel() => currentLevel;
    public long GetExperience() => currentExperience;
    public int GetStatPoints() => availableStatPoints;
    public PlayerClass GetClass() => playerClass;
    
    // Stat-based calculations (NexusTK style)
    public int GetMaxHealth()
    {
        return 50 + (constitution * 3) + (currentLevel * 2);
    }
    
    public int GetMaxMana()
    {
        return 30 + (magic * 2) + (wisdom * 2) + currentLevel;
    }
    
    public int GetMeleeAttackPower()
    {
        return (strength / 2) + (currentLevel / 3) + 1;
    }
    
    public int GetMagicAttackPower()
    {
        return (magic / 2) + (wisdom / 4) + (currentLevel / 3) + 1;
    }
    
    public float GetAttackSpeed()
    {
        return 1.0f + (dexterity * 0.01f);
    }
    
    // Debug GUI for testing
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 400));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"Level: {currentLevel} ({playerClass})");
        GUILayout.Label($"EXP: {currentExperience:N0}");
        if (currentLevel < maxLevel)
        {
            long needed = GetExperienceForLevel(currentLevel + 1) - currentExperience;
            GUILayout.Label($"To Next: {needed:N0}");
        }
        else
        {
            GUILayout.Label($"Stat Points: {availableStatPoints}");
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Stats:");
        GUILayout.Label($"STR: {strength}");
        GUILayout.Label($"MAG: {magic}");
        GUILayout.Label($"CON: {constitution}");
        GUILayout.Label($"DEX: {dexterity}");
        GUILayout.Label($"WIS: {wisdom}");
        
        GUILayout.Space(5);
        GUILayout.Label("Calculated:");
        GUILayout.Label($"Max HP: {GetMaxHealth()}");
        GUILayout.Label($"Max MP: {GetMaxMana()}");
        GUILayout.Label($"Melee ATK: {GetMeleeAttackPower()}");
        
        GUILayout.Space(5);
        if (GUILayout.Button("Gain 100 EXP"))
        {
            AddExperience(100);
        }
        
        if (availableStatPoints > 0)
        {
            GUILayout.Label("Spend Stat Points:");
            if (GUILayout.Button("STR")) SpendStatPoint("strength");
            if (GUILayout.Button("MAG")) SpendStatPoint("magic");
            if (GUILayout.Button("CON")) SpendStatPoint("constitution");
            if (GUILayout.Button("DEX")) SpendStatPoint("dexterity");
            if (GUILayout.Button("WIS")) SpendStatPoint("wisdom");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}