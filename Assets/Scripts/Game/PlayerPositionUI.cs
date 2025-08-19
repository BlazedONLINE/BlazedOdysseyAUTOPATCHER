using UnityEngine;

/// <summary>
/// Displays the player's current position on screen for debugging and development.
/// Shows coordinates in the center-north area of the screen.
/// </summary>
public class PlayerPositionUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private bool showPosition = true;
    [SerializeField] private Vector2 uiPosition = new Vector2(0.5f, 0.9f); // Center-north of screen
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 16;
    
    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.1f; // Update 10 times per second
    
    private Transform playerTransform;
    private string positionText = "";
    private float lastUpdateTime;
    private GUIStyle textStyle;
    
    void Start()
    {
        FindPlayer();
        SetupGUIStyle();
        
        if (playerTransform == null)
        {
            Debug.LogWarning("⚠️ PlayerPositionUI: No player found with 'Player' tag");
        }
    }
    
    void FindPlayer()
    {
        // Try to find player by tag first
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"✅ PlayerPositionUI: Found player by tag: {player.name}");
            return;
        }
        
        // Try to find by name
        player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"✅ PlayerPositionUI: Found player by name: {player.name}");
            return;
        }
        
        // Try to find SPUM character
        var spumPrefabs = Object.FindFirstObjectByType<SPUM_Prefabs>();
        if (spumPrefabs != null)
        {
            playerTransform = spumPrefabs.transform;
            Debug.Log($"✅ PlayerPositionUI: Found SPUM player: {spumPrefabs.name}");
            return;
        }
        
        Debug.LogWarning("⚠️ PlayerPositionUI: No player found!");
    }
    
    void SetupGUIStyle()
    {
        textStyle = new GUIStyle();
        textStyle.fontSize = fontSize;
        textStyle.normal.textColor = textColor;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontStyle = FontStyle.Bold;
    }
    
    void Update()
    {
        if (!showPosition || playerTransform == null) return;
        
        // Update position text periodically
        if (Time.time - lastUpdateTime > updateInterval)
        {
            Vector3 pos = playerTransform.position;
            positionText = $"Position: X {pos.x:F6} Y {pos.y:F6} Z {pos.z:F6}";
            lastUpdateTime = Time.time;
        }
        
        // Check if player reference is lost and try to find it again
        if (playerTransform == null)
        {
            FindPlayer();
        }
    }
    
    void OnGUI()
    {
        if (!showPosition || string.IsNullOrEmpty(positionText)) return;
        
        if (textStyle == null) SetupGUIStyle();
        
        // Calculate screen position
        float screenX = Screen.width * uiPosition.x;
        float screenY = Screen.height * (1f - uiPosition.y); // Flip Y since GUI uses top-left origin
        
        // Create content and calculate size
        GUIContent content = new GUIContent(positionText);
        Vector2 textSize = textStyle.CalcSize(content);
        
        // Draw background box
        Rect backgroundRect = new Rect(
            screenX - textSize.x * 0.5f - 10,
            screenY - textSize.y * 0.5f - 5,
            textSize.x + 20,
            textSize.y + 10
        );
        
        GUI.Box(backgroundRect, "", GUI.skin.box);
        
        // Draw text
        Rect textRect = new Rect(
            screenX - textSize.x * 0.5f,
            screenY - textSize.y * 0.5f,
            textSize.x,
            textSize.y
        );
        
        GUI.Label(textRect, content, textStyle);
    }
    
    /// <summary>
    /// Toggle position display on/off
    /// </summary>
    public void TogglePositionDisplay()
    {
        showPosition = !showPosition;
    }
    
    /// <summary>
    /// Set whether to show position
    /// </summary>
    public void SetPositionDisplay(bool show)
    {
        showPosition = show;
    }
    
    /// <summary>
    /// Get current player position as Vector3
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        return playerTransform != null ? playerTransform.position : Vector3.zero;
    }
    
    /// <summary>
    /// Get current player position as formatted string
    /// </summary>
    public string GetPlayerPositionString()
    {
        return positionText;
    }
}