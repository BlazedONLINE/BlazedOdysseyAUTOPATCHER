using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    
    [Header("Character Data")]
    public List<CharacterData> playerCharacters = new List<CharacterData>();
    public CharacterData selectedCharacter;
    
    [Header("Development Settings")]
    public bool createMockCharacters = true;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Start()
    {
        LoadPlayerCharacters();
        
        // Create mock characters for development/testing
        if (createMockCharacters && playerCharacters.Count == 0)
        {
            CreateMockCharacters();
        }
    }
    
    public void LoadPlayerCharacters()
    {
        // Get current player email from AuthenticationManager
        AuthenticationManager authManager = FindFirstObjectByType<AuthenticationManager>();
        if (authManager != null && authManager.CurrentUser != null)
        {
            string playerEmail = authManager.CurrentUser.email;
            Debug.Log($"📋 Loading characters for player: {playerEmail}");
            
            // In development, load from PlayerPrefs
            // In production, this would load from server/database
            LoadCharactersFromPlayerPrefs(playerEmail);
        }
        else
        {
            Debug.LogWarning("⚠️ No authenticated user found - using mock data");
        }
    }
    
    void LoadCharactersFromPlayerPrefs(string playerEmail)
    {
        // For development - in production this would be a server call
        string savedData = PlayerPrefs.GetString($"Characters_{playerEmail}", "");
        
        if (!string.IsNullOrEmpty(savedData))
        {
            try
            {
                CharacterSaveData saveData = JsonUtility.FromJson<CharacterSaveData>(savedData);
                playerCharacters = saveData.characters;
                Debug.Log($"✅ Loaded {playerCharacters.Count} characters from save data");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load character data: {e.Message}");
                playerCharacters = new List<CharacterData>();
            }
        }
        else
        {
            Debug.Log("📝 No saved characters found - starting fresh");
            playerCharacters = new List<CharacterData>();
        }
    }
    
    public void SavePlayerCharacters()
    {
        AuthenticationManager authManager = FindFirstObjectByType<AuthenticationManager>();
        if (authManager?.CurrentUser != null)
        {
            string playerEmail = authManager.CurrentUser.email;
            CharacterSaveData saveData = new CharacterSaveData { characters = playerCharacters };
            string jsonData = JsonUtility.ToJson(saveData);
            
            PlayerPrefs.SetString($"Characters_{playerEmail}", jsonData);
            PlayerPrefs.Save();
            
            Debug.Log($"💾 Saved {playerCharacters.Count} characters for {playerEmail}");
        }
    }
    
    public bool HasCharacters()
    {
        return playerCharacters != null && playerCharacters.Count > 0;
    }
    
    public void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
        character.lastPlayed = System.DateTime.Now;
        SavePlayerCharacters();
        
        Debug.Log($"✅ Selected character: {character.characterName} (Level {character.level} {character.characterClass.className})");
    }
    
    public void CreateNewCharacter(CharacterData newCharacter)
    {
        playerCharacters.Add(newCharacter);
        SavePlayerCharacters();
        
        Debug.Log($"🎉 Created new character: {newCharacter.characterName}");
    }
    
    public void DeleteCharacter(CharacterData character)
    {
        playerCharacters.Remove(character);
        SavePlayerCharacters();
        
        Debug.Log($"🗑️ Deleted character: {character.characterName}");
    }
    
    void CreateMockCharacters()
    {
        Debug.Log("🎭 Creating mock characters for development...");
        
        AuthenticationManager authManager = FindFirstObjectByType<AuthenticationManager>();
        string playerEmail = authManager?.CurrentUser?.email ?? "dev@blazed.com";
        
        // Load available character classes
        CharacterClass[] availableClasses = Resources.LoadAll<CharacterClass>("CharacterClasses");
        
        if (availableClasses.Length == 0)
        {
            Debug.LogWarning("⚠️ No character classes found in Resources/CharacterClasses/");
            return;
        }
        
        // Create 2-3 mock characters with different classes and levels
        CreateMockCharacter("Thunderblade", availableClasses[0], playerEmail, 15, "Elite", 3);
        
        if (availableClasses.Length > 1)
        {
            CreateMockCharacter("Mysticwind", availableClasses[1], playerEmail, 8, "Veteran", 2);
        }
        
        if (availableClasses.Length > 2)
        {
            CreateMockCharacter("Shadowstrike", availableClasses[2], playerEmail, 22, "Master", 1);
        }
        
        SavePlayerCharacters();
    }
    
    void CreateMockCharacter(string name, CharacterClass charClass, string email, int level, string rank, int stars)
    {
        CharacterData mockChar = new CharacterData(name, charClass, email);
        
        // Customize the mock character
        mockChar.level = level;
        mockChar.rank = rank;
        mockChar.rankStars = stars;
        mockChar.lastPlayed = System.DateTime.Now.AddHours(-Random.Range(1, 48));
        mockChar.totalPlayTime = Random.Range(5f, 50f);
        
        // Scale stats with level
        float statMultiplier = 1f + (level - 1) * 0.1f;
        mockChar.maxHealth = Mathf.RoundToInt(mockChar.maxHealth * statMultiplier);
        mockChar.health = mockChar.maxHealth;
        mockChar.maxMana = Mathf.RoundToInt(mockChar.maxMana * statMultiplier);
        mockChar.mana = mockChar.maxMana;
        mockChar.attack = Mathf.RoundToInt(mockChar.attack * statMultiplier);
        mockChar.defense = Mathf.RoundToInt(mockChar.defense * statMultiplier);
        mockChar.magic = Mathf.RoundToInt(mockChar.magic * statMultiplier);
        
        mockChar.gold = Random.Range(500, 2000);
        
        playerCharacters.Add(mockChar);
    }
}

[System.Serializable]
public class CharacterSaveData
{
    public List<CharacterData> characters;
}
