using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/// <summary>
/// Character data structure matching database schema
/// </summary>
[System.Serializable]
public class MMOCharacterData
{
    public int id;
    public int accountId;
    public string characterName;
    public string characterClass;
    public int level;
    public int experience;
    public int health;
    public int maxHealth;
    public int mana;
    public int maxMana;
    
    // Position data
    public string sceneName;
    public float positionX;
    public float positionY;
    public float positionZ;
    
    // SPUM Character Appearance Data
    public bool isMale;
    public string bodyType;
    public string skinColor;
    
    // Equipment/Appearance indices for SPUM system
    public int hairIndex;
    public int faceIndex;
    public int eyebrowIndex;
    public int eyesIndex;
    public int noseIndex;
    public int mouthIndex;
    public int beardIndex;
    
    // Equipment indices
    public int helmetIndex;
    public int armorIndex;
    public int pantsIndex;
    public int shoesIndex;
    public int glovesIndex;
    public int weaponIndex;
    public int shieldIndex;
    public int backIndex;
    
    // Character colors (hex values)
    public string hairColor;
    public string eyebrowColor;
    public string eyesColor;
    public string beardColor;
    
    // Equipment colors
    public string helmetColor;
    public string armorColor;
    public string pantsColor;
    public string shoesColor;
    public string glovesColor;
    public string weaponColor;
    public string shieldColor;
    public string backColor;
    
    // Character stats
    public int gold;
    public int statStrength;
    public int statDexterity;
    public int statIntelligence;
    public int statVitality;
    
    // Timestamps
    public DateTime createdAt;
    public DateTime updatedAt;
    public DateTime lastPlayed;
}

/// <summary>
/// Character creation request
/// </summary>
[System.Serializable]
public class MMOCreateCharacterRequest
{
    public string characterName;
    public string characterClass;
    public bool isMale;
    public string skinColor = "#FFDBAC";
    
    // Basic customization
    public int hairIndex = 0;
    public int faceIndex = 0;
    public string hairColor = "#8B4513";
    public string eyesColor = "#4169E1";
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(characterName) && 
               !string.IsNullOrEmpty(characterClass) &&
               characterName.Length >= 2 && characterName.Length <= 50;
    }
}

/// <summary>
/// Character management with database integration
/// </summary>
public class MMOCharacterManager : MonoBehaviour
{
    [Header("Character Settings")]
    [SerializeField] private int maxCharactersPerAccount = 3;
    [SerializeField] private string defaultScene = "StarterMap";
    [SerializeField] private Vector3 defaultSpawnPosition = Vector3.zero;
    
    private static MMOCharacterManager _instance;
    public static MMOCharacterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MMOCharacterManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CharacterManager");
                    _instance = go.AddComponent<MMOCharacterManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // Current selected character
    public MMOCharacterData CurrentCharacter { get; private set; }
    public bool HasSelectedCharacter => CurrentCharacter != null;
    
    // Events
    public event System.Action<MMOCharacterData> OnCharacterSelected;
    public event System.Action<MMOCharacterData> OnCharacterCreated;
    public event System.Action<string> OnCharacterError;
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Create a new character for the current account
    /// </summary>
    public async Task<bool> CreateCharacter(MMOCreateCharacterRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                OnCharacterError?.Invoke("Invalid character data. Check name and class.");
                return false;
            }
            
            var accountManager = AccountManager.Instance;
            if (!accountManager.IsLoggedIn)
            {
                OnCharacterError?.Invoke("You must be logged in to create a character.");
                return false;
            }
            
            Debug.Log($"🎭 Creating character: {request.characterName} ({request.characterClass})");
            
            // Check character name availability
            if (await CheckCharacterNameExists(request.characterName))
            {
                OnCharacterError?.Invoke("Character name already exists. Please choose a different name.");
                return false;
            }
            
            // Check character limit
            var existingCharacters = await GetCharactersForAccount(accountManager.CurrentAccount.id);
            if (existingCharacters.Count >= maxCharactersPerAccount)
            {
                OnCharacterError?.Invoke($"Character limit reached. Maximum {maxCharactersPerAccount} characters per account.");
                return false;
            }
            
            // Create character data
            var characterData = CreateCharacterFromRequest(request, accountManager.CurrentAccount.id);
            
            // Save to database
            bool success = await SaveCharacterToDatabase(characterData);
            
            if (success)
            {
                Debug.Log($"✅ Character '{request.characterName}' created successfully");
                OnCharacterCreated?.Invoke(characterData);
                return true;
            }
            else
            {
                OnCharacterError?.Invoke("Failed to create character. Please try again.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error creating character: {e.Message}");
            OnCharacterError?.Invoke("Character creation failed due to server error.");
            return false;
        }
    }
    
    /// <summary>
    /// Get all characters for the current account
    /// </summary>
    public async Task<List<MMOCharacterData>> GetCharactersForCurrentAccount()
    {
        var accountManager = AccountManager.Instance;
        if (!accountManager.IsLoggedIn)
        {
            return new List<MMOCharacterData>();
        }
        
        return await GetCharactersForAccount(accountManager.CurrentAccount.id);
    }
    
    /// <summary>
    /// Select a character for gameplay
    /// </summary>
    public void SelectCharacter(MMOCharacterData character)
    {
        if (character == null)
        {
            OnCharacterError?.Invoke("Invalid character selection.");
            return;
        }
        
        CurrentCharacter = character;
        OnCharacterSelected?.Invoke(CurrentCharacter);
        
        Debug.Log($"🎮 Selected character: {character.characterName} (Level {character.level} {character.characterClass})");
    }
    
    /// <summary>
    /// Update character data (position, stats, etc.)
    /// </summary>
    public async Task<bool> UpdateCharacter(MMOCharacterData character)
    {
        try
        {
            if (character == null)
            {
                OnCharacterError?.Invoke("Invalid character data for update.");
                return false;
            }
            
            // Update last played timestamp
            character.lastPlayed = DateTime.Now;
            character.updatedAt = DateTime.Now;
            
            bool success = await UpdateCharacterInDatabase(character);
            
            if (success)
            {
                Debug.Log($"✅ Character '{character.characterName}' updated successfully");
                return true;
            }
            else
            {
                OnCharacterError?.Invoke("Failed to update character data.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error updating character: {e.Message}");
            OnCharacterError?.Invoke("Character update failed due to server error.");
            return false;
        }
    }
    
    /// <summary>
    /// Delete a character
    /// </summary>
    public async Task<bool> DeleteCharacter(int characterId)
    {
        try
        {
            bool success = await DeleteCharacterFromDatabase(characterId);
            
            if (success)
            {
                Debug.Log($"🗑️ Character deleted successfully");
                
                // Clear current character if it was deleted
                if (CurrentCharacter != null && CurrentCharacter.id == characterId)
                {
                    CurrentCharacter = null;
                }
                
                return true;
            }
            else
            {
                OnCharacterError?.Invoke("Failed to delete character.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error deleting character: {e.Message}");
            OnCharacterError?.Invoke("Character deletion failed due to server error.");
            return false;
        }
    }
    
    /// <summary>
    /// Create character data from creation request
    /// </summary>
    private MMOCharacterData CreateCharacterFromRequest(MMOCreateCharacterRequest request, int accountId)
    {
        return new MMOCharacterData
        {
            accountId = accountId,
            characterName = request.characterName,
            characterClass = request.characterClass,
            level = 1,
            experience = 0,
            health = 100,
            maxHealth = 100,
            mana = 100,
            maxMana = 100,
            
            // Position
            sceneName = defaultScene,
            positionX = defaultSpawnPosition.x,
            positionY = defaultSpawnPosition.y,
            positionZ = defaultSpawnPosition.z,
            
            // Appearance
            isMale = request.isMale,
            bodyType = "Human",
            skinColor = request.skinColor,
            
            // Basic appearance
            hairIndex = request.hairIndex,
            faceIndex = request.faceIndex,
            eyebrowIndex = 0,
            eyesIndex = 0,
            noseIndex = 0,
            mouthIndex = 0,
            beardIndex = request.isMale ? 0 : -1,
            
            // Equipment (start with basic gear)
            helmetIndex = -1,
            armorIndex = 0,
            pantsIndex = 0,
            shoesIndex = 0,
            glovesIndex = -1,
            weaponIndex = -1,
            shieldIndex = -1,
            backIndex = -1,
            
            // Colors
            hairColor = request.hairColor,
            eyebrowColor = request.hairColor,
            eyesColor = request.eyesColor,
            beardColor = request.hairColor,
            
            // Equipment colors
            helmetColor = "#FFFFFF",
            armorColor = "#FFFFFF",
            pantsColor = "#FFFFFF",
            shoesColor = "#FFFFFF",
            glovesColor = "#FFFFFF",
            weaponColor = "#FFFFFF",
            shieldColor = "#FFFFFF",
            backColor = "#FFFFFF",
            
            // Stats
            gold = 100,
            statStrength = 10,
            statDexterity = 10,
            statIntelligence = 10,
            statVitality = 10,
            
            // Timestamps
            createdAt = DateTime.Now,
            updatedAt = DateTime.Now,
            lastPlayed = DateTime.Now
        };
    }
    
    // Unity-compatible web database methods using UnityWebDatabase
    private async Task<bool> CheckCharacterNameExists(string characterName)
    {
        return await UnityWebDatabase.Instance.CheckCharacterNameExists(characterName);
    }
    
    private async Task<List<MMOCharacterData>> GetCharactersForAccount(int accountId)
    {
        return await UnityWebDatabase.Instance.GetCharactersForAccount(accountId);
    }
    
    private async Task<bool> SaveCharacterToDatabase(MMOCharacterData character)
    {
        int characterId = await UnityWebDatabase.Instance.CreateCharacter(character);
        if (characterId > 0)
        {
            character.id = characterId;
            return true;
        }
        return false;
    }
    
    private async Task<bool> UpdateCharacterInDatabase(MMOCharacterData character)
    {
        return await UnityWebDatabase.Instance.UpdateCharacter(character);
    }
    
    private async Task<bool> DeleteCharacterFromDatabase(int characterId)
    {
        return await UnityWebDatabase.Instance.DeleteCharacter(characterId);
    }
    
    void Start()
    {
        Debug.Log("🎭 MMOCharacterManager initialized");
    }
}