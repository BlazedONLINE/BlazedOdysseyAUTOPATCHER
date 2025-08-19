using UnityEngine;

/// <summary>
/// Simple manager that ensures the UltimateChatAdapter is set up correctly in the scene.
/// This should be attached to a persistent GameObject in scenes that use the Ultimate Chat Box.
/// </summary>
public class ChatSystemManager : MonoBehaviour
{
    [Header("Chat Settings")]
    [SerializeField] private string localPlayerName = "Player";
    [SerializeField] private bool enableDebugLogs = true;
    
    private UltimateChatAdapter chatAdapter;

    private void Awake()
    {
        // Make this persistent across scene loads
        DontDestroyOnLoad(gameObject);
        
        // Set up the chat adapter
        SetupChatAdapter();
    }

    private void SetupChatAdapter()
    {
        // Check if adapter component already exists on this GameObject
        chatAdapter = GetComponent<UltimateChatAdapter>();
        
        if (chatAdapter == null)
        {
            // Add the adapter component
            chatAdapter = gameObject.AddComponent<UltimateChatAdapter>();
            
            if (enableDebugLogs)
                Debug.Log("ChatSystemManager: Added UltimateChatAdapter component.");
        }
        
        if (enableDebugLogs)
            Debug.Log("ChatSystemManager: Chat system initialized. Press Enter to open chat, Escape to close.");
    }

    /// <summary>
    /// Call this method to send a chat message programmatically
    /// </summary>
    public void SendChatMessage(string sender, string message)
    {
        if (chatAdapter != null)
        {
            chatAdapter.RegisterMessage(sender, message);
        }
    }

    /// <summary>
    /// Get the current player name for chat
    /// </summary>
    public string GetPlayerName()
    {
        return localPlayerName;
    }

    /// <summary>
    /// Set the player name for chat
    /// </summary>
    public void SetPlayerName(string playerName)
    {
        localPlayerName = playerName;
    }
}