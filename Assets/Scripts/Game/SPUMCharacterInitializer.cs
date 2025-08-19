using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Initializes SPUM characters using the built-in SPUM system.
/// Works with SPUM_Prefab components and ImageElement data to properly load character sprites.
/// Uses SPUM's official LoadSpriteFromMultiple method for sprite loading.
/// This approach works with the existing SPUM workflow instead of bypassing it.
/// </summary>
public class SPUMCharacterInitializer : MonoBehaviour
{
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    [Header("Character Data")]
    private string currentCharacterClass;
    
    /// <summary>
    /// Initialize SPUM character by loading default sprites for the character type
    /// </summary>
    public void InitializeCharacter(string characterClass = null)
    {
        // Store character class for use in sprite loading
        currentCharacterClass = characterClass;
        
        if (enableDebugLogs)
            Debug.Log($"🎭 Initializing SPUM character: {characterClass ?? "Default"}");
        
        // Get all SPUM matching lists (the sprite mapping components)
        var matchingLists = GetComponentsInChildren<SPUM_MatchingList>(true);
        if (matchingLists.Length == 0)
        {
            Debug.LogWarning("⚠️ No SPUM_MatchingList components found - this might not be a SPUM character");
            return;
        }
        
        // Initialize basic character appearance
        InitializeBasicAppearance(matchingLists);
        
        if (enableDebugLogs)
            Debug.Log($"✅ SPUM character initialized with {matchingLists.Length} matching lists");
    }
    
    /// <summary>
    /// Initialize SPUM character using the built-in SPUM system
    /// </summary>
    private void InitializeBasicAppearance(SPUM_MatchingList[] matchingLists)
    {
        if (enableDebugLogs)
            Debug.Log($"🎭 Initializing SPUM character using built-in SPUM system");
        
        // The SPUM prefab should already have sprite data configured!
        // We just need to trigger the proper SPUM initialization
        
        // Check if this prefab has SPUM data configured
        var spumPrefab = GetComponent<SPUM_Prefabs>();
        if (spumPrefab != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"✅ Found SPUM_Prefabs component!");
                Debug.Log($"   - ImageElement count: {spumPrefab.ImageElement?.Count ?? 0}");
                Debug.Log($"   - UnitType: {spumPrefab.UnitType}");
                Debug.Log($"   - Code: {spumPrefab._code}");
            }
            
            if (spumPrefab.ImageElement != null && spumPrefab.ImageElement.Count > 0)
            {
                if (enableDebugLogs)
                    Debug.Log($"🔧 Found {spumPrefab.ImageElement.Count} image elements - loading sprites directly");
                
                // Load sprites directly without SPUM_Manager
                InitializeSpritesDirectly(spumPrefab, matchingLists);
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"⚠️ SPUM_Prefabs has no ImageElement data - trying fallback");
                TriggerDirectSpriteLoad(matchingLists);
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning($"⚠️ No SPUM_Prefabs component found - trying fallback");
            
            TriggerDirectSpriteLoad(matchingLists);
        }
        
        // Update sorting orders to ensure visibility
        var allMatchingElements = matchingLists
            .SelectMany(ml => ml.matchingTables)
            .Where(me => me.renderer != null)
            .ToList();
        UpdateSortingOrders(allMatchingElements);
    }
    
    /// <summary>
    /// Load sprites directly from Resources without SPUM_Manager
    /// </summary>
    private void InitializeSpritesDirectly(SPUM_Prefabs spumPrefab, SPUM_MatchingList[] matchingLists)
    {
        if (enableDebugLogs)
            Debug.Log($"🔧 Loading sprites directly from ImageElement data");
        
        var allMatchingElements = matchingLists
            .SelectMany(ml => ml.matchingTables)
            .Where(me => me.renderer != null)
            .ToList();
        
        int spritesLoaded = 0;
        
        foreach (var imageElement in spumPrefab.ImageElement)
        {
            if (enableDebugLogs)
                Debug.Log($"🔍 Processing ImageElement: {imageElement.PartType}/{imageElement.Structure} from {imageElement.ItemPath}");
            
            // Find matching renderer
            var matchingElement = allMatchingElements.FirstOrDefault(me => 
                (me.UnitType == imageElement.UnitType) &&
                (me.PartType == imageElement.PartType) &&
                (me.Dir == imageElement.Dir) &&
                (me.Structure == imageElement.Structure) &&
                (me.PartSubType == imageElement.PartSubType));
            
            if (matchingElement != null)
            {
                // Load sprite directly from Resources
                var sprite = LoadSpriteDirectly(imageElement.ItemPath, imageElement.Structure);
                if (sprite != null)
                {
                    matchingElement.renderer.sprite = sprite;
                    matchingElement.renderer.color = imageElement.Color;
                    matchingElement.renderer.maskInteraction = (SpriteMaskInteraction)imageElement.MaskIndex;
                    matchingElement.ItemPath = imageElement.ItemPath;
                    matchingElement.Color = imageElement.Color;
                    matchingElement.MaskIndex = imageElement.MaskIndex;
                    
                    spritesLoaded++;
                    
                    if (enableDebugLogs)
                        Debug.Log($"✅ Loaded sprite: {sprite.name} for {imageElement.PartType}/{imageElement.Structure}");
                }
                else if (enableDebugLogs)
                {
                    Debug.LogWarning($"⚠️ Failed to load sprite from: {imageElement.ItemPath}");
                }
            }
            else if (enableDebugLogs)
            {
                Debug.LogWarning($"⚠️ No matching renderer found for {imageElement.PartType}/{imageElement.Structure}");
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"🎨 Direct sprite loading complete: loaded {spritesLoaded}/{spumPrefab.ImageElement.Count} sprites");
        
        // CRITICAL DEBUGGING: Check character visibility
        DebugCharacterVisibility();
    }
    
    /// <summary>
    /// Load sprite directly from Resources (replicate SPUM's LoadSpriteFromMultiple logic)
    /// </summary>
    private Sprite LoadSpriteDirectly(string itemPath, string structure)
    {
        if (string.IsNullOrEmpty(itemPath))
            return null;
        
        if (enableDebugLogs)
            Debug.Log($"🔍 Loading sprite from path: {itemPath}, structure: {structure}");
        
        // Try to load sprites from the path
        var sprites = Resources.LoadAll<Sprite>(itemPath);
        if (sprites == null || sprites.Length == 0)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"⚠️ No sprites found at Resources path: {itemPath}");
            return null;
        }
        
        if (enableDebugLogs)
            Debug.Log($"📁 Found {sprites.Length} sprites at {itemPath}");
        
        // If multiple sprites, try to find one matching the structure
        if (sprites.Length > 1 && !string.IsNullOrEmpty(structure))
        {
            var specificSprite = System.Array.Find(sprites, s => s.name.Contains(structure));
            if (specificSprite != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"✅ Found specific sprite: {specificSprite.name}");
                return specificSprite;
            }
        }
        
        // Return first sprite as fallback
        if (enableDebugLogs)
            Debug.Log($"📷 Using first sprite: {sprites[0].name}");
        return sprites[0];
    }
    
    /// <summary>
    /// Fallback: try to load any available sprites for the character
    /// </summary>
    private void TriggerDirectSpriteLoad(SPUM_MatchingList[] matchingLists)
    {
        if (enableDebugLogs)
            Debug.Log($"🔄 Triggering direct sprite load fallback");
        
        var allMatchingElements = matchingLists
            .SelectMany(ml => ml.matchingTables)
            .Where(me => me.renderer != null)
            .ToList();
        
        // Load ALL essential SPUM character parts based on SPUM Character Creator system
        LoadBasicSprite(allMatchingElements, "Body", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        LoadBasicSprite(allMatchingElements, "Head", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        
        // Load ALL eye variations to ensure proper eye coverage
        LoadBasicSprite(allMatchingElements, "Eye", "Addons/Legacy/0_Unit/0_Sprite/2_Eye/Eye0", "Eye0");
        LoadBasicSprite(allMatchingElements, "Eyes", "Addons/Legacy/0_Unit/0_Sprite/2_Eye/Eye0", "Eye0");
        LoadBasicSprite(allMatchingElements, "EyeBrow", "Addons/Legacy/0_Unit/0_Sprite/2_Eye/Eye0", "Eye0");
        LoadBasicSprite(allMatchingElements, "Eyebrow", "Addons/Legacy/0_Unit/0_Sprite/2_Eye/Eye0", "Eye0");
        LoadBasicSprite(allMatchingElements, "EyeLash", "Addons/Legacy/0_Unit/0_Sprite/2_Eye/Eye0", "Eye0");
        LoadBasicSprite(allMatchingElements, "Pupil", "Addons/Legacy/0_Unit/0_Sprite/2_Eye/Eye0", "Eye0");
        
        // Load other body parts
        LoadBasicSprite(allMatchingElements, "Hair", "Addons/Legacy/0_Unit/0_Sprite/3_Hair/Hair0", "Hair0");
        LoadBasicSprite(allMatchingElements, "Face", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        LoadBasicSprite(allMatchingElements, "Nose", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        LoadBasicSprite(allMatchingElements, "Mouth", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        LoadBasicSprite(allMatchingElements, "Ear", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        
        // Arms and limbs
        LoadBasicSprite(allMatchingElements, "Arm", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        LoadBasicSprite(allMatchingElements, "Hand", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        LoadBasicSprite(allMatchingElements, "Leg", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        LoadBasicSprite(allMatchingElements, "Foot", "Addons/Legacy/0_Unit/0_Sprite/1_Body/Human_1", "Human_1");
        
        // Clothing and equipment
        LoadBasicSprite(allMatchingElements, "Outfit", "Addons/Legacy/0_Unit/0_Sprite/4_Outfit/Outfit0", "Outfit0");
        LoadBasicSprite(allMatchingElements, "Armor", "Addons/Legacy/0_Unit/0_Sprite/4_Outfit/Outfit0", "Outfit0");
        LoadBasicSprite(allMatchingElements, "Accessory", "Addons/Legacy/0_Unit/0_Sprite/4_Outfit/Outfit0", "Outfit0");
        LoadBasicSprite(allMatchingElements, "Weapon", "Addons/Legacy/0_Unit/0_Sprite/5_Weapon/Weapon0", "Weapon0");
        LoadBasicSprite(allMatchingElements, "Shield", "Addons/Legacy/0_Unit/0_Sprite/5_Weapon/Weapon0", "Weapon0");
        
        if (enableDebugLogs)
            Debug.Log($"✅ Fallback sprite loading complete for all essential parts");
    }
    
    /// <summary>
    /// Helper method to load and apply basic sprites for specific body parts
    /// </summary>
    private void LoadBasicSprite(List<MatchingElement> elements, string partType, string path, string spriteName)
    {
        var sprite = LoadSpriteDirectly(path, spriteName);
        if (sprite != null)
        {
            int appliedCount = 0;
            foreach (var element in elements)
            {
                if (element.Structure.ToLower().Contains(partType.ToLower()) || 
                    element.PartType.ToLower().Contains(partType.ToLower()))
                {
                    element.renderer.sprite = sprite;
                    element.renderer.color = Color.white;
                    
                    // Ensure proper scaling and visibility for eye parts
                    if (partType.ToLower().Contains("eye"))
                    {
                        element.renderer.transform.localScale = Vector3.one;
                        element.renderer.sortingOrder = 10; // Higher sorting order for eyes
                        element.renderer.enabled = true;
                        
                        // Apply proper eye color based on character selection or default
                        Color eyeColor = GetEyeColorForCharacter();
                        element.renderer.color = eyeColor;
                        
                        if (enableDebugLogs)
                            Debug.Log($"👁️ Applied eye sprite with color {eyeColor} to {element.renderer.name}");
                    }
                    
                    appliedCount++;
                    
                    if (enableDebugLogs)
                        Debug.Log($"✅ Applied {partType} sprite to {element.renderer.name} (sorting: {element.renderer.sortingOrder})");
                }
            }
            
            if (enableDebugLogs && appliedCount > 0)
                Debug.Log($"🎨 Successfully applied {partType} sprite to {appliedCount} renderers");
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"⚠️ Failed to load {partType} sprite from: {path}");
        }
    }
    
    /// <summary>
    /// Fallback: Try to trigger default SPUM character setup
    /// </summary>
    private void TriggerDefaultSPUMSetup(SPUM_MatchingList[] matchingLists)
    {
        if (enableDebugLogs)
            Debug.Log($"🔄 Triggering default SPUM setup as fallback");
        
        var spumManager = SoonsoonData.Instance?._spumManager;
        if (spumManager != null)
        {
            // Try to apply default human character setup
            spumManager.SetDefultSet("Unit", "Body", "Human_1", Color.white);
            spumManager.SetDefultSet("Unit", "Eye", "Eye0", new Color32(71, 26, 26, 255));
            
            if (enableDebugLogs)
                Debug.Log($"✅ Applied default SPUM character setup");
        }
    }
    
    /// <summary>
    /// DEPRECATED: Old Franuka sprite loading method - replaced by registry prefab copying
    /// </summary>
    private void LoadFranukaCharacterSprites(List<MatchingElement> matchingElements)
    {
        if (enableDebugLogs)
            Debug.LogWarning($"⚠️ LoadFranukaCharacterSprites is deprecated - using registry prefab copying instead");
    }
    
    // Old debugging methods removed - no longer needed with registry approach
    
    // Character mapping removed - using SPUMUnitRegistry directly now
    
    // Old sprite application methods removed - using direct prefab copying now
    
    /// <summary>
    /// Load sprite from Resources path
    /// </summary>
    private Sprite LoadSpriteFromPath(string path, string spriteName)
    {
        if (enableDebugLogs)
            Debug.Log($"🔍 Loading sprite from: {path}");
        
        var sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"⚠️ No sprites found at path: {path}");
            return null;
        }
        
        if (enableDebugLogs)
            Debug.Log($"📁 Found {sprites.Length} sprites at {path}");
        
        // For multi-sprite assets, try to find the right sub-sprite
        if (sprites.Length > 1)
        {
            // Try to find sprite by exact name first
            var exactMatch = System.Array.Find(sprites, s => s.name == spriteName);
            if (exactMatch != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"✅ Found exact match: {exactMatch.name}");
                return exactMatch;
            }
            
            // Try partial name match
            var partialMatch = System.Array.Find(sprites, s => s.name.ToLower().Contains(spriteName.ToLower()));
            if (partialMatch != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"✅ Found partial match: {partialMatch.name}");
                return partialMatch;
            }
        }
        
        // Return first sprite if no specific match found
        if (enableDebugLogs)
            Debug.Log($"📦 Using first sprite: {sprites[0].name}");
        return sprites[0];
    }
    
    /// <summary>
    /// Update sorting orders to ensure character is visible
    /// </summary>
    private void UpdateSortingOrders(List<MatchingElement> matchingElements)
    {
        int baseSortingOrder = 100;
        
        foreach (var element in matchingElements)
        {
            if (element.renderer != null)
            {
                element.renderer.sortingLayerName = "Default";
                element.renderer.sortingOrder = baseSortingOrder++;
            }
        }
    }
    
    /// <summary>
    /// Copy sprite data from another SPUM character (useful for character selection preview)
    /// </summary>
    public void CopyFromSPUMCharacter(GameObject sourceCharacter)
    {
        if (sourceCharacter == null) return;
        
        var sourceMatchingLists = sourceCharacter.GetComponentsInChildren<SPUM_MatchingList>(true);
        var targetMatchingLists = GetComponentsInChildren<SPUM_MatchingList>(true);
        
        if (sourceMatchingLists.Length == 0 || targetMatchingLists.Length == 0)
        {
            Debug.LogWarning("⚠️ Source or target character missing SPUM_MatchingList components");
            return;
        }
        
        // Copy sprite data from source to target
        var sourceElements = sourceMatchingLists.SelectMany(ml => ml.matchingTables).ToList();
        var targetElements = targetMatchingLists.SelectMany(ml => ml.matchingTables).ToList();
        
        foreach (var targetElement in targetElements)
        {
            if (targetElement.renderer == null) continue;
            
            // Find matching source element
            var sourceElement = sourceElements.FirstOrDefault(se => 
                se.renderer != null && 
                se.Structure == targetElement.Structure);
            
            if (sourceElement != null && sourceElement.renderer.sprite != null)
            {
                targetElement.renderer.sprite = sourceElement.renderer.sprite;
                targetElement.renderer.color = sourceElement.renderer.color;
                targetElement.ItemPath = sourceElement.ItemPath;
                targetElement.Color = sourceElement.Color;
                targetElement.MaskIndex = sourceElement.MaskIndex;
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"✅ Copied SPUM sprite data from {sourceCharacter.name}");
    }
    
    void Start()
    {
        // Auto-initialize if no other initialization happens
        Invoke(nameof(AutoInitialize), 0.1f);
    }
    
    private void AutoInitialize()
    {
        // Check if character already has sprites loaded
        var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        bool hasSprites = renderers.Any(r => r.sprite != null);
        
        if (!hasSprites)
        {
            if (enableDebugLogs)
                Debug.Log("🔄 Auto-initializing SPUM character...");
            InitializeCharacter();
        }
    }
    
    
    /// <summary>
    /// Find or create SpumUnitRegistry (same approach as character selection)
    /// </summary>
    private SpumUnitRegistry FindSpumUnitRegistry()
    {
        // First, try to find existing registry in scene
        var existingRegistry = FindFirstObjectByType<SpumUnitRegistry>();
        if (existingRegistry != null)
        {
            if (enableDebugLogs)
                Debug.Log($"✅ Found existing SpumUnitRegistry in scene");
            return existingRegistry;
        }
        
        // If not found, try to load from project assets using AssetDatabase
        #if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpumUnitRegistry");
        if (guids.Length > 0)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            var projectRegistry = UnityEditor.AssetDatabase.LoadAssetAtPath<SpumUnitRegistry>(assetPath);
            if (projectRegistry != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"📁 Loaded SpumUnitRegistry from project: {assetPath}");
                return projectRegistry;
            }
        }
        #endif
        
        // Try to load from Resources (if asset was placed there)
        var resourceRegistry = Resources.Load<SpumUnitRegistry>("SpumUnitRegistry");
        if (resourceRegistry != null)
        {
            if (enableDebugLogs)
                Debug.Log($"📁 Loaded SpumUnitRegistry from Resources");
            return resourceRegistry;
        }
        
        // Check if there's a registry asset file in the project
        var spumRegAsset = Resources.Load("SPUM/Config/SpumUnitRegistry");
        if (spumRegAsset != null)
        {
            if (enableDebugLogs)
                Debug.Log($"📄 Found registry asset but wrong type");
        }
        
        // Last resort: Try to create one if we're in character selection scene
        var characterSelector = FindFirstObjectByType<BlazedCharacterSelector>();
        if (characterSelector != null && characterSelector.spumRegistry != null)
        {
            if (enableDebugLogs)
                Debug.Log($"📋 Using SpumUnitRegistry from BlazedCharacterSelector");
            return characterSelector.spumRegistry;
        }
        
        if (enableDebugLogs)
        {
            Debug.LogError($"❌ Could not find SpumUnitRegistry anywhere!");
            Debug.LogError($"   - Not in scene as component");
            Debug.LogError($"   - Not in project assets");
            Debug.LogError($"   - Not in Resources folder");
            Debug.LogError($"   - BlazedCharacterSelector not found or has no registry");
            Debug.LogError($"   - You may need to assign SpumUnitRegistry to the character selector manually");
        }
        
        return null;
    }
    
    /// <summary>
    /// Get appropriate eye color based on character class and gender
    /// </summary>
    private Color GetEyeColorForCharacter()
    {
        // Default eye colors by character type and gender
        var eyeColors = new System.Collections.Generic.Dictionary<string, Color>
        {
            {"warrior_male", new Color32(139, 69, 19, 255)}, // Brown eyes
            {"warrior_female", new Color32(34, 139, 34, 255)}, // Green eyes
            {"mage_male", new Color32(0, 100, 200, 255)}, // Blue eyes
            {"mage_female", new Color32(138, 43, 226, 255)}, // Purple eyes
            {"rogue_male", new Color32(105, 105, 105, 255)}, // Gray eyes
            {"rogue_female", new Color32(220, 20, 60, 255)}, // Crimson eyes
            {"knight_male", new Color32(70, 130, 180, 255)}, // Steel blue
            {"knight_female", new Color32(72, 209, 204, 255)}, // Turquoise
            {"vanguard_male", new Color32(184, 134, 11, 255)}, // Dark golden
            {"vanguard_female", new Color32(50, 205, 50, 255)} // Lime green
        };
        
        // Get current character info
        string className = currentCharacterClass?.ToLower() ?? "warrior";
        bool isMale = SelectedCharacter.IsMale;
        string key = $"{className}_{(isMale ? "male" : "female")}";
        
        if (eyeColors.TryGetValue(key, out Color specificColor))
        {
            if (enableDebugLogs)
                Debug.Log($"👁️ Using specific eye color for {key}: {specificColor}");
            return specificColor;
        }
        
        // Fallback colors
        Color fallbackColor = isMale ? new Color32(71, 26, 26, 255) : new Color32(34, 139, 34, 255);
        
        if (enableDebugLogs)
            Debug.Log($"👁️ Using fallback eye color for {key}: {fallbackColor}");
        
        return fallbackColor;
    }
    
    /// <summary>
    /// Debug character visibility issues - find out why character appears as blue box
    /// </summary>
    private void DebugCharacterVisibility()
    {
        if (!enableDebugLogs) return;
        
        Debug.Log("🔍 === DEBUGGING CHARACTER VISIBILITY ===");
        
        // Check transform
        var transform = this.transform;
        Debug.Log($"📍 Character Position: {transform.position}");
        Debug.Log($"📏 Character Scale: {transform.localScale}");
        Debug.Log($"🔄 Character Rotation: {transform.rotation.eulerAngles}");
        
        // Check all sprite renderers
        var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        Debug.Log($"📊 Total SpriteRenderers found: {renderers.Length}");
        
        int visibleRenderers = 0;
        int renderersWithSprites = 0;
        
        foreach (var renderer in renderers)
        {
            if (renderer.sprite != null)
            {
                renderersWithSprites++;
                
                if (renderer.enabled && renderer.gameObject.activeSelf)
                {
                    visibleRenderers++;
                    
                    Debug.Log($"✅ VISIBLE: {renderer.name} - Sprite: {renderer.sprite.name}, Order: {renderer.sortingOrder}, Color: {renderer.color}, Bounds: {renderer.bounds}");
                }
                else
                {
                    Debug.Log($"❌ HIDDEN: {renderer.name} - Enabled: {renderer.enabled}, Active: {renderer.gameObject.activeSelf}");
                }
            }
        }
        
        Debug.Log($"📈 Summary: {renderersWithSprites} have sprites, {visibleRenderers} are visible");
        
        // Check camera position relative to character
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
            Debug.Log($"📷 Camera distance from character: {distance}");
            Debug.Log($"📷 Camera position: {mainCamera.transform.position}");
            Debug.Log($"📷 Camera orthographic size: {mainCamera.orthographicSize}");
            
            // Check if character is in camera view
            var viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
            bool inView = viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
            Debug.Log($"📺 Character in camera view: {inView} (viewport: {viewportPoint})");
        }
        
        // Check for animator
        var animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log($"🎬 Animator found: {animator.name}, Enabled: {animator.enabled}");
            Debug.Log($"🎬 Current State: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}");
            Debug.Log($"🎬 Has Controller: {animator.runtimeAnimatorController != null}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No Animator found - SPUM characters usually need animators!");
        }
        
        // Check scale issues
        if (transform.localScale.x < 0.1f || transform.localScale.y < 0.1f)
        {
            Debug.LogError($"🚨 CHARACTER TOO SMALL! Scale: {transform.localScale} - This could make it invisible!");
        }
        
        if (transform.localScale.x > 10f || transform.localScale.y > 10f)
        {
            Debug.LogError($"🚨 CHARACTER TOO LARGE! Scale: {transform.localScale} - This could make it render outside view!");
        }
        
        Debug.Log("🔍 === END VISIBILITY DEBUG ===");
    }
}
