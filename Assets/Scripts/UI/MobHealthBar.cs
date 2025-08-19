using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamic health bar that appears above mobs
/// Shows different colors based on mob aggression state
/// </summary>
public class MobHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Canvas healthCanvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text healthText;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.2f, 0);
    [SerializeField] private float barWidth = 1.0f;
    [SerializeField] private float barHeight = 0.15f;
    [SerializeField] private float hideDelay = 3f; // Hide after 3 seconds of no damage
    
    [Header("Colors")]
    [SerializeField] private Color highHealthColor = new Color(0.85f, 0.20f, 0.20f, 1f); // Bright red
    [SerializeField] private Color lowHealthColor = new Color(0.40f, 0.05f, 0.05f, 1f);  // Dark red
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Header("Text Display")]
    [SerializeField] private bool showHealthNumbers = true;
    [SerializeField] private Color textColor = Color.white;
    
    private Transform parentMob;
    private Camera playerCamera;
    private bool isVisible = false;
    private float lastDamageTime = 0f;
    private bool isAggressive = false;
    
    public void Initialize(Transform mob, bool aggressive = false)
    {
        parentMob = mob;
        isAggressive = aggressive;
        playerCamera = Camera.main;
        
        CreateHealthBar();
        SetVisible(aggressive); // Aggressive mobs show health bar immediately
    }
    
    void CreateHealthBar()
    {
        // Create canvas
        GameObject canvasGO = new GameObject("HealthBar_Canvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.zero;
        
        healthCanvas = canvasGO.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        healthCanvas.worldCamera = playerCamera;
        healthCanvas.sortingOrder = 100; // Above everything
        
        // Scale the canvas
        RectTransform canvasRect = healthCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);
        canvasRect.localScale = Vector3.one;
        
        // Create background
        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(canvasGO.transform);
        
        backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create slider
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(canvasGO.transform);
        
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
        fillAreaGO.transform.SetParent(sliderGO.transform);
        
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Create fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform);
        
        fillImage = fillGO.AddComponent<Image>();
        fillImage.color = highHealthColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        healthSlider.fillRect = fillRect;
        
        // Create health text if enabled
        if (showHealthNumbers)
        {
            GameObject textGO = new GameObject("HealthText");
            textGO.transform.SetParent(canvasGO.transform);
            
            healthText = textGO.AddComponent<Text>();
            healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            healthText.fontSize = 12;
            healthText.color = textColor;
            healthText.alignment = TextAnchor.MiddleCenter;
            healthText.text = "20/20";
            
            RectTransform textRect = healthText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        Debug.Log($"🏥 Created health bar for {parentMob.name} (Aggressive: {isAggressive})");
    }
    
    void Update()
    {
        if (parentMob == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Update position
        transform.position = parentMob.position + offset;
        
        // Face camera
        if (playerCamera != null && healthCanvas != null)
        {
            healthCanvas.transform.LookAt(healthCanvas.transform.position + playerCamera.transform.rotation * Vector3.forward,
                                        playerCamera.transform.rotation * Vector3.up);
        }
        
        // Auto-hide passive health bars after delay
        if (isVisible && !isAggressive && Time.time - lastDamageTime > hideDelay)
        {
            SetVisible(false);
        }
    }
    
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            float healthPercent = (float)currentHealth / (float)maxHealth;
            healthSlider.value = healthPercent;
            
            // Update color based on health percentage (red scheme like PlayerHUD)
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercent);
            }
            
            // Update health text
            if (healthText != null && showHealthNumbers)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
            
            // Show health bar when damaged (for passive mobs)
            if (!isAggressive)
            {
                SetVisible(true);
                lastDamageTime = Time.time;
            }
            
            Debug.Log($"🏥 Updated health bar: {currentHealth}/{maxHealth} ({healthPercent:P0}) - Color: {(fillImage != null ? fillImage.color.ToString() : "null")}");
        }
    }
    
    public void SetAggressive(bool aggressive)
    {
        isAggressive = aggressive;
        
        // Color is now handled in UpdateHealth based on percentage
        
        // Aggressive mobs always show health bar
        if (isAggressive)
        {
            SetVisible(true);
        }
        
        Debug.Log($"🏥 Set {parentMob.name} aggression to {aggressive}");
    }
    
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        
        if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(visible);
        }
    }
    
    public void OnMobDeath()
    {
        // Hide health bar on death, but don't destroy immediately
        SetVisible(false);
        
        // Destroy after a brief delay
        Destroy(gameObject, 0.5f);
    }
}