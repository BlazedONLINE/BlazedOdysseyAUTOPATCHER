using UnityEngine;

/// <summary>
/// Integration layer between existing character system and new MMO database system
/// This bridges the gap between BlazedCharacterSelector and MMOCharacterManager
/// </summary>
public class MMOIntegration : MonoBehaviour
{
    [Header("Integration Settings")]
    [SerializeField] private bool enableMMODatabase = true;
    [SerializeField] private bool debugMode = true;
    
    private MMOCharacterManager mmoCharacterManager;
    private AccountManager accountManager;
    
    void Start()
    {
        if (!enableMMODatabase)
        {
            Debug.Log("🔄 MMO database integration is disabled");
            return;
        }
        
        InitializeMMOManagers();
        SetupEventListeners();
        
        Debug.Log("🔗 MMO Integration initialized");
    }
    
    void InitializeMMOManagers()
    {
        // Get or create MMO managers
        mmoCharacterManager = MMOCharacterManager.Instance;
        accountManager = AccountManager.Instance;
        
        if (debugMode)
        {
            Debug.Log($"🔗 MMO Managers initialized - Account: {accountManager.IsLoggedIn}, Character: {mmoCharacterManager.HasSelectedCharacter}");
        }
    }
    
    void SetupEventListeners()
    {
        if (mmoCharacterManager != null)
        {
            mmoCharacterManager.OnCharacterSelected += OnMMOCharacterSelected;
            mmoCharacterManager.OnCharacterCreated += OnMMOCharacterCreated;
            mmoCharacterManager.OnCharacterError += OnMMOCharacterError;
        }
        
        if (accountManager != null)
        {
            accountManager.OnAccountLogin += OnMMOAccountLogin;
            accountManager.OnAccountLogout += OnMMOAccountLogout;
            accountManager.OnAccountError += OnMMOAccountError;
        }
    }
    
    /// <summary>
    /// Convert existing SelectedCharacter data to MMO character and save to database
    /// Call this when player confirms character selection in BlazedCharacterSelector
    /// </summary>
    public async void SaveCurrentSelectionToMMO()
    {
        if (!enableMMODatabase || !accountManager.IsLoggedIn)
        {
            if (debugMode) Debug.Log("⏭️ Skipping MMO save - not logged in or disabled");
            return;
        }
        
        // Convert SelectedCharacter to MMO format
        var mmoRequest = new MMOCreateCharacterRequest
        {
            characterName = !string.IsNullOrEmpty(SelectedCharacter.CharacterName) ? SelectedCharacter.CharacterName : "Unnamed",
            characterClass = !string.IsNullOrEmpty(SelectedCharacter.ClassName) ? SelectedCharacter.ClassName : "Vanguard Knight",
            isMale = SelectedCharacter.IsMale,
            skinColor = "#FFDBAC",
            hairIndex = 0,
            faceIndex = 0,
            hairColor = "#8B4513",
            eyesColor = "#4169E1"
        };
        
        if (debugMode)
        {
            Debug.Log($"💾 Saving character to MMO database: {mmoRequest.characterName} ({mmoRequest.characterClass})");
        }
        
        bool success = await mmoCharacterManager.CreateCharacter(mmoRequest);
        
        if (success && debugMode)
        {
            Debug.Log("✅ Character saved to MMO database successfully");
        }
    }
    
    /// <summary>
    /// Load character list from MMO database for character selection UI
    /// </summary>
    public async void LoadCharactersFromMMO()
    {
        if (!enableMMODatabase || !accountManager.IsLoggedIn)
        {
            if (debugMode) Debug.Log("⏭️ Skipping MMO load - not logged in or disabled");
            return;
        }
        
        var characters = await mmoCharacterManager.GetCharactersForCurrentAccount();
        
        if (debugMode)
        {
            Debug.Log($"📥 Loaded {characters.Count} characters from MMO database");
        }
        
        // You can extend this to populate the character selection UI
        // For now, just log the character names
        foreach (var character in characters)
        {
            if (debugMode)
            {
                Debug.Log($"   • {character.characterName} (Level {character.level} {character.characterClass})");
            }
        }
    }
    
    // Event handlers for MMO system
    void OnMMOCharacterSelected(MMOCharacterData character)
    {
        if (debugMode)
        {
            Debug.Log($"🎮 MMO Character selected: {character.characterName}");
        }
        
        // Update SelectedCharacter for compatibility with existing systems
        SelectedCharacter.CharacterName = character.characterName;
        SelectedCharacter.ClassName = character.characterClass;
        SelectedCharacter.IsMale = character.isMale;
        SelectedCharacter.Race = character.bodyType;
    }
    
    void OnMMOCharacterCreated(MMOCharacterData character)
    {
        if (debugMode)
        {
            Debug.Log($"🎭 MMO Character created: {character.characterName}");
        }
    }
    
    void OnMMOCharacterError(string error)
    {
        Debug.LogWarning($"❌ MMO Character error: {error}");
    }
    
    void OnMMOAccountLogin(MMOAccountData account)
    {
        if (debugMode)
        {
            Debug.Log($"🔐 MMO Account login: {account.username}");
        }
        
        // Auto-load characters when logging in
        LoadCharactersFromMMO();
    }
    
    void OnMMOAccountLogout()
    {
        if (debugMode)
        {
            Debug.Log("🚪 MMO Account logout");
        }
    }
    
    void OnMMOAccountError(string error)
    {
        Debug.LogWarning($"❌ MMO Account error: {error}");
    }
    
    void OnDestroy()
    {
        // Clean up event listeners
        if (mmoCharacterManager != null)
        {
            mmoCharacterManager.OnCharacterSelected -= OnMMOCharacterSelected;
            mmoCharacterManager.OnCharacterCreated -= OnMMOCharacterCreated;
            mmoCharacterManager.OnCharacterError -= OnMMOCharacterError;
        }
        
        if (accountManager != null)
        {
            accountManager.OnAccountLogin -= OnMMOAccountLogin;
            accountManager.OnAccountLogout -= OnMMOAccountLogout;
            accountManager.OnAccountError -= OnMMOAccountError;
        }
    }
    
    // Public helper methods for UI integration
    public bool IsMMOEnabled() => enableMMODatabase;
    public bool IsLoggedIn() => accountManager != null && accountManager.IsLoggedIn;
    public bool HasSelectedCharacter() => mmoCharacterManager != null && mmoCharacterManager.HasSelectedCharacter;
    public MMOAccountData GetCurrentAccount() => accountManager?.CurrentAccount;
    public MMOCharacterData GetCurrentCharacter() => mmoCharacterManager?.CurrentCharacter;
}