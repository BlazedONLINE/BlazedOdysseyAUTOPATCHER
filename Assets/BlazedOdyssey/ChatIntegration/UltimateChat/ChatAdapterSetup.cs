using UnityEngine;

/// <summary>
/// Setup helper to automatically create and configure the UltimateChatAdapter GameObject
/// in scenes that contain the Ultimate Chat Box. Run this in the editor or attach to any GameObject
/// in your scene to automatically set up chat input handling.
/// </summary>
public class ChatAdapterSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnAwake = true;
    [SerializeField] private string playerName = "Player";
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (autoSetupOnAwake)
        {
            SetupChatAdapter();
        }
    }

    [ContextMenu("Setup Chat Adapter")]
    public void SetupChatAdapter()
    {
        // Check if adapter already exists
        UltimateChatAdapter existingAdapter = Object.FindFirstObjectByType<UltimateChatAdapter>();
        if (existingAdapter != null)
        {
            Debug.Log("UltimateChatAdapter already exists in scene. Configuration complete.");
            return;
        }

        // Create new GameObject for the adapter
        GameObject adapterGO = new GameObject("ChatAdapter");
        
        // Add the adapter component
        UltimateChatAdapter adapter = adapterGO.AddComponent<UltimateChatAdapter>();
        
        // Configure the adapter using reflection to set private fields
        var localPlayerNameField = typeof(UltimateChatAdapter).GetField("localPlayerName", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var showDebugLogsField = typeof(UltimateChatAdapter).GetField("showDebugLogs", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (localPlayerNameField != null)
            localPlayerNameField.SetValue(adapter, playerName);
            
        if (showDebugLogsField != null)
            showDebugLogsField.SetValue(adapter, showDebugLogs);

        Debug.Log($"Created UltimateChatAdapter GameObject. Chat input should now work with Enter/Escape keys.");
        
        // Optionally destroy this setup component after successful setup
        if (Application.isPlaying)
        {
            Destroy(this);
        }
    }
}