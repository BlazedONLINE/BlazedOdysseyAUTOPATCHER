using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using BlazedOdyssey.UI;

/// <summary>
/// Ensures proper setup for the Ultimate Chat Box system including EventSystem
/// This script should be added to a GameObject in your main game scene
/// Only activates chat in gameplay scenes (maps/dungeons), not in character selection
/// </summary>
public class ChatSystemBootstrap : MonoBehaviour
{
    [Header("Chat System Setup")]
    [SerializeField] private bool autoSetupOnAwake = true;
    [SerializeField] private bool createEventSystemIfMissing = true;
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool onlyShowInGameplayScenes = true;
    
    private bool _isInitialized = false;
    
    private void Awake()
    {
        // Check if we should show chat in this scene
        if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowChat())
        {
            if (enableDebugLogs)
                Debug.Log($"[ChatSystemBootstrap] Skipping chat setup - not in gameplay scene (Current: {SceneStateDetector.GetSceneTypeDisplayName()})");
            gameObject.SetActive(false);
            return;
        }

        if (autoSetupOnAwake)
        {
            SetupChatSystem();
        }
    }

    private void Start()
    {
        // Double-check scene state after all Awake calls
        if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowChat())
        {
            if (enableDebugLogs)
                Debug.Log($"[ChatSystemBootstrap] Disabling chat - scene changed or not in gameplay scene");
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-evaluate if we should show chat in the new scene
        if (onlyShowInGameplayScenes)
        {
            bool shouldShow = SceneStateDetector.ShouldShowChat();
            gameObject.SetActive(shouldShow);
            
            if (shouldShow && !_isInitialized && autoSetupOnAwake)
            {
                SetupChatSystem();
            }
            
            if (enableDebugLogs)
                Debug.Log($"[ChatSystemBootstrap] Scene loaded: {scene.name}, Chat active: {shouldShow}");
        }
    }
    
    [ContextMenu("Setup Chat System")]
    public void SetupChatSystem()
    {
        // Check if we should activate in this scene
        if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowChat())
        {
            if (enableDebugLogs)
                Debug.Log($"[ChatSystemBootstrap] Chat not needed in {SceneStateDetector.GetSceneTypeDisplayName()} scene");
            return;
        }

        if (_isInitialized)
        {
            if (enableDebugLogs)
                Debug.Log("[ChatSystemBootstrap] Already initialized");
            return;
        }

        if (enableDebugLogs)
            Debug.Log("🚀 ChatSystemBootstrap: Setting up chat system...");
        
        // 1. Ensure EventSystem exists
        EnsureEventSystem();
        
        // 2. Setup UltimateChatAdapter
        SetupChatAdapter();
        
        // 3. Setup SPUM Portrait Capture
        SetupPortraitCapture();
        
        _isInitialized = true;
        
        if (enableDebugLogs)
            Debug.Log("✅ ChatSystemBootstrap: Setup complete!");
    }
    
    private void EnsureEventSystem()
    {
        var existingEventSystem = Object.FindFirstObjectByType<EventSystem>();
        
        if (existingEventSystem == null && createEventSystemIfMissing)
        {
            // Create EventSystem GameObject
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            
            if (enableDebugLogs)
                Debug.Log("✅ Created EventSystem for UI input handling");
        }
        else if (existingEventSystem != null)
        {
            if (enableDebugLogs)
                Debug.Log("✅ EventSystem already exists");
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ No EventSystem found and auto-creation is disabled");
        }
    }
    
    private void SetupChatAdapter()
    {
        // Check if ChatSystemManager already exists
        var existingManager = Object.FindFirstObjectByType<ChatSystemManager>();
        if (existingManager != null)
        {
            if (enableDebugLogs)
                Debug.Log("✅ ChatSystemManager already exists");
            return;
        }
        
        // Check if UltimateChatAdapter already exists
        var existingAdapter = Object.FindFirstObjectByType<UltimateChatAdapter>();
        if (existingAdapter != null)
        {
            if (enableDebugLogs)
                Debug.Log("✅ UltimateChatAdapter already exists");
            return;
        }
        
        // Create ChatSystemManager (which will create the adapter)
        var chatManagerGO = new GameObject("ChatSystemManager");
        var chatManager = chatManagerGO.AddComponent<ChatSystemManager>();
        
        // Ensure the chat manager respects scene changes
        if (onlyShowInGameplayScenes)
        {
            var chatSceneController = chatManagerGO.AddComponent<ChatSceneController>();
            chatSceneController.enableDebugLogs = enableDebugLogs;
        }
        
        if (enableDebugLogs)
            Debug.Log("✅ Created ChatSystemManager with UltimateChatAdapter");
    }
    
    private void SetupPortraitCapture()
    {
        // Check if SPUMPortraitCapture already exists
        var existingCapture = Object.FindFirstObjectByType<SPUMPortraitCapture>();
        if (existingCapture != null)
        {
            if (enableDebugLogs)
                Debug.Log("✅ SPUMPortraitCapture already exists");
            return;
        }
        
        // Add SPUMPortraitCapture to this GameObject
        gameObject.AddComponent<SPUMPortraitCapture>();
        
        if (enableDebugLogs)
            Debug.Log("✅ Added SPUMPortraitCapture for live HUD portraits");
    }
    
    /// <summary>
    /// Test the chat system by attempting to send a message
    /// </summary>
    [ContextMenu("Test Chat System")]
    public void TestChatSystem()
    {
        var chatManager = Object.FindFirstObjectByType<ChatSystemManager>();
        if (chatManager != null)
        {
            chatManager.SendChatMessage("System", "Chat system test - if you see this, the chat is working!");
            Debug.Log("📬 Sent test message to chat");
        }
        else
        {
            Debug.LogWarning("⚠️ ChatSystemManager not found - setup may be incomplete");
        }
    }
}

/// <summary>
/// Helper component to control chat visibility based on scene changes
/// </summary>
public class ChatSceneController : MonoBehaviour
{
    public bool enableDebugLogs = true;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldShow = SceneStateDetector.ShouldShowChat();
        gameObject.SetActive(shouldShow);
        
        if (enableDebugLogs)
            Debug.Log($"[ChatSceneController] Scene: {scene.name}, Chat active: {shouldShow}");
    }
}