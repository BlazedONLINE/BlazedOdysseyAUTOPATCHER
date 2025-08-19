#nullable enable
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using BlazedOdyssey.UI;
using System.Collections.Generic;
using System.Linq;

namespace BlazedOdyssey.UI
{
    public class BlazedUIBootstrap : MonoBehaviour
    {
        [Header("Canvas")]
        public Canvas canvas = null!;
        public CanvasScaler scaler = null!;
        public RectTransform uiRoot = null!; // scaled by Settings

        [Header("HUD")] public PlayerHUD playerHUD = null!; public GroupHUD groupHUD = null!;
        [Header("Inventory")] public InventoryUI inventoryUI = null!;
        [Header("Action Bar")] public ActionBarUI actionBar = null!;
        [Header("Settings")] public SettingsPanel settingsPanel = null!;
        
        [Header("Scene Control")]
        public bool onlyShowInGameplayScenes = true; // Restored to true for proper chat functionality
        [Tooltip("Force HUD to show even in non-gameplay scenes (for editing)")]
        public bool forceShowForEditing = false;
        
        [Header("Player HUD Configuration")]
        [Tooltip("Width of the player HUD panel")]
        public float hudWidth = 320f;
        [Tooltip("Height of the player HUD panel")]
        public float hudHeight = 110f;
        [Tooltip("Size of the character portrait")]
        public float portraitSize = 80f;
        [Tooltip("Padding around the portrait")]
        public float portraitPadding = 8f;
        
        [Header("HP/MP Bar Customization")]
        [Tooltip("Height of HP bar")]
        public float hpBarHeight = 22f;
        [Tooltip("Width of HP bar (0 = auto-calculate from available space)")]
        public float hpBarWidth = 0f;
        [Tooltip("Height of MP bar")]
        public float mpBarHeight = 22f;
        [Tooltip("Width of MP bar (0 = auto-calculate from available space)")]
        public float mpBarWidth = 0f;
        [Tooltip("Spacing between HP and MP bars")]
        public float barSpacing = 4f;
        [Tooltip("Right side padding")]
        public float rightPadding = 8f;
        
        [Header("Background Panel Options")]
        [Tooltip("Show the grey background panel behind the HUD elements")]
        public bool showBackgroundPanel = true;
        [Tooltip("Background panel color")]
        public Color backgroundPanelColor = new Color(0f, 0f, 0f, 0.55f);
        [Tooltip("Background panel outline color")]
        public Color backgroundOutlineColor = new Color(1f, 1f, 1f, 0.12f);
        
        private bool _isInitialized = false;
        private static bool _staticInitialized = false;
        private static BlazedUIBootstrap? _primaryInstance = null;

        private void Awake()
        {
            // NUCLEAR INFINITE LOOP PREVENTION - Only allow ONE instance EVER
            if (_staticInitialized && _primaryInstance != null && _primaryInstance != this)
            {
                Debug.LogError($"[BlazedUIBootstrap] DESTROYING DUPLICATE INSTANCE - Primary instance already exists on {_primaryInstance.gameObject.name}!");
                DestroyImmediate(this.gameObject);
                return;
            }
            
            if (_primaryInstance == null)
            {
                _primaryInstance = this;
                Debug.Log("[BlazedUIBootstrap] Setting as PRIMARY instance");
            }
            
            // Additional safety - destroy any other existing bootstraps
            var existingBootstraps = FindObjectsByType<BlazedUIBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existingBootstraps.Length > 1)
            {
                Debug.LogError($"[BlazedUIBootstrap] FOUND {existingBootstraps.Length} BOOTSTRAPS - Destroying all except primary!");
                foreach (var bootstrap in existingBootstraps)
                {
                    if (bootstrap != _primaryInstance)
                    {
                        Debug.LogError($"[BlazedUIBootstrap] Destroying duplicate bootstrap on {bootstrap.gameObject.name}");
                        DestroyImmediate(bootstrap.gameObject);
                    }
                }
                return; // Exit early after cleanup
            }
            
            // Check if we should show HUD in this scene
            if (onlyShowInGameplayScenes && !forceShowForEditing && !SceneStateDetector.ShouldShowHUD())
            {
                gameObject.SetActive(false);
                return;
            }

            if (forceShowForEditing)
            {
                Debug.Log("[BlazedUIBootstrap] Force showing HUD for editing purposes");
            }

            // ONLY Initialize if not already done
            if (!_isInitialized)
            {
                Initialize();
            }
            else
            {
                Debug.Log("[BlazedUIBootstrap] Already initialized - skipping duplicate initialization");
            }
        }

        private void Start()
        {
            // Double-check scene state after all Awake calls
            if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowHUD())
            {
                Debug.Log($"[BlazedUIBootstrap] Disabling HUD - scene changed or not in gameplay scene");
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
            // Re-evaluate if we should show HUD in the new scene
            if (onlyShowInGameplayScenes)
            {
                bool shouldShow = SceneStateDetector.ShouldShowHUD();
                gameObject.SetActive(shouldShow);
                
                if (shouldShow && !_isInitialized)
                {
                    Initialize();
                }
                
                Debug.Log($"[BlazedUIBootstrap] Scene loaded: {scene.name}, HUD active: {shouldShow}");
                DisableLegacyHudSystems();
            }
        }

        private void Initialize()
        {
            if (_isInitialized) 
            {
                Debug.Log("[BlazedUIBootstrap] Initialize() called but already initialized - skipping");
                return;
            }
            
            if (_staticInitialized)
            {
                Debug.LogError("[BlazedUIBootstrap] STATIC FLAG ALREADY SET - Another instance already initialized! Aborting to prevent infinite loop!");
                return;
            }
            
            Debug.Log("[BlazedUIBootstrap] Starting HUD initialization...");
            
            // Clear any existing HUD elements first to prevent duplicates
            ClearExistingHUD();
            DestroyDuplicateCanvases();
            DisableLegacyHudSystems();
            
            EnsureEventSystem();
            EnsureAudioListener();
            EnsureGameAudioManager();
            EnsureChatAdapterAndFixEmoji();
            BuildCanvas();
            BuildPlayerHUD();
            CreatePlayerNameLabel();
            BuildGroupHUD();
            BuildInventory();
            BuildActionBar();
            BuildSettings();
            EnsureHudComponents();
            SetupChatSystem();
            SetupPortraitCapture();
            
            // Force load saved layout after initialization
            Debug.Log("[BlazedUIBootstrap] Loading saved HUD layout...");
            HudEditMode.ForceApplySavedLayout();
            
            _isInitialized = true;
            _staticInitialized = true; // Set static flag to prevent any other instances
            Debug.Log("[BlazedUIBootstrap] HUD initialization complete! Static flag set to prevent duplicates.");
        }

        /// <summary>
        /// Reset static flags - ONLY call this when transitioning scenes or when you're absolutely sure no other instances exist
        /// </summary>
        public static void ResetStaticFlags()
        {
            Debug.Log("[BlazedUIBootstrap] RESETTING STATIC FLAGS - allowing new initialization");
            _staticInitialized = false;
            _primaryInstance = null;
        }

        private void OnDestroy()
        {
            // If the primary instance is being destroyed, reset static flags to allow new instances
            if (_primaryInstance == this)
            {
                Debug.Log("[BlazedUIBootstrap] Primary instance destroyed - resetting static flags");
                ResetStaticFlags();
            }
        }

        private void Update()
        {
            // Process input whenever this HUD is active; guard against typing in UI fields
            if (!gameObject.activeInHierarchy) return;

            bool typing = false;
            var es = EventSystem.current;
            if (es != null && es.currentSelectedGameObject != null)
            {
                // typing if any input field (TMP or legacy) is focused
                if (es.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null) typing = true;
                if (es.currentSelectedGameObject.GetComponent<InputField>() != null) typing = true;
            }
            if (typing) return;

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (inventoryUI == null) inventoryUI = uiRoot != null ? uiRoot.GetComponentInChildren<InventoryUI>(true) : FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
                if (inventoryUI == null)
                {
                    EnsureCanvasAndRoot();
                    BuildInventory();
                }
                inventoryUI?.Toggle();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsPanel == null) settingsPanel = uiRoot != null ? uiRoot.GetComponentInChildren<SettingsPanel>(true) : FindFirstObjectByType<SettingsPanel>(FindObjectsInactive.Include);
                if (settingsPanel == null)
                {
                    EnsureCanvasAndRoot();
                    BuildSettings();
                }
                settingsPanel?.Toggle();
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (groupHUD == null)
                {
                    BuildGroupHUD();
                }
                else
                {
                    var go = groupHUD.gameObject;
                    go.SetActive(!go.activeSelf);
                }
            }

            // Update player coordinates (whole numbers)
            UpdatePlayerCoordinates();
        }

        private Transform _playerTransform;
        private Vector2Int _lastCoords = new Vector2Int(int.MinValue, int.MinValue);
        private void UpdatePlayerCoordinates()
        {
            if (playerHUD == null || playerHUD.coordsText == null) return;
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _playerTransform = player.transform;
            }
            if (_playerTransform == null) return;
            var pos = _playerTransform.position;
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            if (_lastCoords.x == x && _lastCoords.y == y) return;
            _lastCoords = new Vector2Int(x, y);
            playerHUD.coordsText.text = $"X:{x} Y:{y}";
        }

        private void EnsureCanvasAndRoot()
        {
            if (canvas != null && uiRoot != null) return;
            BuildCanvas();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }
        }

        private void EnsureAudioListener()
        {
            // Keep exactly one AudioListener: prefer MainCamera, else attach to any camera, else create a listener object
            var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (listeners.Length == 0)
            {
                var cam = Camera.main ?? FindFirstObjectByType<Camera>();
                if (cam != null)
                {
                    if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    var go = new GameObject("AudioListener");
                    go.AddComponent<AudioListener>();
                }
            }
            else if (listeners.Length > 1)
            {
                for (int i = 1; i < listeners.Length; i++) listeners[i].enabled = false;
            }
        }

        private void EnsureGameAudioManager()
        {
            if (FindFirstObjectByType<GameAudioManager>() == null)
            {
                new GameObject("GameAudioManager", typeof(GameAudioManager));
            }
        }

        private void EnsureChatAdapterAndFixEmoji()
        {
            // Ensure UltimateChatAdapter exists and disables emoji features if no sprite asset is present
            var adapter = FindFirstObjectByType<UltimateChatAdapter>(FindObjectsInactive.Include);
            if (adapter == null)
            {
                var go = new GameObject("UltimateChatAdapter");
                go.AddComponent<UltimateChatAdapter>();
            }
        }

        private void DisableLegacyHudSystems()
        {
            var monos = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var m in monos)
            {
                if (m == null) continue;
                var tn = m.GetType().Name;
                if (tn == "MMOUISystem" || tn == "SimpleMMOUISystem" || tn == "GameUIBootstrap" || tn == "GameUIBootstrap_DISABLED")
                {
                    if (m.gameObject.activeInHierarchy)
                    {
                        Debug.Log($"[BlazedUIBootstrap] Disabling legacy HUD system: {tn} on {m.gameObject.name}");
                        m.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void BuildCanvas()
        {
            // Reuse existing BlazedUI_Canvas if present; otherwise create a new one
            if (canvas != null && uiRoot != null)
            {
                return;
            }
            var existingCanvasGO = GameObject.Find("BlazedUI_Canvas");
            if (existingCanvasGO != null)
            {
                canvas = existingCanvasGO.GetComponent<Canvas>();
                if (canvas == null) canvas = existingCanvasGO.AddComponent<Canvas>();
                scaler = existingCanvasGO.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = existingCanvasGO.AddComponent<CanvasScaler>();
                var gr = existingCanvasGO.GetComponent<GraphicRaycaster>();
                if (gr == null) existingCanvasGO.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                uiRoot = existingCanvasGO.transform.Find("UIRoot")?.GetComponent<RectTransform>();
                if (uiRoot == null)
                {
                    uiRoot = new GameObject("UIRoot", typeof(RectTransform)).GetComponent<RectTransform>();
                    uiRoot.SetParent(canvas.transform, false);
                    uiRoot.anchorMin = Vector2.zero; uiRoot.anchorMax = Vector2.one; uiRoot.offsetMin = Vector2.zero; uiRoot.offsetMax = Vector2.zero;
                }
                Debug.Log("[BlazedUIBootstrap] Reusing existing BlazedUI_Canvas.");
                return;
            }
            // Create new
            Debug.Log("[BlazedUIBootstrap] Creating new BlazedUI_Canvas...");
            var canvasGO = new GameObject("BlazedUI_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.layer = LayerMask.NameToLayer("UI");
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            uiRoot = new GameObject("UIRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            uiRoot.SetParent(canvas.transform, false);
            uiRoot.anchorMin = Vector2.zero; uiRoot.anchorMax = Vector2.one; uiRoot.offsetMin = Vector2.zero; uiRoot.offsetMax = Vector2.zero;
            Debug.Log($"[BlazedUIBootstrap] Successfully created BlazedUI_Canvas with ID: {canvasGO.GetInstanceID()}");
        }

        private void DestroyDuplicateCanvases()
        {
            // If multiple BlazedUI_Canvas exist, keep the first and destroy extras
            var canvases = Resources.FindObjectsOfTypeAll<Canvas>().Where(c => c.gameObject.name == "BlazedUI_Canvas").Select(c=>c.gameObject).ToList();
            if (canvases.Count <= 1) return;
            Debug.LogWarning($"[BlazedUIBootstrap] Found {canvases.Count} BlazedUI_Canvas objects. Destroying duplicates.");
            var keep = canvases[0];
            for (int i = 1; i < canvases.Count; i++)
            {
                DestroyImmediate(canvases[i]);
            }
        }

        private RectTransform Panel(RectTransform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2? anchoredPos = null, bool addBackground = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = size;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
            if (anchoredPos.HasValue) rt.anchoredPosition = anchoredPos.Value;
            
            // Add background panel if enabled and requested
            if (showBackgroundPanel && addBackground)
            {
                AddBackgroundPanel(go);
            }
            
            return rt;
        }
        
        private RectTransform PanelNoBackground(RectTransform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2? anchoredPos = null)
        {
            return Panel(parent, name, size, anchorMin, anchorMax, pivot, anchoredPos, false);
        }
        
        private void AddBackgroundPanel(GameObject target)
        {
            var img = target.AddComponent<Image>();
            img.color = backgroundPanelColor;
            var outline = target.AddComponent<Outline>();
            outline.effectColor = backgroundOutlineColor;
            outline.effectDistance = new Vector2(1f, -1f);
        }
        
        /// <summary>
        /// Toggle background panel visibility at runtime
        /// </summary>
        public void ToggleBackgroundPanels(bool visible)
        {
            showBackgroundPanel = visible;
            
            // Find all existing panels and toggle their background components
            var hudMovables = FindObjectsByType<HudMovable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var movable in hudMovables)
            {
                var img = movable.GetComponent<Image>();
                var outline = movable.GetComponent<Outline>();
                
                if (visible && img == null)
                {
                    // Add background if it doesn't exist
                    AddBackgroundPanel(movable.gameObject);
                }
                else if (!visible && img != null)
                {
                    // Remove background if it exists
                    if (outline != null) DestroyImmediate(outline);
                    DestroyImmediate(img);
                }
                else if (visible && img != null)
                {
                    // Update existing background colors
                    img.color = backgroundPanelColor;
                    if (outline != null) outline.effectColor = backgroundOutlineColor;
                }
            }
            
            Debug.Log($"[BlazedUIBootstrap] Background panels {(visible ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Context menu methods for testing in editor
        /// </summary>
        [ContextMenu("Toggle Background Panels")]
        public void ToggleBackgroundPanelsMenu()
        {
            ToggleBackgroundPanels(!showBackgroundPanel);
        }
        
        
        [ContextMenu("Enable Scene View Editing")]
        public void EnableSceneViewEditing()
        {
            forceShowForEditing = true;
            var hudEditMode = GetComponent<HudEditMode>();
            if (hudEditMode == null)
            {
                hudEditMode = gameObject.AddComponent<HudEditMode>();
            }
            hudEditMode.allowEditingAnywhere = true;
            
            if (!_isInitialized)
            {
                Initialize();
            }
            
            Debug.Log("[BlazedUIBootstrap] Scene view editing enabled - press F2 to edit HUD");
        }
        
        [ContextMenu("Disable Scene View Editing")]
        public void DisableSceneViewEditing()
        {
            Debug.Log($"[BlazedUIBootstrap] BEFORE - forceShowForEditing: {forceShowForEditing}, GameObject active: {gameObject.activeInHierarchy}");
            
            forceShowForEditing = false;
            var hudEditMode = GetComponent<HudEditMode>();
            if (hudEditMode != null)
            {
                hudEditMode.allowEditingAnywhere = false;
                
                // If we're currently in edit mode, turn it off
                hudEditMode.ExitEditMode();
            }
            
            // Force re-evaluation of scene state
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[BlazedUIBootstrap] Current scene after disable: {currentScene}");
            Debug.Log($"[BlazedUIBootstrap] Scene type detected: {SceneStateDetector.GetSceneTypeDisplayName()}");
            Debug.Log($"[BlazedUIBootstrap] Should show HUD: {SceneStateDetector.ShouldShowHUD()}");
            Debug.Log($"[BlazedUIBootstrap] onlyShowInGameplayScenes: {onlyShowInGameplayScenes}");
            
            // NOW actually hide the HUD if we shouldn't show it
            if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowHUD())
            {
                Debug.Log("[BlazedUIBootstrap] HIDING HUD - calling SetActive(false)");
                gameObject.SetActive(false);
                Debug.Log($"[BlazedUIBootstrap] GameObject active after SetActive(false): {gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.Log("[BlazedUIBootstrap] HUD remains visible - in gameplay scene");
            }
            
            // Also hide all child HUD elements manually
            var hudMovables = GetComponentsInChildren<HudMovable>(true);
            foreach (var hud in hudMovables)
            {
                if (!SceneStateDetector.ShouldShowHUD())
                {
                    hud.gameObject.SetActive(false);
                    Debug.Log($"[BlazedUIBootstrap] Manually disabled HUD element: {hud.Id}");
                }
            }
            
            Debug.Log("[BlazedUIBootstrap] Scene view editing disabled - normal scene detection restored");
        }
        
        [ContextMenu("Reset to Default Settings")]
        public void ResetToDefaultSettings()
        {
            // Reset all flags to default
            forceShowForEditing = false;
            onlyShowInGameplayScenes = true;
            showBackgroundPanel = true;
            backgroundPanelColor = new Color(0f, 0f, 0f, 0.35f);
            backgroundOutlineColor = new Color(1f, 1f, 1f, 0.08f);
            
            var hudEditMode = GetComponent<HudEditMode>();
            if (hudEditMode != null)
            {
                hudEditMode.allowEditingAnywhere = false;
                hudEditMode.onlyAllowInGameplayScenes = true;
                hudEditMode.ExitEditMode();
            }
            
            // Check if we should be visible in current scene
            if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowHUD())
            {
                gameObject.SetActive(false);
            }
            
            Debug.Log("[BlazedUIBootstrap] Reset to default settings - normal gameplay-only mode restored");
        }
        
        [ContextMenu("Debug Current Scene")]
        public void DebugCurrentScene()
        {
            SceneStateDetector.LogCurrentSceneInfo();
        }
        
        [ContextMenu("Add Current Scene as Gameplay Scene")]
        public void AddCurrentSceneAsGameplay()
        {
            SceneStateDetector.AddCurrentSceneToGameplayScenes();
            Debug.Log("[BlazedUIBootstrap] Current scene added to gameplay scenes - HUD should now show here");
        }
        
        [ContextMenu("Add Current Scene as Character Scene")]
        public void AddCurrentSceneAsCharacter()
        {
            SceneStateDetector.AddCurrentSceneToCharacterScenes();
            Debug.Log("[BlazedUIBootstrap] Current scene added to character scenes - HUD should NOT show here");
        }
        
        [ContextMenu("Force Show HUD Here")]
        public void ForceShowHUD()
        {
            gameObject.SetActive(true);
            if (!_isInitialized)
            {
                Initialize();
            }
            Debug.Log("[BlazedUIBootstrap] HUD force shown in current scene");
        }
        
        [ContextMenu("EMERGENCY - Reset Saved HUD Layout")]
        public void EmergencyResetHUDLayout()
        {
            var transforms = new Dictionary<string, HudTransformData>();
            HudLayoutStore.SaveComplete(transforms);
            Debug.Log("[BlazedUIBootstrap] EMERGENCY: Cleared all saved HUD layout data");
            
            // Force reinitialize
            if (_isInitialized)
            {
                _isInitialized = false;
                Initialize();
            }
        }
        
        [ContextMenu("Refresh HUD Visibility")]
        public void RefreshHUDVisibility()
        {
            Debug.Log($"[BlazedUIBootstrap] Current flags - forceShowForEditing: {forceShowForEditing}, onlyShowInGameplayScenes: {onlyShowInGameplayScenes}");
            Debug.Log($"[BlazedUIBootstrap] Scene detection - Should show HUD: {SceneStateDetector.ShouldShowHUD()}");
            
            // Apply the same logic as Awake
            bool shouldShow = !onlyShowInGameplayScenes || forceShowForEditing || SceneStateDetector.ShouldShowHUD();
            
            Debug.Log($"[BlazedUIBootstrap] Final decision - Should show HUD: {shouldShow}");
            
            gameObject.SetActive(shouldShow);
            
            if (shouldShow && !_isInitialized)
            {
                Initialize();
            }
        }
        
        [ContextMenu("Find All HUD Instances")]
        public void FindAllHUDInstances()
        {
            var allBootstraps = FindObjectsByType<BlazedUIBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[BlazedUIBootstrap] Found {allBootstraps.Length} BlazedUIBootstrap instances:");
            
            for (int i = 0; i < allBootstraps.Length; i++)
            {
                var bootstrap = allBootstraps[i];
                Debug.Log($"  {i+1}. {bootstrap.gameObject.name} (active: {bootstrap.gameObject.activeInHierarchy}) - forceShowForEditing: {bootstrap.forceShowForEditing}");
            }
            
            var allMovables = FindObjectsByType<HudMovable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[BlazedUIBootstrap] Found {allMovables.Length} HudMovable instances:");
            
            foreach (var movable in allMovables)
            {
                Debug.Log($"  - {movable.Id} on {movable.gameObject.name} (active: {movable.gameObject.activeInHierarchy})");
            }
        }
        
        [ContextMenu("NUCLEAR - Destroy All HUD")]
        public void NuclearDestroyAllHUD()
        {
            Debug.Log("[BlazedUIBootstrap] NUCLEAR OPTION - Destroying ALL HUD elements");
            
            // Find and destroy all BlazedUIBootstrap instances
            var allBootstraps = FindObjectsByType<BlazedUIBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[BlazedUIBootstrap] Found {allBootstraps.Length} BlazedUIBootstrap instances to destroy");
            
            foreach (var bootstrap in allBootstraps)
            {
                Debug.Log($"[BlazedUIBootstrap] Destroying: {bootstrap.gameObject.name}");
                if (bootstrap != this)
                {
                    DestroyImmediate(bootstrap.gameObject);
                }
            }
            
            // Find and destroy all HudMovable elements
            var allMovables = FindObjectsByType<HudMovable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[BlazedUIBootstrap] Found {allMovables.Length} HudMovable instances to destroy");
            
            foreach (var movable in allMovables)
            {
                Debug.Log($"[BlazedUIBootstrap] Destroying HUD element: {movable.Id}");
                DestroyImmediate(movable.gameObject);
            }
            
            // Find and destroy anything with "HUD" in the name (but NOT player spawn objects!)
            var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                var name = obj.name.ToLower();
                if ((name.Contains("hud") || name.Contains("actionbar")) && 
                    !name.Contains("spawn") && !name.Contains("character") && !name.Contains("player"))
                {
                    Debug.Log($"[BlazedUIBootstrap] Destroying object with HUD-like name: {obj.name}");
                    DestroyImmediate(obj);
                }
            }
            
            // Finally destroy this instance
            Debug.Log("[BlazedUIBootstrap] Destroying self");
            DestroyImmediate(this.gameObject);
        }
        
        [ContextMenu("AGGRESSIVE - Disable All HUD")]
        public void AggressiveDisableAllHUD()
        {
            Debug.Log("[BlazedUIBootstrap] AGGRESSIVE DISABLE - Disabling ALL HUD elements");
            
            // Disable all BlazedUIBootstrap instances
            var allBootstraps = FindObjectsByType<BlazedUIBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var bootstrap in allBootstraps)
            {
                bootstrap.forceShowForEditing = false;
                bootstrap.gameObject.SetActive(false);
                Debug.Log($"[BlazedUIBootstrap] Disabled: {bootstrap.gameObject.name}");
            }
            
            // Disable all HudMovable elements
            var allMovables = FindObjectsByType<HudMovable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var movable in allMovables)
            {
                movable.gameObject.SetActive(false);
                Debug.Log($"[BlazedUIBootstrap] Disabled HUD element: {movable.Id}");
            }
            
            // Disable anything with "HUD" in the name (but NOT player spawn objects!)
            var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                var name = obj.name.ToLower();
                if ((name.Contains("hud") || name.Contains("actionbar")) && 
                    !name.Contains("spawn") && !name.Contains("character") && !name.Contains("player"))
                {
                    obj.SetActive(false);
                    Debug.Log($"[BlazedUIBootstrap] Disabled object: {obj.name}");
                }
            }
            
            Debug.Log("[BlazedUIBootstrap] All HUD elements should now be disabled");
        }
        
        [ContextMenu("Show All Active UI Objects")]
        public void ShowAllActiveUIObjects()
        {
            Debug.Log("[BlazedUIBootstrap] === ALL ACTIVE UI OBJECTS ===");
            
            var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var uiObjects = new System.Collections.Generic.List<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.activeInHierarchy && (
                    obj.GetComponent<Canvas>() != null ||
                    obj.GetComponent<Image>() != null ||
                    obj.GetComponent<Text>() != null ||
                    obj.name.ToLower().Contains("ui") ||
                    obj.name.ToLower().Contains("hud") ||
                    obj.name.ToLower().Contains("canvas")))
                {
                    uiObjects.Add(obj);
                }
            }
            
            Debug.Log($"[BlazedUIBootstrap] Found {uiObjects.Count} active UI objects:");
            foreach (var obj in uiObjects)
            {
                var parent = obj.transform.parent ? obj.transform.parent.name : "ROOT";
                Debug.Log($"  - {obj.name} (parent: {parent}) - Layer: {obj.layer}");
            }
        }
        
        [ContextMenu("EMERGENCY - Restore Player Spawn")]
        public void RestorePlayerSpawn()
        {
            Debug.Log("[BlazedUIBootstrap] EMERGENCY - Looking for disabled PlayerSpawn objects");
            
            var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                var name = obj.name.ToLower();
                if (name.Contains("spawn") || name.Contains("player"))
                {
                    obj.SetActive(true);
                    Debug.Log($"[BlazedUIBootstrap] Restored: {obj.name} (was active: {obj.activeInHierarchy})");
                }
            }
            
            Debug.Log("[BlazedUIBootstrap] Player spawn restoration complete");
        }
        
        [ContextMenu("SCENE VIEW - Enable HUD Editing")]
        public void EnableSceneViewHUDEditing()
        {
            // Force show in any scene for editing
            forceShowForEditing = true;
            onlyShowInGameplayScenes = false; // Allow in any scene
            
            // Ensure HUD is visible
            gameObject.SetActive(true);
            
            // Initialize if needed
            if (!_isInitialized)
            {
                Initialize();
            }
            
            // Enable editing
            var hudEditMode = GetComponent<HudEditMode>();
            if (hudEditMode == null)
            {
                hudEditMode = gameObject.AddComponent<HudEditMode>();
            }
            hudEditMode.allowEditingAnywhere = true;
            
            Debug.Log("[BlazedUIBootstrap] SCENE VIEW EDITING ENABLED - Press F2 to edit HUD");
            Debug.Log("[BlazedUIBootstrap] Controls: F2=Toggle Edit, Drag=Move, Ctrl+L=Save, Ctrl+R=Reset, B=Toggle BG");
        }
        
        [ContextMenu("SCENE VIEW - Disable and Return to Normal")]
        public void DisableSceneViewEditingAndReturnToNormal()
        {
            // Restore normal settings
            forceShowForEditing = false;
            onlyShowInGameplayScenes = true;
            
            // Disable editing
            var hudEditMode = GetComponent<HudEditMode>();
            if (hudEditMode != null)
            {
                hudEditMode.allowEditingAnywhere = false;
                hudEditMode.ExitEditMode();
            }
            
            // Apply normal scene logic
            if (!SceneStateDetector.ShouldShowHUD())
            {
                gameObject.SetActive(false);
                Debug.Log("[BlazedUIBootstrap] HUD hidden - returned to normal gameplay-only mode");
            }
            else
            {
                Debug.Log("[BlazedUIBootstrap] HUD visible - in gameplay scene");
            }
        }
        
        [ContextMenu("FORCE HIDE HUD in Character Selection")]
        public void ForceHideHUDInCharacterSelection()
        {
            forceShowForEditing = false;
            
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
            if (currentScene.Contains("character") || currentScene.Contains("selection"))
            {
                // Forcibly hide everything
                gameObject.SetActive(false);
                
                // Also hide canvas
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(false);
                }
                
                Debug.Log("[BlazedUIBootstrap] FORCE HIDDEN in Character Selection scene");
            }
            else
            {
                Debug.Log($"[BlazedUIBootstrap] Not in Character Selection (current: {currentScene})");
            }
        }
        
        private void ClearExistingHUD()
        {
            // Clean up duplicates but keep a single BlazedUI_Canvas if present
            var canvases = Resources.FindObjectsOfTypeAll<Canvas>().Where(c => c.gameObject.name == "BlazedUI_Canvas").Select(c=>c.gameObject).ToList();
            if (canvases.Count > 1)
            {
                Debug.LogWarning($"[BlazedUIBootstrap] Found {canvases.Count} BlazedUI_Canvas objects. Destroying extras to keep one.");
                for (int i = 1; i < canvases.Count; i++) DestroyImmediate(canvases[i]);
            }
            // Reset refs so BuildCanvas can rebind to the remaining or create new
            canvas = null;
            uiRoot = null;
        }
        
        [ContextMenu("DEBUG - Check Save File")]
        public void DebugSaveFile()
        {
            var savePath = System.IO.Path.Combine(Application.persistentDataPath, "hud_layout.json");
            Debug.Log($"[BlazedUIBootstrap] Save file path: {savePath}");
            
            if (System.IO.File.Exists(savePath))
            {
                var content = System.IO.File.ReadAllText(savePath);
                Debug.Log($"[BlazedUIBootstrap] Save file exists. Content: {content}");
            }
            else
            {
                Debug.Log("[BlazedUIBootstrap] Save file does NOT exist");
            }
            
            // Also test loading
            if (HudLayoutStore.Load(out var positions))
            {
                Debug.Log($"[BlazedUIBootstrap] Successfully loaded {positions.Count} saved positions:");
                foreach (var kv in positions)
                {
                    Debug.Log($"  {kv.Key} = {kv.Value}");
                }
            }
            else
            {
                Debug.Log("[BlazedUIBootstrap] Failed to load saved positions");
            }
        }

        private void SetupChatSystem()
        {
            // Only setup chat in gameplay scenes
            if (!SceneStateDetector.ShouldShowChat())
            {
                Debug.Log("[BlazedUIBootstrap] Skipping chat setup - not in gameplay scene");
                return;
            }

            // Check if UltimateChatAdapter already exists (existing system)
            var existingAdapters = FindObjectsByType<UltimateChatAdapter>(FindObjectsInactive.Include, FindObjectsSortMode.None); // Include inactive objects
            Debug.Log($"[BlazedUIBootstrap] Found {existingAdapters.Length} UltimateChatAdapter(s) in scene");
            
            if (existingAdapters.Length > 0)
            {
                var existingAdapter = existingAdapters[0];
                Debug.Log($"[BlazedUIBootstrap] UltimateChatAdapter already exists on {existingAdapter.gameObject.name} - using existing chat system");
                
                // The existing adapter should now be scene-aware with our modifications
                // Make sure it's enabled if we're in a gameplay scene
                if (SceneStateDetector.ShouldShowChat() && !existingAdapter.gameObject.activeInHierarchy)
                {
                    existingAdapter.gameObject.SetActive(true);
                    Debug.Log("[BlazedUIBootstrap] Enabled existing UltimateChatAdapter for gameplay scene");
                }
                return;
            }

            // Check if ChatSystemBootstrap already exists
            var existingBootstrap = FindFirstObjectByType<ChatSystemBootstrap>();
            if (existingBootstrap != null)
            {
                Debug.Log("[BlazedUIBootstrap] ChatSystemBootstrap already exists");
                return;
            }

            // Check if ChatSystemManager already exists
            var existingManager = FindFirstObjectByType<ChatSystemManager>();
            if (existingManager != null)
            {
                Debug.Log("[BlazedUIBootstrap] ChatSystemManager already exists");
                return;
            }

            Debug.Log("[BlazedUIBootstrap] Setting up chat system...");

            // TEMPORARY: Create a simple UltimateChatAdapter directly instead of ChatSystemBootstrap
            // to avoid conflicts with existing systems
            var chatAdapterGO = new GameObject("UltimateChatAdapter_Runtime");
            var chatAdapter = chatAdapterGO.AddComponent<UltimateChatAdapter>();
            
            // Don't destroy on load so it persists between scenes
            DontDestroyOnLoad(chatAdapterGO);
            
            Debug.Log("[BlazedUIBootstrap] Chat system setup complete!");
        }

        private void SetupPortraitCapture()
        {
            // Check if SPUMPortraitCapture already exists
            var existingCapture = FindFirstObjectByType<SPUMPortraitCapture>();
            if (existingCapture != null)
            {
                Debug.Log("[BlazedUIBootstrap] SPUMPortraitCapture already exists");
                return;
            }

            Debug.Log("[BlazedUIBootstrap] Setting up portrait capture system...");

            // Add SPUMPortraitCapture to this GameObject
            var portraitCapture = gameObject.AddComponent<SPUMPortraitCapture>();
            
            Debug.Log("[BlazedUIBootstrap] Portrait capture system setup complete!");
        }

        private Text MakeText(RectTransform parent, string text, int size, TextAnchor anchor, Vector2 offsetMin, Vector2 offsetMax, FontStyle style = FontStyle.Normal, float alpha = 0.95f)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            var t = go.AddComponent<Text>();
            
            // Use safe font loading
            try
            {
                // Try preferred cute fonts first
                Font f = Resources.Load<Font>("Breathe Fire III");
                if (f == null) f = Resources.Load<Font>("Fonts/Breathe Fire III");
                if (f == null) f = Resources.Load<Font>("BreatheFireIII");
                if (f == null) f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.font = f;
            }
            catch (System.Exception)
            {
                Debug.LogWarning("[BlazedUIBootstrap] Could not load LegacyRuntime.ttf, using default font");
            }
            
            t.text = text; t.fontSize = size; t.alignment = anchor; t.fontStyle = style;
            t.color = new Color(1,1,1,alpha);
            return t;
        }

        private Image MakeImage(RectTransform parent, string name, Vector2 offsetMin, Vector2 offsetMax, Sprite sprite = null, Image.Type type = Image.Type.Sliced)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            var img = go.GetComponent<Image>();
            img.sprite = sprite; img.type = type; img.color = new Color(1,1,1,0.08f);
            return img;
        }

        private void CreatePlayerNameLabel()
        {
            // Player nameplate that follows the player's world position.
            var go = new GameObject("PlayerNameplate", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(uiRoot, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(180, 26);
            var text = MakeText(rt, "Hero", 16, TextAnchor.LowerCenter, Vector2.zero, Vector2.zero, FontStyle.Bold);
            var outline = text.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0f,0f,0f,0.9f); outline.effectDistance = new Vector2(1,-1);

            var nameplate = go.AddComponent<BlazedOdyssey.UI.NameplateUI>();
            nameplate.rectTransform = rt;
            nameplate.label = text;
            nameplate.outline = outline;
            nameplate.canvas = canvas;

            // Try to find player transform
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Use HUD name text for consistency
                var displayName = playerHUD != null && playerHUD.nameText != null ? playerHUD.nameText.text : "Hero";
                nameplate.Initialize(player.transform, displayName, canvas);
            }
        }

        private BarUI MakeBar(RectTransform parent, string name, Vector2 position, Vector2 size)
        {
            var barGO = new GameObject(name, typeof(RectTransform), typeof(Image));
            var barRT = barGO.GetComponent<RectTransform>();
            barRT.SetParent(parent, false);
            // Use fixed anchoring for manual resizing - no stretching
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(0, 0);
            barRT.pivot = new Vector2(0, 0);
            barRT.sizeDelta = size;
            barRT.anchoredPosition = position;
            
            var barImg = barGO.GetComponent<Image>();
            barImg.color = new Color(0f, 0f, 0f, 0.5f);

            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.SetParent(barRT, false);
            fillRT.anchorMin = Vector2.zero; 
            fillRT.anchorMax = Vector2.one; 
            fillRT.offsetMin = new Vector2(2, 2); 
            fillRT.offsetMax = new Vector2(-2, -2);
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.type = Image.Type.Filled; 
            fillImg.fillMethod = Image.FillMethod.Horizontal; 
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;

            var label = MakeText(barRT, "100/100", 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);

            var bar = barGO.AddComponent<BarUI>();
            bar.fill = fillImg; 
            bar.label = label; 
            return bar;
        }

        private HudMovable MakeHudMovable(RectTransform rt, string id)
        {
            var movable = rt.gameObject.AddComponent<HudMovable>();
            movable.Id = id;
            movable.HighlightInEdit = true;
            return movable;
        }

        private void EnsureHudComponents()
        {
            // Ensure HudEditMode exists on the canvas
            var hudEditMode = canvas.GetComponent<HudEditMode>();
            if (hudEditMode == null)
            {
                hudEditMode = canvas.gameObject.AddComponent<HudEditMode>();
                Debug.Log("[BlazedUIBootstrap] Added HudEditMode to canvas");
            }
        }

        private void BuildPlayerHUD()
        {
            // Calculate available space for bars using exposed configuration
            var availableWidth = hudWidth - 24f - rightPadding; // No portrait, just left/right padding
            var defaultBarWidth = availableWidth - 10f; 
            var barStartX = 12f; // Start from left edge with padding (no portrait)
            
            // Use custom widths if set, otherwise use calculated default
            var actualHPWidth = hpBarWidth > 0 ? hpBarWidth : defaultBarWidth;
            var actualMPWidth = mpBarWidth > 0 ? mpBarWidth : defaultBarWidth;
            
            Debug.Log($"[BlazedUIBootstrap] HUD Dimensions - Panel: {hudWidth}x{hudHeight}, Portrait: {portraitSize}x{portraitSize}, HP Bar: {actualHPWidth}x{hpBarHeight}, MP Bar: {actualMPWidth}x{mpBarHeight} at x={barStartX}");
            
            // Create a properly positioned player HUD panel without background (we'll add selective backgrounds)
            var root = PanelNoBackground(uiRoot, "PlayerHUD", new Vector2(hudWidth, hudHeight), new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(12, -12));
            
            // Detached background for ground/angle: separate movable panel (ensure behind all)
            // Text frame background anchored near bottom-right; user will place it precisely
            float textPad = 8f;
            float textHeight = hpBarHeight + mpBarHeight + 14f + 48f + 20f; // slightly taller vertically
            var textBG = Panel(uiRoot, "HUD_TextBackground",
                new Vector2(actualMPWidth + 2 * textPad + 16f, textHeight),
                new Vector2(1,0), new Vector2(1,0), new Vector2(1,0),
                new Vector2(-12 - (actualMPWidth + 2 * textPad + 16f), 12));
            textBG.SetSiblingIndex(0); // behind HUD elements
            MakeHudMovable(textBG, "HUD_TextBackground");

            // Portrait removed as requested - no portrait frame needed
            /*
            var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            var portraitRT = portraitGO.GetComponent<RectTransform>();
            portraitRT.SetParent(root, false);
            portraitRT.anchorMin = new Vector2(0, 0); 
            portraitRT.anchorMax = new Vector2(0, 1);
            portraitRT.pivot = new Vector2(0, 0.5f);
            portraitRT.sizeDelta = new Vector2(portraitSize, portraitSize);
            portraitRT.anchoredPosition = new Vector2(portraitPadding, 0);
            var portraitImg = portraitGO.GetComponent<Image>();
            // Start with a transparent placeholder - SPUMPortraitCapture will update this
            portraitImg.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            portraitImg.sprite = null; // Will be set by SPUMPortraitCapture
            */
            
            // Set portrait variables to null since we're not creating them
            RectTransform portraitRT = null;
            Image portraitImg = null;

            // HP bar positioned correctly to fit within available space
            var hpBarY = 45f; // From top of panel (moved down to make room for name/level)
            var mpBarY = hpBarY - hpBarHeight - barSpacing; // Below HP bar with spacing
            
            // Name and level text ABOVE the HP bar (same font/size as bottom labels)
            var nameText = MakeText(root, "PlayerName", 16, TextAnchor.UpperLeft, new Vector2(barStartX, -8), new Vector2(-rightPadding, hpBarY + 25), FontStyle.Bold);
            var levelText = MakeText(root, "Lv 1", 16, TextAnchor.UpperRight, new Vector2(barStartX, -8), new Vector2(-rightPadding, hpBarY + 25), FontStyle.Bold);
            var nameOutline = nameText.gameObject.AddComponent<Outline>(); nameOutline.effectColor = new Color(0,0,0,0.9f); nameOutline.effectDistance = new Vector2(1,-1);
            var lvlOutline = levelText.gameObject.AddComponent<Outline>(); lvlOutline.effectColor = new Color(0,0,0,0.9f); lvlOutline.effectDistance = new Vector2(1,-1);
            
            var hpBar = MakeBar(root, "HP", new Vector2(barStartX, hpBarY), new Vector2(actualHPWidth, hpBarHeight));
            hpBar.highColor = new Color(0.25f, 0.9f, 0.3f, 1f);
            hpBar.fill.color = hpBar.highColor;

            // MP bar positioned below HP bar
            var mpBar = MakeBar(root, "MP", new Vector2(barStartX, mpBarY), new Vector2(actualMPWidth, mpBarHeight));
            mpBar.highColor = new Color(0.2f, 0.55f, 0.95f, 1f);
            mpBar.fill.color = mpBar.highColor;

            // XP bar positioned below MP bar
            var xpBarY = mpBarY - mpBarHeight - barSpacing;
            var xpBar = MakeBar(root, "XP", new Vector2(barStartX, xpBarY), new Vector2(actualMPWidth, 14f));
            xpBar.highColor = new Color(0.85f, 0.75f, 0.2f, 1f);
            xpBar.fill.color = xpBar.highColor;

            // Separate movable info row: Gold (left), Coords (right)
            var infoRow = new GameObject("HUD_InfoRow", typeof(RectTransform)).GetComponent<RectTransform>();
            infoRow.SetParent(uiRoot, false);
            infoRow.anchorMin = new Vector2(0, 1); infoRow.anchorMax = new Vector2(0, 1); infoRow.pivot = new Vector2(0, 1);
            infoRow.sizeDelta = new Vector2(hudWidth - barStartX - rightPadding, 20);
            infoRow.anchoredPosition = new Vector2(12 + barStartX, -12 - hudHeight + 8);
            var gold = MakeText(infoRow, "Gold: 0", 16, TextAnchor.LowerLeft, new Vector2(0,0), new Vector2(-40,0), FontStyle.Bold);
            var goldOutline = gold.gameObject.AddComponent<Outline>(); goldOutline.effectColor = new Color(0,0,0,0.9f); goldOutline.effectDistance = new Vector2(1,-1);
            // Moved coordinates closer to gold - reduced right margin to move coords closer to gold
            var coords = MakeText(infoRow, "X:0 Y:0", 16, TextAnchor.LowerRight, new Vector2(0,0), new Vector2(-40,0), FontStyle.Bold);
            var coordsOutline = coords.gameObject.AddComponent<Outline>(); coordsOutline.effectColor = new Color(0,0,0,0.9f); coordsOutline.effectDistance = new Vector2(1,-1);

            playerHUD = root.gameObject.AddComponent<PlayerHUD>();
            // playerHUD.portrait = portraitImg; // Portrait removed 
            playerHUD.nameText = nameText; 
            playerHUD.levelText = levelText; 
            playerHUD.hpBar = hpBar; 
            playerHUD.mpBar = mpBar;
            playerHUD.xpBar = xpBar;
            playerHUD.goldText = gold;
            playerHUD.coordsText = coords;
            
            // Configure health bar with red colors
            if (hpBar != null)
            {
                hpBar.highColor = new Color(0.85f, 0.20f, 0.20f, 1f); // Bright red
                hpBar.lowColor = new Color(0.40f, 0.05f, 0.05f, 1f);  // Dark red
            }
            
            playerHUD.Configure("Hero", 1, 100, 100, 50, 50, null);
            
            // Connect to PlayerHealth system for real-time updates (with slight delay)
            StartCoroutine(DelayedHealthConnection());

            // Make individual components moveable
            // MakeHudMovable(portraitRT, "HUD_Portrait"); // Portrait removed
            MakeHudMovable(hpBar.transform as RectTransform, "HUD_HPBar");
            MakeHudMovable(mpBar.transform as RectTransform, "HUD_MPBar");
            MakeHudMovable(xpBar.transform as RectTransform, "HUD_XPBar");
            MakeHudMovable(infoRow, "HUD_InfoRow");
            
            // Make name/level independently movable (no shared container)
            MakeHudMovable(nameText.rectTransform, "HUD_NameLeft");
            MakeHudMovable(levelText.rectTransform, "HUD_LevelRight");
            
            // Also make the whole player HUD moveable for backward compatibility
            MakeHudMovable(root, "HUD_Player");
        }


        private void BuildGroupHUD()
        {
            // Improved positioning - left side, centered vertically
            var root = Panel(uiRoot, "GroupHUD", new Vector2(280, 320), new Vector2(0,0.5f), new Vector2(0,0.5f), new Vector2(0,0.5f), new Vector2(12, 0));
            var list = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            list.SetParent(root, false);
            list.anchorMin = new Vector2(0, 0); list.anchorMax = new Vector2(1, 1); list.offsetMin = new Vector2(6, 6); list.offsetMax = new Vector2(-6, -6);
            var vlg = list.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft; vlg.spacing = 4; vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
            list.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var entryGO = new GameObject("EntryTemplate", typeof(RectTransform));
            entryGO.transform.SetParent(list, false);
            entryGO.SetActive(false);
            var entryRT = entryGO.GetComponent<RectTransform>();
            entryRT.sizeDelta = new Vector2(0, 48);

            var nameText = MakeText(entryRT, "Member", 14, TextAnchor.UpperLeft, new Vector2(6, -4), new Vector2(-6, -26));
            var hp = MakeBar(entryRT, "HP", new Vector2(6, 24), new Vector2(-12, 16));
            hp.highColor = new Color(0.25f, 0.9f, 0.3f, 1f);
            hp.fill.color = hp.highColor;

            var entry = entryGO.AddComponent<GroupMemberEntry>();
            entry.nameText = nameText; entry.hpBar = hp;

            groupHUD = root.gameObject.AddComponent<GroupHUD>();
            groupHUD.listRoot = list; groupHUD.entryTemplate = entry;

            MakeHudMovable(root, "HUD_Group");
        }

        private void BuildInventory()
        {
            // Improved positioning - right side with better spacing
            var root = Panel(uiRoot, "Inventory", new Vector2(340, 370), new Vector2(1,0), new Vector2(1,0), new Vector2(1,0), new Vector2(-12, 12));
            var title = MakeText(root, "Inventory", 16, TextAnchor.UpperLeft, new Vector2(6, -4), new Vector2(-6, -26));
            var gridHost = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup)).GetComponent<RectTransform>();
            gridHost.SetParent(root, false);
            gridHost.anchorMin = new Vector2(0, 0); gridHost.anchorMax = new Vector2(1, 1); gridHost.offsetMin = new Vector2(6, 6); gridHost.offsetMax = new Vector2(-6, -32);
            var grid = gridHost.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(60, 60); grid.spacing = new Vector2(4, 4); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 5;

            var slot = new GameObject("SlotTemplate", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonSfx)).GetComponent<RectTransform>();
            slot.SetParent(gridHost, false);
            slot.gameObject.SetActive(false);
            slot.sizeDelta = new Vector2(60, 60);
            var slotImg = slot.GetComponent<Image>(); slotImg.color = new Color(1,1,1,0.08f);
            var btn = slot.GetComponent<Button>();

            var icon = MakeImage(slot, "Icon", new Vector2(4,4), new Vector2(-4,-4));
            var stack = MakeText(slot, "", 12, TextAnchor.LowerRight, new Vector2(4,4), new Vector2(-4,-4));

            var slotUI = slot.gameObject.AddComponent<InventorySlotUI>();
            slotUI.icon = icon; slotUI.frame = slotImg; slotUI.stackText = stack;

            inventoryUI = root.gameObject.AddComponent<InventoryUI>();
            inventoryUI.window = root; inventoryUI.grid = grid; inventoryUI.slotTemplate = slotUI;
            inventoryUI.columns = 5; inventoryUI.rows = 5;

            MakeHudMovable(root, "HUD_Inventory");
        }

        private void BuildActionBar()
        {
            // Better positioning - bottom center with improved spacing
            var root = Panel(uiRoot, "ActionBar", new Vector2(11 * 60 + 10 * 6 + 12, 78), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 12));
            var strip = new GameObject("Strip", typeof(RectTransform)).GetComponent<RectTransform>();
            strip.SetParent(root, false);
            strip.anchorMin = new Vector2(0.5f, 0.5f); strip.anchorMax = new Vector2(0.5f, 0.5f); strip.pivot = new Vector2(0.5f, 0.5f);
            strip.sizeDelta = new Vector2(11 * 60 + 10 * 6, 60);

            var slot = new GameObject("ActionSlotTemplate", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonSfx)).GetComponent<RectTransform>();
            slot.SetParent(strip, false); slot.gameObject.SetActive(false); slot.sizeDelta = new Vector2(60,60);
            var slotImg = slot.GetComponent<Image>(); slotImg.color = new Color(1,1,1,0.08f);
            var icon = MakeImage(slot, "Icon", new Vector2(4,4), new Vector2(-4,-4));
            var stack = MakeText(slot, "", 12, TextAnchor.LowerRight, new Vector2(4,4), new Vector2(-4,-4));
            var slotUI = slot.gameObject.AddComponent<InventorySlotUI>(); slotUI.icon = icon; slotUI.frame = slotImg; slotUI.stackText = stack;

            var hrt = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            hrt.childAlignment = TextAnchor.MiddleCenter; hrt.spacing = 6; hrt.childForceExpandHeight = false; hrt.childForceExpandWidth = false;

            var bar = root.gameObject.AddComponent<ActionBarUI>();
            bar.root = strip; bar.slotTemplate = slotUI;
            actionBar = bar;

            MakeHudMovable(root, "HUD_ActionBar");
        }

        private void BuildSettings()
        {
            var root = Panel(uiRoot, "Settings", new Vector2(560, 420), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0, 20));
            var title = MakeText(root, "Settings", 20, TextAnchor.UpperLeft, new Vector2(10, -2), new Vector2(-10, -30));

            if (FindObjectOfType<GameAudioManager>() == null)
            {
                new GameObject("GameAudioManager", typeof(GameAudioManager));
            }

            var tabsRow = new GameObject("Tabs", typeof(RectTransform)).GetComponent<RectTransform>();
            tabsRow.SetParent(root, false);
            // Top horizontal tabs row
            tabsRow.anchorMin = new Vector2(0,1); tabsRow.anchorMax = new Vector2(1,1); tabsRow.pivot = new Vector2(0,1);
            tabsRow.sizeDelta = new Vector2(0, 36); tabsRow.anchoredPosition = new Vector2(-12, -42);

            RectTransform CreateContentRoot(string name)
            {
                var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
                rt.SetParent(root, false);
                // Content area under tabs
                rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = new Vector2(12, 60);
                rt.offsetMax = new Vector2(-12, -60);
                return rt;
            }

            RectTransform generalRoot = CreateContentRoot("GeneralRoot");
            RectTransform soundRoot = CreateContentRoot("SoundRoot");
            RectTransform uiRootTab = CreateContentRoot("UIRoot");
            RectTransform displayRoot = CreateContentRoot("DisplayRoot");

            Slider CreateSlider(RectTransform parent)
            {
                var go = new GameObject("Slider", typeof(RectTransform));
                var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false);
                rt.anchorMin = new Vector2(0.45f, 0.2f); rt.anchorMax = new Vector2(0.98f, 0.8f); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                var bg = new GameObject("Background", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                bg.SetParent(rt, false); bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one; bg.offsetMin = Vector2.zero; bg.offsetMax = Vector2.zero; bg.GetComponent<Image>().color = new Color(1,1,1,0.08f);
                var fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
                fillArea.SetParent(rt, false); fillArea.anchorMin = new Vector2(0,0.25f); fillArea.anchorMax = new Vector2(1,0.75f); fillArea.offsetMin = new Vector2(8,0); fillArea.offsetMax = new Vector2(-16,0);
                var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                fill.SetParent(fillArea, false); fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero; var fillImg = fill.GetComponent<Image>(); fillImg.color = new Color(1,1,1,0.35f);
                var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                handle.SetParent(rt, false); handle.sizeDelta = new Vector2(16,16); handle.anchorMin = new Vector2(0,0.5f); handle.anchorMax = new Vector2(0,0.5f); handle.anchoredPosition = Vector2.zero; handle.GetComponent<Image>().color = new Color(1,1,1,0.9f);
                var slider = go.AddComponent<Slider>();
                slider.fillRect = fill; slider.handleRect = handle; slider.targetGraphic = handle.GetComponent<Image>();
                slider.direction = Slider.Direction.LeftToRight; slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0.6f;
                return slider;
            }

            RectTransform AddRow(RectTransform parent, string label, float y)
            {
                var row = new GameObject(label+"Row", typeof(RectTransform)).GetComponent<RectTransform>();
                row.SetParent(parent, false);
                row.anchorMin = new Vector2(0,1); row.anchorMax = new Vector2(1,1); row.pivot = new Vector2(0,1);
                row.sizeDelta = new Vector2(0, 36); row.anchoredPosition = new Vector2(0, -y);
                var l = MakeText(row, label, 14, TextAnchor.MiddleLeft, new Vector2(10,0), new Vector2(-10,0)); l.color = new Color(1,1,1,0.9f);
                return row;
            }

            var gRow = AddRow(generalRoot, "General", 10);
            // (Add general controls later as needed)

            var sRow = AddRow(soundRoot, "Master Volume", 10);
            var vol = CreateSlider(sRow);
            var mRow = AddRow(soundRoot, "Music Volume", 52);
            var music = CreateSlider(mRow);

            var uRow = AddRow(uiRootTab, "UI Scale", 10);
            var scale = CreateSlider(uRow);

            var dRow = AddRow(displayRoot, "Resolution", 10);
            var ddGO = new GameObject("Dropdown", typeof(RectTransform), typeof(Dropdown));
            var ddRT = ddGO.GetComponent<RectTransform>(); ddRT.SetParent(dRow, false);
            ddRT.anchorMin = new Vector2(0.45f, 0.2f); ddRT.anchorMax = new Vector2(0.98f, 0.8f); ddRT.offsetMin = Vector2.zero; ddRT.offsetMax = Vector2.zero; var dd = ddGO.GetComponent<Dropdown>();

            var fsRow = AddRow(displayRoot, "Fullscreen", 52);
            var tgGO = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
            var tgRT = tgGO.GetComponent<RectTransform>(); tgRT.SetParent(fsRow, false);
            tgRT.anchorMin = new Vector2(0.45f, 0.2f); tgRT.anchorMax = new Vector2(0.6f, 0.8f); tgRT.offsetMin = Vector2.zero; tgRT.offsetMax = Vector2.zero; var tg = tgGO.GetComponent<Toggle>(); tgGO.GetComponent<Image>().color = new Color(1,1,1,0.12f);

            var btnApply = MakeButton(root, new Vector2(-190, 12), new Vector2(160, 36), "Apply");
            var btnClose = MakeButton(root, new Vector2(-10, 12), new Vector2(160, 36), "Close");
            // Ensure buttons don't overlap the tabs: move them slightly right if needed
            var btnApplyRT = btnApply.GetComponent<RectTransform>();
            var btnCloseRT = btnClose.GetComponent<RectTransform>();
            btnApplyRT.anchorMin = new Vector2(1,0); btnApplyRT.anchorMax = new Vector2(1,0);
            btnCloseRT.anchorMin = new Vector2(1,0); btnCloseRT.anchorMax = new Vector2(1,0);

            Button CreateTab(string text, float x, System.Action onClick)
            {
                var go = new GameObject(text+"Tab", typeof(RectTransform), typeof(Image), typeof(Button));
                var rt = go.GetComponent<RectTransform>(); rt.SetParent(tabsRow, false);
                rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1);
                rt.sizeDelta = new Vector2(120, 28); rt.anchoredPosition = new Vector2(12 + x, -4);
                go.GetComponent<Image>().color = new Color(1,1,1,0.12f);
                var t = MakeText(rt, text, 14, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                var b = go.GetComponent<Button>(); b.onClick.AddListener(()=> onClick());
                return b;
            }

            void ShowTab(RectTransform show, params RectTransform[] hide)
            {
                show.gameObject.SetActive(true);
                foreach (var h in hide) if (h != null) h.gameObject.SetActive(false);
            }

            CreateTab("General", 0, ()=> ShowTab(generalRoot, soundRoot, uiRootTab, displayRoot));
            CreateTab("Sounds", 130, ()=> ShowTab(soundRoot, generalRoot, uiRootTab, displayRoot));
            CreateTab("UI", 260, ()=> ShowTab(uiRootTab, generalRoot, soundRoot, displayRoot));
            CreateTab("Display", 390, ()=> ShowTab(displayRoot, generalRoot, soundRoot, uiRootTab));
            ShowTab(generalRoot, soundRoot, uiRootTab, displayRoot);

            settingsPanel = root.gameObject.AddComponent<SettingsPanel>();
            settingsPanel.window = root; settingsPanel.musicVolume = music; settingsPanel.uiScale = scale; settingsPanel.resolutionDropdown = dd; settingsPanel.fullscreenToggle = tg; settingsPanel.applyButton = btnApply; settingsPanel.closeButton = btnClose; settingsPanel.uiRootToScale = uiRoot;

            MakeHudMovable(root, "HUD_Settings");
        }

        private Button MakeButton(RectTransform parent, Vector2 bottomRightOffset, Vector2 size, string text)
        {
            var go = new GameObject(text+"Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonSfx));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(1,0); rt.anchorMax = new Vector2(1,0); rt.pivot = new Vector2(1,0);
            rt.sizeDelta = size; rt.anchoredPosition = bottomRightOffset * new Vector2(1,1);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.08f);
            var t = MakeText(rt, text, 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            return go.GetComponent<Button>();
        }

        private void ConnectPlayerHealthToHUD()
        {
            // Find player and ensure PlayerHealth component exists
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Look for SPUM character
                SPUM_Prefabs spum = Object.FindFirstObjectByType<SPUM_Prefabs>();
                if (spum != null)
                {
                    player = spum.gameObject;
                    if (!player.CompareTag("Player"))
                    {
                        player.tag = "Player";
                    }
                }
            }

            if (player != null)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    playerHealth = player.AddComponent<PlayerHealth>();
                    Debug.Log("✅ Added PlayerHealth component to player");
                }

                // Subscribe to health change events
                playerHealth.OnHealthChanged += OnPlayerHealthChanged;
                Debug.Log("✅ Connected PlayerHealth to BlazedUI PlayerHUD");
                
                // Set initial health values
                OnPlayerHealthChanged(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find player to connect health system");
            }
        }

        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            if (playerHUD != null)
            {
                playerHUD.UpdateHealth(currentHealth, maxHealth);
                Debug.Log($"🏥 PlayerHUD health updated: {currentHealth}/{maxHealth}");
            }
        }

        private System.Collections.IEnumerator DelayedHealthConnection()
        {
            // Wait a frame to ensure UI is fully initialized
            yield return new WaitForEndOfFrame();
            ConnectPlayerHealthToHUD();
        }
    }
}