using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Reflection;

/// <summary>
/// Captures the SPUM character sprite and applies it to the portrait in the HUD
/// This creates a real-time portrait that matches the actual character
/// </summary>
public class SPUMPortraitCapture : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableLivePortrait = false; // ✅ Changed to false for static portrait
    [SerializeField] private bool useStaticPortrait = true; // ✅ Force static PNG instead of animation
    [SerializeField] private float updateInterval = 0.5f; // Update every half second
    [SerializeField] private bool debugMode = true; // Enable debug to troubleshoot portrait issues
    [SerializeField] private bool scaleToFitPortrait = true; // Scale sprite to fit portrait bounds
    
    [Header("Fallback Sprite (if live character not found)")]
    [SerializeField] private Sprite fallbackSprite; // Manually assigned fallback sprite
    
    [Header("Explicit References (optional)")]
    [SerializeField] private Image portraitImage; // Assign your HUD portrait Image here to avoid name lookups
    [SerializeField] private SPUM_Prefabs targetSpum; // If assigned, use this SPUM instance directly
    [SerializeField] private Transform playerRoot; // If assigned, search here for SPUM_Prefabs
    [SerializeField] private string targetSpumCode; // If set, match SPUM_Prefabs by its Code string
    
    private SPUM_Prefabs playerSPUM;
    private SpriteRenderer playerSpriteRenderer;
    private float lastUpdateTime;
    
    private void Start()
    {
        // Find the portrait image in the HUD if not explicitly assigned
        if (portraitImage == null)
            portraitImage = FindPortraitImage();
        
        if (portraitImage == null)
        {
            if (debugMode) Debug.LogWarning("SPUMPortraitCapture: Portrait image not found in HUD");
            return;
        }
        // Help avoid distortion with non-square sprites
        portraitImage.preserveAspect = true;
        
        // Start the update coroutine
        if (enableLivePortrait && !useStaticPortrait)
        {
            StartCoroutine(UpdatePortraitPeriodically());
        }
        else
        {
            // Static mode - wait a moment for character to spawn, then update once
            StartCoroutine(DelayedStaticUpdate());
            
            if (useStaticPortrait && debugMode)
                Debug.Log("SPUMPortraitCapture: Using static portrait mode - single update after delay");
        }
    }
    
    private Image FindPortraitImage()
    {
        if (debugMode) Debug.Log("SPUMPortraitCapture: Searching for portrait image");
        
        // Look for the portrait in the HUD structure
        // BlazedUI_Canvas > UIRoot > Portrait or HUD_Canvas > HUD_Panel > PortraitFrame > PortraitMask > Portrait
        var hudCanvas = GameObject.Find("BlazedUI_Canvas");
        if (hudCanvas == null)
        {
            // Fallback to old canvas name
            hudCanvas = GameObject.Find("HUD_Canvas");
        }
        
        if (hudCanvas == null)
        {
            if (debugMode) Debug.Log("SPUMPortraitCapture: No HUD canvas found (BlazedUI_Canvas or HUD_Canvas)");
            return null;
        }
        
        if (debugMode) Debug.Log($"SPUMPortraitCapture: Found HUD canvas: {hudCanvas.name}");
        
        var portrait = FindInChildren(hudCanvas.transform, "Portrait");
        if (portrait != null)
        {
            var portraitImage = portrait.GetComponent<Image>();
            if (debugMode) Debug.Log($"SPUMPortraitCapture: Found portrait in hierarchy: {portrait.name} (has Image: {portraitImage != null})");
            return portraitImage;
        }
        
        if (debugMode) Debug.Log("SPUMPortraitCapture: No portrait found in HUD hierarchy, searching all Images");
        
        // Fallback: search all Images with "Portrait" in the name
        var allImages = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (debugMode) Debug.Log($"SPUMPortraitCapture: Found {allImages.Length} total Images in scene");
        
        foreach (var img in allImages)
        {
            if (img.name.Contains("Portrait") || img.name.Contains("portrait"))
            {
                if (debugMode) Debug.Log($"SPUMPortraitCapture: Found portrait by name search: {img.name}");
                return img;
            }
        }
        
        if (debugMode) Debug.Log("SPUMPortraitCapture: No portrait image found anywhere");
        return null;
    }
    
    private Transform FindInChildren(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        
        for (int i = 0; i < parent.childCount; i++)
        {
            var result = FindInChildren(parent.GetChild(i), name);
            if (result != null) return result;
        }
        
        return null;
    }
    
    private IEnumerator UpdatePortraitPeriodically()
    {
        while (enableLivePortrait)
        {
            UpdatePortrait();
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    private IEnumerator DelayedStaticUpdate()
    {
        // Wait for character to fully spawn and initialize
        yield return new WaitForSeconds(1f);
        
        // Try a few times to find the character
        for (int attempts = 0; attempts < 5; attempts++)
        {
            UpdatePortrait();
            
            // Check if we successfully found a sprite
            if (portraitImage != null && portraitImage.sprite != null)
            {
                if (debugMode)
                    Debug.Log($"SPUMPortraitCapture: Successfully set static portrait on attempt {attempts + 1}");
                break;
            }
            
            if (debugMode)
                Debug.Log($"SPUMPortraitCapture: Attempt {attempts + 1} - no sprite found, retrying...");
            
            yield return new WaitForSeconds(0.5f);
        }
        
        // If still no sprite, try fallback
        if (portraitImage != null && portraitImage.sprite == null && fallbackSprite != null)
        {
            portraitImage.sprite = fallbackSprite;
            if (debugMode)
                Debug.Log("SPUMPortraitCapture: Used fallback sprite after all attempts failed");
        }
    }
    
    private void UpdatePortrait()
    {
        if (portraitImage == null) return;
        
        Sprite newSprite = null;
        
        // Method 1: Try to get sprite from live SPUM character
        newSprite = GetLiveSPUMSprite();
        
        // Method 2: Fallback to manually assigned sprite if live sprite not found
        if (newSprite == null && fallbackSprite != null)
        {
            newSprite = fallbackSprite;
            if (debugMode)
                Debug.Log("SPUMPortraitCapture: Using fallback sprite");
        }
        
        // Update the portrait if we found a sprite
        if (newSprite != null)
        {
            portraitImage.sprite = newSprite;
            
            // Scale the sprite to fit the portrait if enabled
            if (scaleToFitPortrait)
            {
                portraitImage.preserveAspect = true;
                portraitImage.type = Image.Type.Simple;
            }
            
            if (debugMode) 
                Debug.Log($"SPUMPortraitCapture: Updated portrait with sprite: {newSprite.name}");
        }
        else if (debugMode)
        {
            Debug.LogWarning("SPUMPortraitCapture: No sprite found via live SPUM or fallback sprite");
        }
    }
    
    private void FindPlayerSPUM()
    {
        if (debugMode) Debug.Log("SPUMPortraitCapture: Starting FindPlayerSPUM()");

        // 1) Direct explicit target
        if (targetSpum != null)
        {
            playerSPUM = targetSpum;
            playerSpriteRenderer = playerSPUM.GetComponentInChildren<SpriteRenderer>();
            if (debugMode) Debug.Log("SPUMPortraitCapture: Using explicit targetSpum");
            return;
        }

        // 2) Search within provided player root
        if (playerRoot != null)
        {
            playerSPUM = playerRoot.GetComponentInChildren<SPUM_Prefabs>();
            playerSpriteRenderer = playerRoot.GetComponentInChildren<SpriteRenderer>();
            if (playerSPUM != null) { if (debugMode) Debug.Log("SPUMPortraitCapture: Found SPUM via playerRoot"); return; }
        }

        // 3) Player tag
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (debugMode) Debug.Log($"SPUMPortraitCapture: Found player GameObject: {player.name}");

            playerSPUM = player.GetComponent<SPUM_Prefabs>() ?? player.GetComponentInChildren<SPUM_Prefabs>();
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>() ?? player.GetComponentInChildren<SpriteRenderer>();

            if (playerSPUM != null)
            {
                if (debugMode) Debug.Log($"SPUMPortraitCapture: Player SPUM_Prefabs found: {playerSPUM.name}");
                return;
            }
        }

        // 4) Match by SPUM Code
        if (!string.IsNullOrWhiteSpace(targetSpumCode))
        {
            var all = Object.FindObjectsByType<SPUM_Prefabs>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var s in all)
            {
                var code = TryGetSpumCode(s);
                if (!string.IsNullOrEmpty(code) && string.Equals(code, targetSpumCode, System.StringComparison.OrdinalIgnoreCase))
                {
                    playerSPUM = s;
                    playerSpriteRenderer = s.GetComponentInChildren<SpriteRenderer>();
                    if (debugMode) Debug.Log($"SPUMPortraitCapture: Found SPUM by Code match: {code}");
                    return;
                }
            }
        }

        // 5) Any SPUM in scene
        if (playerSPUM == null)
        {
            if (debugMode) Debug.Log("SPUMPortraitCapture: Searching for any SPUM_Prefabs in scene");
            playerSPUM = FindFirstObjectByType<SPUM_Prefabs>();
            if (playerSPUM != null)
            {
                playerSpriteRenderer = playerSPUM.GetComponentInChildren<SpriteRenderer>();
                if (debugMode) Debug.Log($"SPUMPortraitCapture: Using first found SPUM: {playerSPUM.name}");
            }
        }
    }
    
    private Sprite GetLiveSPUMSprite()
    {
        if (debugMode) Debug.Log($"SPUMPortraitCapture: Starting GetLiveSPUMSprite() - Static mode: {useStaticPortrait}");
        
        // Find the player SPUM component if not already found
        if (playerSPUM == null)
        {
            FindPlayerSPUM();
        }
        
        if (playerSPUM == null)
        {
            if (debugMode) Debug.Log("SPUMPortraitCapture: No SPUM_Prefabs found, cannot get sprite");
            return null;
        }
        
        if (debugMode) Debug.Log($"SPUMPortraitCapture: Using SPUM_Prefabs on {playerSPUM.name}");
        
        // Method 1: Try to get from SPUM system
        var bodyRenderer = playerSPUM.GetComponentInChildren<SpriteRenderer>();
        if (bodyRenderer != null && bodyRenderer.sprite != null)
        {
            if (debugMode) Debug.Log($"SPUMPortraitCapture: Found sprite from SPUM body renderer: {bodyRenderer.sprite.name}");
            return bodyRenderer.sprite;
        }
        
        // Method 2: Get from the main sprite renderer
        if (playerSpriteRenderer != null && playerSpriteRenderer.sprite != null)
        {
            if (debugMode) Debug.Log($"SPUMPortraitCapture: Found sprite from main sprite renderer: {playerSpriteRenderer.sprite.name}");
            return playerSpriteRenderer.sprite;
        }
        
        // Method 3: Try to find any sprite renderer on the player
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (debugMode) Debug.Log("SPUMPortraitCapture: Searching all SpriteRenderers on player");
            
            var renderers = player.GetComponentsInChildren<SpriteRenderer>();
            if (debugMode) Debug.Log($"SPUMPortraitCapture: Found {renderers.Length} renderers to check");
            
            // First pass: Look for body-specific sprites
            foreach (var renderer in renderers)
            {
                if (renderer.sprite != null && renderer.enabled)
                {
                    if (debugMode) Debug.Log($"SPUMPortraitCapture: Checking renderer {renderer.name} with sprite {renderer.sprite.name}");
                    
                    // Prefer sprites that look like body parts
                    if (renderer.name.Contains("Body") || renderer.name.Contains("body") || 
                        renderer.name.Contains("Base") || renderer.name.Contains("Character"))
                    {
                        if (debugMode) Debug.Log($"SPUMPortraitCapture: Found body sprite: {renderer.sprite.name} on {renderer.name}");
                        return renderer.sprite;
                    }
                }
            }
            
            // Second pass: Use any enabled sprite
            foreach (var renderer in renderers)
            {
                if (renderer.sprite != null && renderer.enabled)
                {
                    if (debugMode) Debug.Log($"SPUMPortraitCapture: Using fallback sprite: {renderer.sprite.name} on {renderer.name}");
                    return renderer.sprite;
                }
            }
            
            if (debugMode) Debug.Log("SPUMPortraitCapture: No suitable sprites found in any renderer");
        }
        else
        {
            if (debugMode) Debug.Log("SPUMPortraitCapture: Player GameObject not found for Method 3");
        }
        
        return null;
    }

    private static string TryGetSpumCode(object spum)
    {
        if (spum == null) return null;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var t = spum.GetType();
        var prop = t.GetProperty("Code", flags) ?? t.GetProperty("code", flags);
        if (prop != null)
        {
            var v = prop.GetValue(spum) as string; if (!string.IsNullOrEmpty(v)) return v;
        }
        var fld = t.GetField("Code", flags) ?? t.GetField("code", flags);
        if (fld != null)
        {
            var v = fld.GetValue(spum) as string; if (!string.IsNullOrEmpty(v)) return v;
        }
        return null;
    }
    
    /// <summary>
    /// Force an immediate portrait update
    /// </summary>
    public void ForceUpdatePortrait()
    {
        UpdatePortrait();
    }
    
    /// <summary>
    /// Enable or disable live portrait updates
    /// </summary>
    public void SetLivePortrait(bool enabled)
    {
        enableLivePortrait = enabled && !useStaticPortrait; // Don't enable live if static mode is on
        
        if (enableLivePortrait)
        {
            StartCoroutine(UpdatePortraitPeriodically());
        }
    }
    
    /// <summary>
    /// Enable or disable static portrait mode (PNG instead of animation)
    /// </summary>
    public void SetStaticPortrait(bool enabled)
    {
        useStaticPortrait = enabled;
        
        if (enabled)
        {
            enableLivePortrait = false; // Disable live updates when static mode is on
            StopAllCoroutines(); // Stop any running update coroutines
            UpdatePortrait(); // Do one final static update
            
            if (debugMode)
                Debug.Log("SPUMPortraitCapture: Static portrait mode enabled");
        }
        else
        {
            if (debugMode)
                Debug.Log("SPUMPortraitCapture: Static portrait mode disabled");
        }
    }
    
    /// <summary>
    /// Set fallback sprite manually
    /// </summary>
    public void SetFallbackSprite(Sprite sprite)
    {
        fallbackSprite = sprite;
        
        if (debugMode)
            Debug.Log($"SPUMPortraitCapture: Fallback sprite set to {sprite?.name ?? "null"}");
            
        // Force an update with the new sprite
        UpdatePortrait();
    }
    
    /// <summary>
    /// Get current fallback sprite
    /// </summary>
    public Sprite GetFallbackSprite()
    {
        return fallbackSprite;
    }
}