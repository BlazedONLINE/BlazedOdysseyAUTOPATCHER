using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates a bold, centered player name above the player's head
/// Uses nice font and proper positioning
/// </summary>
public class PlayerNameDisplay : MonoBehaviour
{
    [Header("Name Display Settings")]
    [SerializeField] private string playerName = "Hero";
    [SerializeField] private Vector3 offset = new Vector3(0, 1.8f, 0);
    [SerializeField] private bool autoCreateNameDisplay = true;
    
    [Header("Text Appearance")]
    [SerializeField] private float fontSize = 18f;
    [SerializeField] private Color nameColor = Color.white;
    [SerializeField] private Color outlineColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private float outlineWidth = 2f;
    [SerializeField] private FontStyles fontStyle = FontStyles.Bold;
    
    [Header("Background")]
    [SerializeField] private bool showBackground = true;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.4f);
    [SerializeField] private Vector2 backgroundPadding = new Vector2(8, 4);
    
    private Canvas nameCanvas;
    private TextMeshProUGUI nameText;
    private Transform parentPlayer;
    private Camera playerCamera;
    private Image backgroundImage;
    
    void Start()
    {
        parentPlayer = transform;
        playerCamera = Camera.main;
        
        if (autoCreateNameDisplay)
        {
            CreateNameDisplay();
        }
        
        // Try to get player name from existing sources
        SetPlayerNameFromSources();
    }
    
    void SetPlayerNameFromSources()
    {
        // Try to get name from PlayerHUD
        var playerHUD = Object.FindFirstObjectByType<BlazedOdyssey.UI.PlayerHUD>();
        if (playerHUD != null && playerHUD.nameText != null && !string.IsNullOrEmpty(playerHUD.nameText.text))
        {
            string hudName = playerHUD.nameText.text;
            if (hudName != "PlayerName") // Avoid default placeholder
            {
                SetPlayerName(hudName);
                return;
            }
        }
        
        // Try to get from GameObject name if it's meaningful
        if (!string.IsNullOrEmpty(gameObject.name) && gameObject.name != "Player" && !gameObject.name.Contains("SPUM"))
        {
            SetPlayerName(gameObject.name);
            return;
        }
        
        // Default to "Hero" if no other name found
        SetPlayerName("Hero");
    }
    
    void CreateNameDisplay()
    {
        // Create canvas for name display
        GameObject canvasGO = new GameObject($"NameDisplay_{gameObject.name}");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.zero;
        
        nameCanvas = canvasGO.AddComponent<Canvas>();
        nameCanvas.renderMode = RenderMode.WorldSpace;
        nameCanvas.worldCamera = playerCamera;
        nameCanvas.sortingOrder = 200; // Above everything else
        
        // Scale the canvas appropriately for world space
        RectTransform canvasRect = nameCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 0.5f);
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space
        
        // Create background if enabled
        if (showBackground)
        {
            GameObject backgroundGO = new GameObject("Background");
            backgroundGO.transform.SetParent(canvasGO.transform, false);
            
            backgroundImage = backgroundGO.AddComponent<Image>();
            backgroundImage.color = backgroundColor;
            
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(-backgroundPadding.x, -backgroundPadding.y);
            bgRect.offsetMax = new Vector2(backgroundPadding.x, backgroundPadding.y);
        }
        
        // Create name text
        GameObject textGO = new GameObject("NameText");
        textGO.transform.SetParent(canvasGO.transform, false);
        
        nameText = textGO.AddComponent<TextMeshProUGUI>();
        nameText.text = playerName;
        nameText.fontSize = fontSize;
        nameText.color = nameColor;
        nameText.fontStyle = fontStyle;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.enableAutoSizing = false;
        
        // Add outline for better visibility
        nameText.fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");
        if (nameText.fontMaterial == null)
        {
            // Fallback: add outline component
            var outline = textGO.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(outlineWidth, outlineWidth);
        }
        else
        {
            nameText.outlineColor = outlineColor;
            nameText.outlineWidth = outlineWidth / 100f; // TMP uses 0-1 range
        }
        
        RectTransform textRect = nameText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log($"✅ Created name display for {playerName}");
    }
    
    void Update()
    {
        if (nameCanvas == null || parentPlayer == null) return;
        
        // Update position above player
        nameCanvas.transform.position = parentPlayer.position + offset;
        
        // Face camera
        if (playerCamera != null)
        {
            nameCanvas.transform.LookAt(nameCanvas.transform.position + playerCamera.transform.rotation * Vector3.forward,
                                      playerCamera.transform.rotation * Vector3.up);
        }
    }
    
    public void SetPlayerName(string newName)
    {
        playerName = newName;
        
        if (nameText != null)
        {
            nameText.text = playerName;
            Debug.Log($"🏷️ Updated player name display to: {playerName}");
        }
    }
    
    public void SetNameColor(Color newColor)
    {
        nameColor = newColor;
        
        if (nameText != null)
        {
            nameText.color = nameColor;
        }
    }
    
    public void SetVisible(bool visible)
    {
        if (nameCanvas != null)
        {
            nameCanvas.gameObject.SetActive(visible);
        }
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    // Public methods for different name colors (could indicate player status)
    public void SetNameToFriendly()
    {
        SetNameColor(Color.green);
    }
    
    public void SetNameToNeutral()
    {
        SetNameColor(Color.white);
    }
    
    public void SetNameToHostile()
    {
        SetNameColor(Color.red);
    }
    
    void OnDestroy()
    {
        if (nameCanvas != null)
        {
            Destroy(nameCanvas.gameObject);
        }
    }
}