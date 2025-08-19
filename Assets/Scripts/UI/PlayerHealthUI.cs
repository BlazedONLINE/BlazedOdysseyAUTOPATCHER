using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple player health UI that automatically creates and updates health bar
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private bool autoCreateUI = true;
    [SerializeField] private Vector2 healthBarSize = new Vector2(200, 20);
    [SerializeField] private Vector2 healthBarPosition = new Vector2(20, -30);
    
    [Header("Colors")]
    [SerializeField] private Color healthFillColor = Color.green;
    [SerializeField] private Color healthBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.25f;
    
    [Header("Text")]
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private Color textColor = Color.white;
    
    private Canvas healthCanvas;
    private Slider healthSlider;
    private Image fillImage;
    private TextMeshProUGUI healthText;
    private PlayerHealth playerHealth;
    
    void Start()
    {
        if (autoCreateUI)
        {
            FindPlayerHealth();
            CreateHealthBarUI();
        }
    }
    
    void FindPlayerHealth()
    {
        // Look for PlayerHealth on the same object first
        playerHealth = GetComponent<PlayerHealth>();
        
        if (playerHealth == null)
        {
            // Look for PlayerHealth on player tagged objects
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
                
                if (playerHealth == null)
                {
                    // Add PlayerHealth if it doesn't exist
                    playerHealth = player.AddComponent<PlayerHealth>();
                    Debug.Log("✅ Added PlayerHealth to player object");
                }
            }
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("❌ No PlayerHealth found! Health UI won't work.");
            return;
        }
        
        // Subscribe to health events
        playerHealth.OnHealthChanged += UpdateHealthDisplay;
        Debug.Log("✅ PlayerHealthUI connected to PlayerHealth");
    }
    
    void CreateHealthBarUI()
    {
        if (playerHealth == null) return;
        
        // Create canvas
        GameObject canvasGO = new GameObject("PlayerHealthCanvas");
        canvasGO.transform.SetParent(transform);
        
        healthCanvas = canvasGO.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        healthCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create health bar container
        GameObject healthBarGO = new GameObject("HealthBar");
        healthBarGO.transform.SetParent(canvasGO.transform, false);
        
        RectTransform healthBarRect = healthBarGO.AddComponent<RectTransform>();
        healthBarRect.anchorMin = new Vector2(0, 1);
        healthBarRect.anchorMax = new Vector2(0, 1);
        healthBarRect.anchoredPosition = healthBarPosition;
        healthBarRect.sizeDelta = healthBarSize;
        
        // Create background
        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(healthBarGO.transform, false);
        
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = healthBackgroundColor;
        
        RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create health slider
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(healthBarGO.transform, false);
        
        healthSlider = sliderGO.AddComponent<Slider>();
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;
        
        RectTransform sliderRect = healthSlider.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        // Create fill area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Create fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        
        fillImage = fillGO.AddComponent<Image>();
        fillImage.color = healthFillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        healthSlider.fillRect = fillRect;
        
        // Create health text
        if (showHealthText)
        {
            GameObject textGO = new GameObject("HealthText");
            textGO.transform.SetParent(healthBarGO.transform, false);
            
            healthText = textGO.AddComponent<TextMeshProUGUI>();
            healthText.text = $"{playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}";
            healthText.fontSize = 14;
            healthText.color = textColor;
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.fontStyle = FontStyles.Bold;
            
            RectTransform textRect = healthText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        Debug.Log("✅ Player health bar UI created");
        
        // Initial display update
        UpdateHealthDisplay(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
    }
    
    void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (healthSlider == null) return;
        
        float healthPercentage = (float)currentHealth / (float)maxHealth;
        healthSlider.value = healthPercentage;
        
        // Update fill color based on health percentage
        if (fillImage != null)
        {
            fillImage.color = healthPercentage <= lowHealthThreshold ? lowHealthColor : healthFillColor;
        }
        
        // Update text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        
        Debug.Log($"🏥 Health UI updated: {currentHealth}/{maxHealth} ({healthPercentage:P0})");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }
    }
    
    // Public methods for manual control
    public void SetVisible(bool visible)
    {
        if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(visible);
        }
    }
    
    public void UpdatePosition(Vector2 newPosition)
    {
        healthBarPosition = newPosition;
        if (healthCanvas != null)
        {
            RectTransform healthBarRect = healthCanvas.transform.GetChild(0).GetComponent<RectTransform>();
            if (healthBarRect != null)
            {
                healthBarRect.anchoredPosition = healthBarPosition;
            }
        }
    }
}