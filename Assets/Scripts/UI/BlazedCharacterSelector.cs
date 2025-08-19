using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Clean character selection system for Blazed Odyssey
/// No SPUM dependency - follows user's guidelines exactly
/// </summary>
public class BlazedCharacterSelector : MonoBehaviour
{
    [Header("UI References")]
    public Transform characterGridParent;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statusText;
    public Button generateAllButton;
    public Button backButton;
    
    [Header("Character Slot Prefab Setup")]
    public GameObject characterSlotPrefab; // Will create dynamically if null
    
    [Header("PixelLAB Settings")]
    public bool useRealPixelLAB = true; // Set to true when ready for real generation

    [Header("Display Settings")]
    public bool showGenerateControls = false; // Hide generation UI when using imported sprites
    public bool showGrid = false; // New: hide old grid; we'll use preview UI instead

    [Header("SPUM Integration (optional)")]
    public bool useSpumForPreview = true;
    public bool useSpumRegistryForClasses = true;
    public SpumUnitRegistry spumRegistry; // map className -> prefab

    // Preview UI state
    private BlazedCharacterClass currentClass;
    private Image previewImage;
    private RawImage spumPreviewImage;
    private TextMeshProUGUI previewCaption;
    private Button prevFrameButton;
    private Button nextFrameButton;
    private Button selectCurrentButton;
    private Transform classButtonsBar;
    private Transform raceButtonsBar;
    private Transform genderButtonsBar;
    private List<Sprite> currentPreviewFrames = new List<Sprite>();
    private int currentFrameIndex = 0;
    private readonly string[] dirTokens = new[] { "south", "east", "north", "west" };
    private int currentDirIndex = 0; // 0=south
    private string currentRace = "Hero";
    private bool currentIsMale = true;
    private readonly System.Collections.Generic.Dictionary<int, string[]> dirTokenAlts = new System.Collections.Generic.Dictionary<int, string[]>
    {
        { 0, new[] { "south", "down", "front", "s" } },
        { 1, new[] { "east", "right", "e" } },
        { 2, new[] { "north", "up", "back", "n" } },
        { 3, new[] { "west", "left", "w" } },
    };
    private readonly Dictionary<string, List<Sprite>> spritesByClass = new Dictionary<string, List<Sprite>>(System.StringComparer.OrdinalIgnoreCase);
    private readonly string[] walkTokens = new[] { "walk", "move", "run" };
    private readonly string[] idleTokens = new[] { "idle", "stand" };
    private float animFps = 8f;
    private float animTimer = 0f;
    private bool previewSizedOnce = false;
    private bool directionChangeCooldown = false;
    
    // UI interaction fixes
    private float lastButtonClickTime = -10f; // Initialize to allow immediate first click
    private const float BUTTON_DEBOUNCE_TIME = 0.5f;
    private const float CLASS_SELECTION_DEBOUNCE = 0.15f; // Reduced for better responsiveness

    // SPUM preview rig state
    private GameObject spumPreviewRig;
    private Camera spumPreviewCamera;
    private RenderTexture spumPreviewRT;
    private GameObject spumPreviewInstance;
    private Animator spumPreviewAnimator;

    // Info panel
    private TextMeshProUGUI infoHeader; // Name / Class / Level
    private TextMeshProUGUI infoBody;   // Description
    
    private List<CharacterSlot> characterSlots = new List<CharacterSlot>();
    private List<BlazedCharacterClass> allClasses;
    
    [System.Serializable]
    public class CharacterSlot
    {
        public GameObject slotObject;
        public Image portraitImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI raceText;
        public TextMeshProUGUI statsText;
        public Button generateButton;
        public Button selectButton;
        public BlazedCharacterClass characterClass;
        public CharacterPortraitLoader portraitLoader;
        public bool isMale;
    }
    
    private void Start()
    {
        CreateUI();
        LoadCharacterData();
        SetupButtons();
    }
    
    private void CreateUI()
    {
        Debug.Log("🎭 Creating Blazed Character Selection UI...");
        
        // Create main canvas if it doesn't exist
        if (GameObject.Find("Character Selection Canvas") == null)
        {
            CreateMainCanvas();
        }

        // Bind to existing UI in scene if present (handles prefabs/manual placement)
        TryBindExistingUI();
    }
    
    private void CreateMainCanvas()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("Character Selection Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f; // balanced scaling
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create background
        CreateBackground(canvasObj);
        
        // Create title
        CreateTitle(canvasObj);
        
        // Create status text (hidden)
        CreateStatus(canvasObj);
        
        // Create preview panel
        CreatePreviewPanel(canvasObj);
        // Create selection bars
        CreateSelectionBars(canvasObj);
        // Create buttons
        CreateButtons(canvasObj);
        
        Debug.Log("✅ Main canvas created successfully!");
    }
    
    private void CreateBackground(GameObject parent)
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(parent.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.07f, 0.08f, 0.16f, 1f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
    }
    
    private void CreateTitle(GameObject parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform, false);
        
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "BLAZED ODYSSEY - CHARACTER SELECTION";
        titleText.fontSize = 36;
        titleText.color = Color.yellow;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.90f);
        titleRect.anchorMax = new Vector2(0.9f, 0.98f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    private void CreateStatus(GameObject parent)
    {
        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(parent.transform, false);
        statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = string.Empty; // hidden banner
        statusText.fontSize = 1; // minimize
        statusText.color = new Color(0,0,0,0);
        statusText.alignment = TextAlignmentOptions.Center;
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 1.1f); // off-screen
        statusRect.anchorMax = new Vector2(0.5f, 1.1f);
        statusRect.sizeDelta = Vector2.zero;
    }
    
    private void CreateSelectionBars(GameObject parent)
    {
        // Race bar (top)
        GameObject raceBar = new GameObject("Race Bar");
        raceBar.transform.SetParent(parent.transform, false);
        var rh = raceBar.AddComponent<HorizontalLayoutGroup>();
        rh.spacing = 12; rh.childAlignment = TextAnchor.MiddleCenter;
        var rr = raceBar.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.08f, 0.86f);
        rr.anchorMax = new Vector2(0.92f, 0.92f);
        rr.offsetMin = Vector2.zero; rr.offsetMax = Vector2.zero;
        raceButtonsBar = raceBar.transform;

        // Gender bar (middle)
        GameObject gBar = new GameObject("Gender Bar");
        gBar.transform.SetParent(parent.transform, false);
        var gh = gBar.AddComponent<HorizontalLayoutGroup>();
        gh.spacing = 12; gh.childAlignment = TextAnchor.MiddleCenter;
        var gr = gBar.GetComponent<RectTransform>();
        gr.anchorMin = new Vector2(0.3f, 0.80f);
        gr.anchorMax = new Vector2(0.7f, 0.84f);
        gr.offsetMin = Vector2.zero; gr.offsetMax = Vector2.zero;
        genderButtonsBar = gBar.transform;
        CreateGenderButtons();

        // Class bar (below gender)
        GameObject classBar = new GameObject("Class Bar");
        classBar.transform.SetParent(parent.transform, false);
        var h = classBar.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 12; h.childAlignment = TextAnchor.MiddleCenter;
        var r = classBar.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.08f, 0.72f);
        r.anchorMax = new Vector2(0.92f, 0.78f);
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        classButtonsBar = classBar.transform;

        // Defer population until character data is loaded
    }

    // Minimal grid creator used when showGrid=true and code requests a grid
    private void CreateCharacterGrid(GameObject parent)
    {
        GameObject gridObj = new GameObject("Character Grid");
        gridObj.transform.SetParent(parent.transform, false);
        var rt = gridObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.15f);
        rt.anchorMax = new Vector2(0.95f, 0.75f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var layout = gridObj.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(260, 160);
        layout.spacing = new Vector2(10, 10);
        characterGridParent = rt;
    }

    private void CreatePreviewPanel(GameObject parent)
    {
        GameObject panel = new GameObject("Preview Panel");
        panel.transform.SetParent(parent.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f,0.09f,0.18f,0.9f);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.22f, 0.20f);
        rect.anchorMax = new Vector2(0.78f, 0.70f);
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

        // Left: preview image & caption
        GameObject imgObj = new GameObject("Preview Image");
        imgObj.transform.SetParent(panel.transform, false);
        previewImage = imgObj.AddComponent<Image>();
        previewImage.type = Image.Type.Simple;
        previewImage.preserveAspect = true;
        var ir = imgObj.GetComponent<RectTransform>();
        ir.anchorMin = new Vector2(0.35f, 0.5f);
        ir.anchorMax = new Vector2(0.35f, 0.5f);
        ir.sizeDelta = new Vector2(330, 330);

        // SPUM RawImage (off by default)
        GameObject rawObj = new GameObject("SPUM Preview RawImage");
        rawObj.transform.SetParent(panel.transform, false);
        spumPreviewImage = rawObj.AddComponent<RawImage>();
        var rr = rawObj.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.35f, 0.5f);
        rr.anchorMax = new Vector2(0.35f, 0.5f);
        rr.sizeDelta = new Vector2(330, 330);
        spumPreviewImage.gameObject.SetActive(false);

        GameObject capObj = new GameObject("Preview Caption");
        capObj.transform.SetParent(panel.transform, false);
        previewCaption = capObj.AddComponent<TextMeshProUGUI>();
        previewCaption.alignment = TextAlignmentOptions.Center;
        previewCaption.fontSize = 20; previewCaption.color = new Color(0.95f,0.95f,1f,1f);
        var cr = capObj.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.35f, 0.22f);
        cr.anchorMax = new Vector2(0.35f, 0.22f);
        cr.sizeDelta = new Vector2(280, 36);

        // Arrows removed per request

        // Gender buttons are now in Gender Bar

        // Right: info card
        GameObject infoObj = new GameObject("Info Panel");
        infoObj.transform.SetParent(panel.transform, false);
        var infoBg = infoObj.AddComponent<Image>();
        infoBg.color = new Color(0.10f, 0.11f, 0.22f, 0.95f);
        var irt = infoObj.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0.65f, 0.26f); irt.anchorMax = new Vector2(0.92f, 0.66f);
        irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;

        GameObject headerObj = new GameObject("Info Header");
        headerObj.transform.SetParent(infoObj.transform, false);
        infoHeader = headerObj.AddComponent<TextMeshProUGUI>();
        infoHeader.fontSize = 20; infoHeader.color = Color.white; infoHeader.alignment = TextAlignmentOptions.Left;
        var hrt = headerObj.GetComponent<RectTransform>(); hrt.anchorMin = new Vector2(0.05f, 0.75f); hrt.anchorMax = new Vector2(0.95f, 0.95f); hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;

        GameObject bodyObj = new GameObject("Info Body");
        bodyObj.transform.SetParent(infoObj.transform, false);
        infoBody = bodyObj.AddComponent<TextMeshProUGUI>();
        infoBody.fontSize = 16; infoBody.color = new Color(0.85f,0.88f,0.95f,1f); infoBody.alignment = TextAlignmentOptions.TopLeft;
        infoBody.textWrappingMode = TextWrappingModes.Normal;
        var brt = bodyObj.GetComponent<RectTransform>(); brt.anchorMin = new Vector2(0.05f, 0.10f); brt.anchorMax = new Vector2(0.95f, 0.70f); brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

        // Select button (below info)
        selectCurrentButton = CreateButton(panel, "Select", new Color(0.2f,0.6f,0.2f,1f));
        var srt = selectCurrentButton.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.78f, 0.18f); srt.anchorMax = new Vector2(0.88f, 0.22f); srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
        selectCurrentButton.onClick.AddListener(ConfirmSelection);

        // Initialize first class
        // Defer selection until classes are loaded from registry in LoadCharacterData()
    }

    private void CreateRaceButton(GameObject parent, string race)
    {
        var btn = CreateButton(parent, race, new Color(0.25f,0.25f,0.6f,1f));
        btn.onClick.AddListener(() => { currentRace = race; PopulateClassButtons(); });
    }

    private void PopulateClassButtons()
    {
        // Clear
        if (classButtonsBar == null) return;
        for (int i = classButtonsBar.childCount - 1; i >= 0; i--) Destroy(classButtonsBar.GetChild(i).gameObject);

        // Classes from registry or database, filtered by current race if available
        var classesAll = allClasses != null && allClasses.Count > 0 ? allClasses : BlazedCharacterDatabase.GetAllClasses();
        // Distinct by class+race to prevent duplicates
        classesAll = classesAll
            .GroupBy(c => (c.className + "|" + (c.race ?? "")).ToLowerInvariant())
            .Select(g => g.First())
            .ToList();
        List<BlazedCharacterClass> classes = classesAll;
        if (!string.IsNullOrWhiteSpace(currentRace))
            classes = classesAll.FindAll(c => string.Equals(c.race, currentRace, System.StringComparison.OrdinalIgnoreCase));
        // Do not auto-select any class; wait for user click
        currentClass = null;
        ClearPreviewUI();
        foreach (var c in classes)
        {
            var b = CreateButton(classButtonsBar.gameObject, c.className, new Color(0.3f,0.3f,0.5f,1f));
            var captured = c;
            b.onClick.AddListener(() => { 
                // Prevent rapid class switching that causes UI issues
                if (Time.time - lastButtonClickTime < CLASS_SELECTION_DEBOUNCE) 
                {
                    Debug.Log("🚫 Class selection ignored - too soon after last click");
                    return;
                }
                lastButtonClickTime = Time.time;
                
                currentClass = captured; 
                currentRace = captured.race; 
                ClearPreviewUI(); 
                RefreshPreview(); 
                Debug.Log($"🎭 Class selected: {captured.className}");
            });
        }
        // No preview until a class button is clicked
    }

    private void PopulateRaceButtons()
    {
        if (raceButtonsBar == null) return;
        for (int i = raceButtonsBar.childCount - 1; i >= 0; i--) Destroy(raceButtonsBar.GetChild(i).gameObject);
        var source = allClasses != null && allClasses.Count > 0 ? allClasses : BlazedCharacterDatabase.GetAllClasses();
        var races = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < source.Count; i++)
        {
            var r = source[i].race;
            if (!string.IsNullOrWhiteSpace(r)) races.Add(r);
        }
        // Ensure there is at least one race
        if (races.Count == 0) races.Add("Hero");
        // Pick current if not set
        if (string.IsNullOrWhiteSpace(currentRace)) currentRace = races.FirstOrDefault();
        foreach (var race in races)
        {
            var b = CreateButton(raceButtonsBar.gameObject, race, new Color(0.25f,0.25f,0.6f,1f));
            var capturedRace = race;
            b.onClick.AddListener(() => { currentRace = capturedRace; currentClass = null; ClearPreviewUI(); PopulateClassButtons(); });
        }
    }

    private void CreateGenderButton(GameObject parent, string label, bool isMale)
    {
        var btn = CreateButton(parent, label, new Color(0.25f,0.5f,0.25f,1f));
        btn.onClick.AddListener(() => { 
            if (Time.time - lastButtonClickTime < 0.2f) return; // Prevent rapid gender switching
            currentIsMale = isMale; 
            RefreshPreview(); 
            Debug.Log($"🚻 Gender changed to: {(isMale ? "Male" : "Female")}");
        });
    }

    private void RefreshPreview()
    {
        if (currentClass == null) { ClearPreviewUI(); return; }
        // Ensure previous prefab preview is cleared before building a new one
        DestroySpumPreview();
        previewCaption.text = currentClass.className;
        infoHeader.text = $"Name: {currentClass.className}\nClass: {currentClass.className}\nLevel: 1";
        infoBody.text = currentClass.description;
        currentRace = string.IsNullOrEmpty(currentClass.race) ? currentRace : currentClass.race;
        currentDirIndex = 0; // always start facing south
        currentPreviewFrames.Clear(); 
        currentFrameIndex = 0;
        
        // PRIORITY 1: SPUM prefab preview (if enabled and available)
        if (useSpumForPreview && spumRegistry != null && spumRegistry.TryGetPrefabByClassRaceGender(currentClass.className, currentRace, currentIsMale, out var spumPrefab))
        {
            Debug.Log($"🎭 Using SPUM preview for {currentClass.className}");
            ShowSpumPreview(spumPrefab);
            if (selectCurrentButton != null) selectCurrentButton.interactable = true;
            return; // SPUM preview active; skip sprite path
        }

        DestroySpumPreview();
        // PRIORITY 2: Fallback to old sprite system (only if SPUM failed)
        if (currentPreviewFrames.Count == 0)
        {
            Debug.Log($"🖼️ Falling back to sprite sheet preview for {currentClass.className}");
            var all = GetAllSpritesForClass(currentClass.className);
            if (all.Count > 0)
            {
                currentPreviewFrames = BuildDirectionalFramesFromRows(all, 0);
            }
        }
        
        previewSizedOnce = false;
        ApplyPreviewFrame();
        if (selectCurrentButton != null) selectCurrentButton.interactable = HasActivePreview();
    }

    private void ApplyPreviewFrame()
    {
        // When SPUM preview active, just ensure RawImage is visible
        if (spumPreviewImage != null && spumPreviewImage.gameObject.activeSelf)
        {
            if (previewImage != null) previewImage.gameObject.SetActive(false);
            return;
        }
        // Sprite flow
        if (currentPreviewFrames.Count == 0)
        {
            previewImage.sprite = null;
            return;
        }
        if (currentFrameIndex < 0) currentFrameIndex = 0;
        if (currentFrameIndex >= currentPreviewFrames.Count) currentFrameIndex = currentPreviewFrames.Count - 1;
        previewImage.sprite = currentPreviewFrames[currentFrameIndex];
        if (!previewSizedOnce)
        {
            ResizePreviewToIntegerScale();
            previewSizedOnce = true;
        }
    }

    private void Update()
    {
        if (previewImage == null) return;
        if (currentPreviewFrames == null || currentPreviewFrames.Count <= 1) return;
        animTimer += Time.unscaledDeltaTime;
        float frameTime = 1f / Mathf.Max(1f, animFps);
        if (animTimer >= frameTime)
        {
            animTimer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % currentPreviewFrames.Count;
            // Only swap the sprite; avoid rescaling every frame
            var sprite = currentPreviewFrames[currentFrameIndex];
            if (sprite != null) previewImage.sprite = sprite;
        }
    }

    private void ResizePreviewToIntegerScale()
    {
        if (previewImage == null || previewImage.sprite == null) return;
        var parentRect = (RectTransform)previewImage.transform.parent;
        float maxWidth = parentRect.rect.width * 0.8f;  // leave padding
        float maxHeight = parentRect.rect.height * 0.7f;
        // Sprite pixel size
        float sprW = previewImage.sprite.rect.width;
        float sprH = previewImage.sprite.rect.height;
        if (sprW <= 0 || sprH <= 0) return;
        float limit = 384f; // increased cap to allow larger preview
        maxWidth = Mathf.Min(maxWidth, limit);
        maxHeight = Mathf.Min(maxHeight, limit);
        int scale = Mathf.FloorToInt(Mathf.Min(maxWidth / sprW, maxHeight / sprH));
        if (scale < 1) scale = 1;
        var rt = (RectTransform)previewImage.transform;
        rt.sizeDelta = new Vector2(sprW * scale, sprH * scale);
    }

    private Button CreateTransparentHitArea(GameObject parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject area = new GameObject("HitArea");
        area.transform.SetParent(parent.transform, false);
        var img = area.AddComponent<Image>();
        img.color = new Color(0,0,0,0); // invisible
        var rt = area.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var btn = area.AddComponent<Button>();
        ColorBlock cb = btn.colors; cb.highlightedColor = new Color(1,1,1,0.03f); cb.pressedColor = new Color(1,1,1,0.05f); btn.colors = cb;
        return btn;
    }

    private void PrevPreviewSprite()
    {
        if (directionChangeCooldown) return;
        if (!HasActivePreview()) return;
        directionChangeCooldown = true;
        // Rotate direction to the left (south->east->north->west) once, no auto-spin
        currentDirIndex = (currentDirIndex + 3) % 4;
        SetDirectionFrames();
        StartCoroutine(ReleaseDirCooldown());
    }

    private void NextPreviewSprite()
    {
        if (directionChangeCooldown) return;
        if (!HasActivePreview()) return;
        directionChangeCooldown = true;
        // Rotate direction to the right once, no auto-spin
        currentDirIndex = (currentDirIndex + 1) % 4;
        SetDirectionFrames();
        StartCoroutine(ReleaseDirCooldown());
    }

    private IEnumerator ReleaseDirCooldown()
    {
        yield return new WaitForSeconds(0.15f);
        directionChangeCooldown = false;
    }

    private void SetDirectionFrames()
    {
        if (currentClass == null) return;
        string[] tokens = dirTokenAlts[currentDirIndex];
        currentPreviewFrames.Clear();
        
        // PRIORITY 1: Try SPUM prefab first
        if (useSpumForPreview && spumRegistry != null && spumRegistry.TryGetPrefabByClassRaceGender(currentClass.className, currentRace, currentIsMale, out var spumPrefab))
        {
            // If SPUM preview is active, nothing to change here (camera handles it). Just return
            if (spumPreviewImage != null && spumPreviewImage.gameObject.activeSelf) { UpdateSpumFacingFromDir(); ApplyPreviewFrame(); return; }
        }
        
        // PRIORITY 2: Fallback to old sprite system
        if (currentPreviewFrames.Count == 0)
        {
            var all = GetAllSpritesForClass(currentClass.className);
            currentPreviewFrames = FilterFramesByDirectionAndMotion(all, tokens, true);
            if (currentPreviewFrames.Count == 0)
                currentPreviewFrames = FilterFramesByDirectionAndMotion(all, tokens, false);
            if (currentPreviewFrames.Count == 0)
                currentPreviewFrames = BuildDirectionalFramesFromRows(all, currentDirIndex);
        }
        
        currentFrameIndex = 0;
        previewSizedOnce = false;
        ApplyPreviewFrame();
    }

    private List<Sprite> GetAllSpritesForClass(string className)
    {
        if (string.IsNullOrEmpty(className)) return new List<Sprite>();
        if (spritesByClass.TryGetValue(className, out var cached) && cached != null && cached.Count > 0)
            return cached;

        List<Sprite> result = new List<Sprite>();

        // STRICT: Use only the walk.png inside the class folder if present
        var walkFrames = LoadWalkFrames(className);
        if (walkFrames.Count > 0)
        {
            spritesByClass[className] = walkFrames;
            return walkFrames;
        }

        // PREFERRED: Use class folder and pick WALK first, then IDLE; ignore full-sheet/reference files
        string classFolder = $"Characters/{className}";
        // Walk sprites first
        TryAddMatching(result, Resources.LoadAll<Sprite>(classFolder), className, preferTokens: walkTokens, excludeBadSheets: true);
        if (result.Count == 0)
            TryAddMatchingSliced(result, Resources.LoadAll<Texture2D>(classFolder), className, preferTokens: walkTokens, excludeBadSheets: true);
        // Idle next
        if (result.Count == 0)
            TryAddMatching(result, Resources.LoadAll<Sprite>(classFolder), className, preferTokens: idleTokens, excludeBadSheets: true);
        if (result.Count == 0)
            TryAddMatchingSliced(result, Resources.LoadAll<Texture2D>(classFolder), className, preferTokens: idleTokens, excludeBadSheets: true);

        // 1) Expected folder structure
        string folder = $"Characters/New Characters/Character sprites/{className}";
        if (result.Count == 0)
            TryAddSpritesFiltered(result, Resources.LoadAll<Sprite>(folder));
        // Prefer actual individual sprites; slice only if none found
        if (result.Count == 0)
            TryAddTextureSlicedSprites(result, Resources.LoadAll<Texture2D>(folder));

        // 2) Common sheet name inside the class folder
        if (result.Count == 0)
            TryAddSpritesFiltered(result, Resources.LoadAll<Sprite>($"{folder}/{className}"));
        if (result.Count == 0)
            TryAddTextureSlicedSprites(result, Resources.LoadAll<Texture2D>($"{folder}/{className}"));
        // 2b) Alternate simple location: Resources/Characters/<ClassName>
        if (result.Count == 0)
            TryAddSpritesFiltered(result, Resources.LoadAll<Sprite>($"Characters/{className}"));
        if (result.Count == 0)
            TryAddTextureSlicedSprites(result, Resources.LoadAll<Texture2D>($"Characters/{className}"));

        // 3) Any sprite in the Character sprites folder that contains the class name
        if (result.Count == 0)
        {
            var pool = Resources.LoadAll<Sprite>("Characters/New Characters/Character sprites");
            TryAddMatching(result, pool, className, null, true);
            if (result.Count == 0)
            {
                var tpool = Resources.LoadAll<Texture2D>("Characters/New Characters/Character sprites");
                TryAddMatchingSliced(result, tpool, className, null, true);
            }
        }
        // 3b) Any sprite in the Characters root that contains the class name (user placed here)
        if (result.Count == 0)
        {
            var pool = Resources.LoadAll<Sprite>("Characters");
            TryAddMatching(result, pool, className, null, true);
            if (result.Count == 0)
            {
                var tpool = Resources.LoadAll<Texture2D>("Characters");
                TryAddMatchingSliced(result, tpool, className, null, true);
            }
        }

        // 4) As a last resort, scan all Resources sprites and match by name
        if (result.Count == 0)
        {
            var allSprites = Resources.LoadAll<Sprite>(string.Empty);
            TryAddMatching(result, allSprites, className, null, true);
            if (result.Count == 0)
            {
                var allTextures = Resources.LoadAll<Texture2D>(string.Empty);
                TryAddMatchingSliced(result, allTextures, className, null, true);
            }
        }

        spritesByClass[className] = result;
        return result;
    }

    private List<Sprite> LoadWalkFrames(string className)
    {
        List<Sprite> frames = new List<Sprite>();
        string classFolder = $"Characters/{className}";
        // Try exact: <ClassName>_walk
        var tex = Resources.Load<Texture2D>($"{classFolder}/{className}_walk");
        if (tex == null)
            tex = Resources.Load<Texture2D>($"{classFolder}/{className.ToLowerInvariant()}_walk");
        if (tex == null)
        {
            // Pick first *walk* texture in folder
            var allTex = Resources.LoadAll<Texture2D>(classFolder);
            for (int i = 0; i < allTex.Length; i++)
            {
                var t = allTex[i]; if (t == null) continue;
                var n = t.name.ToLowerInvariant();
                if (IsBadSheetName(n)) continue;
                if (n.Contains("walk")) { tex = t; break; }
            }
        }
        if (tex != null)
        {
            tex.filterMode = FilterMode.Point;
            frames = SliceTextureIntoSpritesAuto(tex);
        }
        return frames;
    }

    private void TryAddSpritesFiltered(List<Sprite> list, Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;
        for (int i = 0; i < sprites.Length; i++)
        {
            var s = sprites[i]; if (s == null) continue;
            if (IsBadSheetName(s.name)) continue;
            list.Add(s);
        }
    }

    private void TryAddMatching(List<Sprite> list, Sprite[] sprites, string className, string[] preferTokens = null, bool excludeBadSheets = false)
    {
        if (sprites == null || sprites.Length == 0) return;
        string needle = className.ToLowerInvariant();
        for (int i = 0; i < sprites.Length; i++)
        {
            var s = sprites[i]; if (s == null) continue;
            if (excludeBadSheets && IsBadSheetName(s.name)) continue;
            if (s.name.ToLowerInvariant().Contains(needle)) list.Add(s);
        }
        if (preferTokens != null && list.Count > 0)
        {
            // Keep only those matching tokens if present
            var filtered = new List<Sprite>();
            for (int i = 0; i < list.Count; i++)
            {
                var n = list[i].name.ToLowerInvariant();
                if (ContainsAny(n, preferTokens)) filtered.Add(list[i]);
            }
            if (filtered.Count > 0)
            {
                list.Clear();
                list.AddRange(filtered);
            }
        }
    }

    private void TryAddTextureSlicedSprites(List<Sprite> list, Texture2D[] textures)
    {
        if (textures == null || textures.Length == 0) return;
        for (int i = 0; i < textures.Length; i++)
        {
            var t = textures[i]; if (t == null) continue;
            t.filterMode = FilterMode.Point;
            var sliced = SliceTextureIntoSpritesAuto(t);
            for (int j = 0; j < sliced.Count; j++) list.Add(sliced[j]);
        }
    }

    private void TryAddMatchingSliced(List<Sprite> list, Texture2D[] textures, string className, string[] preferTokens = null, bool excludeBadSheets = false)
    {
        if (textures == null || textures.Length == 0) return;
        string needle = className.ToLowerInvariant();
        for (int i = 0; i < textures.Length; i++)
        {
            var t = textures[i]; if (t == null) continue;
            if (!t.name.ToLowerInvariant().Contains(needle)) continue;
            if (excludeBadSheets && IsBadSheetName(t.name)) continue;
            t.filterMode = FilterMode.Point;
            var sliced = SliceTextureIntoSpritesAuto(t);
            for (int j = 0; j < sliced.Count; j++) list.Add(sliced[j]);
        }
        if (preferTokens != null && list.Count > 0)
        {
            var filtered = new List<Sprite>();
            for (int i = 0; i < list.Count; i++)
            {
                var n = list[i].name.ToLowerInvariant();
                if (ContainsAny(n, preferTokens)) filtered.Add(list[i]);
            }
            if (filtered.Count > 0)
            {
                list.Clear();
                list.AddRange(filtered);
            }
        }
    }

    private bool IsBadSheetName(string name)
    {
        string n = name.ToLowerInvariant();
        return n.Contains("full") || n.Contains("sheet") || n.Contains("reference");
    }

    private List<Sprite> SliceTextureIntoSprites(Texture2D tex, int frameW, int frameH)
    {
        List<Sprite> frames = new List<Sprite>();
        if (tex == null || frameW <= 0 || frameH <= 0) return frames;
        int cols = Mathf.Max(1, tex.width / frameW);
        int rows = Mathf.Max(1, tex.height / frameH);
        for (int ry = 0; ry < rows; ry++)
        {
            for (int cx = 0; cx < cols; cx++)
            {
                int x = cx * frameW;
                int y = ry * frameH;
                if (x + frameW > tex.width || y + frameH > tex.height) continue;
                var rect = new Rect(x, y, frameW, frameH);
                var sp = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 64f);
                sp.name = tex.name + $"_r{ry}_c{cx}";
                frames.Add(sp);
            }
        }
        // Sort bottom-to-top, left-to-right so row 0 = bottom row
        frames.Sort((a, b) =>
        {
            int ay = Mathf.FloorToInt(a.rect.y);
            int by = Mathf.FloorToInt(b.rect.y);
            if (ay != by) return ay.CompareTo(by);
            return Mathf.FloorToInt(a.rect.x).CompareTo(Mathf.FloorToInt(b.rect.x));
        });
        return frames;
    }

    private List<Sprite> SliceTextureIntoSpritesAuto(Texture2D tex)
    {
        // Auto-detect square frame size. Prefer 4-direction rows.
        if (tex == null) return new List<Sprite>();
        int frameH = tex.height / 4; // assume 4 directions stacked vertically
        if (frameH <= 0 || tex.height % 4 != 0) frameH = GuessSquareCellSize(tex.width, tex.height);
        int frameW = frameH;
        if (frameW <= 0) frameW = frameH = 64; // fallback
        return SliceTextureIntoSprites(tex, frameW, frameH);
    }

    private int GuessSquareCellSize(int width, int height)
    {
        // Try common pixel sizes
        int[] candidates = new[] { 48, 64, 32, 96, 72, 144 };
        for (int i = 0; i < candidates.Length; i++)
        {
            int c = candidates[i];
            if (c > 0 && width % c == 0 && height % c == 0) return c;
        }
        // Fallback to GCD of width and height within reasonable bounds
        int a = width, b = height;
        while (b != 0) { int t = a % b; a = b; b = t; }
        int gcd = Mathf.Max(1, a);
        if (gcd >= 16 && gcd <= 256) return gcd;
        return 64;
    }

    private List<Sprite> BuildDirectionalFramesFromRows(List<Sprite> all, int dirIndex)
    {
        List<Sprite> frames = new List<Sprite>();
        if (all == null || all.Count == 0) return frames;
        // Group by row using 64px height heuristic
        Dictionary<int, List<Sprite>> byRow = new Dictionary<int, List<Sprite>>();
        int cellH = Mathf.Max(1, Mathf.RoundToInt(all[0].rect.height));
        for (int i = 0; i < all.Count; i++)
        {
            var s = all[i];
            int row = Mathf.FloorToInt(s.rect.y / (float)cellH);
            if (!byRow.TryGetValue(row, out var list)) { list = new List<Sprite>(); byRow[row] = list; }
            list.Add(s);
        }
        var rowIndices = new List<int>(byRow.Keys);
        rowIndices.Sort(); // bottom to top
        if (rowIndices.Count >= 4)
        {
            // Map: 0=south, 1=east, 2=north, 3=west
            // In many Franuka sheets the order top->bottom is South, East, North, West
            // We sorted bottom->top, so pick from the end for south-first mapping
            int pick = Mathf.Clamp(rowIndices.Count - 1 - dirIndex, 0, rowIndices.Count - 1);
            int mappedRow = rowIndices[pick];
            var rowFrames = byRow[mappedRow];
            rowFrames.Sort((a, b) => a.rect.x.CompareTo(b.rect.x));
            frames.AddRange(rowFrames);
        }
        else
        {
            // Fallback: split evenly into 4 chunks
            int per = Mathf.Max(1, all.Count / 4);
            int start = Mathf.Clamp(dirIndex * per, 0, Mathf.Max(0, all.Count - 1));
            for (int i = start; i < Mathf.Min(all.Count, start + per); i++) frames.Add(all[i]);
            if (frames.Count == 0) frames.AddRange(all);
        }
        return frames;
    }

    private List<Sprite> FilterFramesByDirectionAndMotion(List<Sprite> all, string[] directionTokens, bool preferWalk)
    {
        List<Sprite> frames = new List<Sprite>();
        if (all == null || all.Count == 0) return frames;
        var primaryMotion = preferWalk ? walkTokens : idleTokens;
        // First: direction + primary motion
        for (int i = 0; i < all.Count; i++)
        {
            var s = all[i]; if (s == null) continue;
            string n = s.name.ToLowerInvariant();
            if (!ContainsAny(n, directionTokens)) continue;
            if (ContainsAny(n, primaryMotion)) frames.Add(s);
        }
        if (frames.Count == 0)
        {
            // Second: direction only
            for (int i = 0; i < all.Count; i++)
            {
                var s = all[i]; if (s == null) continue;
                string n = s.name.ToLowerInvariant();
                if (ContainsAny(n, directionTokens)) frames.Add(s);
            }
        }
        SortFramesByNameNumber(frames);
        return frames;
    }

    private bool ContainsAny(string text, string[] tokens)
    {
        for (int i = 0; i < tokens.Length; i++) if (text.Contains(tokens[i])) return true;
        return false;
    }

    private void SortFramesByNameNumber(List<Sprite> frames)
    {
        frames.Sort((a, b) => ExtractTrailingNumber(a.name).CompareTo(ExtractTrailingNumber(b.name)));
    }

    private int ExtractTrailingNumber(string name)
    {
        int number = 0, mul = 1;
        for (int i = name.Length - 1; i >= 0; i--)
        {
            char c = name[i];
            if (c < '0' || c > '9') break;
            number += (c - '0') * mul;
            mul *= 10;
        }
        return number;
    }

    private void ConfirmSelection()
    {
        // Prevent double-clicks and rapid button presses
        if (Time.time - lastButtonClickTime < BUTTON_DEBOUNCE_TIME) 
        {
            Debug.Log("🚫 Button click ignored - too soon after last click");
            return;
        }
        lastButtonClickTime = Time.time;
        
        Debug.Log($"🎮 Selected {currentClass.className}");
        Debug.Log($"🚻 Gender selection: currentIsMale = {currentIsMale}");
        SelectedCharacter.Race = string.IsNullOrEmpty(currentClass.race) ? currentRace : currentClass.race;
        SelectedCharacter.ClassName = currentClass.className;
        SelectedCharacter.IsMale = currentIsMale;
        SelectedCharacter.CharacterName = currentClass.className;
        Debug.Log($"✅ SelectedCharacter.IsMale set to: {SelectedCharacter.IsMale}");
        // Save SPUM prefab name if mapped
        if (spumRegistry != null && spumRegistry.TryGetPrefabByClassRaceGender(currentClass.className, SelectedCharacter.Race, currentIsMale, out var spumPrefab))
        {
            SelectedCharacter.SpumPrefabName = spumPrefab.name; // under Resources/Units
        }
        else
        {
            SelectedCharacter.SpumPrefabName = null;
        }
        // Clean up preview resources to avoid render textures bleeding into next scene
        CleanupPreviewResources();
        // Also destroy this UI root to avoid DontDestroyOnLoad artifacts
        Destroy(this.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("StarterMapScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    private void CreateButtons(GameObject parent)
    {
        GameObject buttonPanelObj = new GameObject("Button Panel");
        buttonPanelObj.transform.SetParent(parent.transform, false);
        
        HorizontalLayoutGroup buttonLayout = buttonPanelObj.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 30;
        buttonLayout.childAlignment = TextAnchor.MiddleCenter;
        
        RectTransform buttonPanelRect = buttonPanelObj.GetComponent<RectTransform>();
        buttonPanelRect.anchorMin = new Vector2(0.2f, 0.05f);
        buttonPanelRect.anchorMax = new Vector2(0.8f, 0.12f);
        buttonPanelRect.offsetMin = Vector2.zero;
        buttonPanelRect.offsetMax = Vector2.zero;
        
        // Generate All button
        generateAllButton = CreateButton(buttonPanelObj, "🎨 GENERATE ALL SPRITES", new Color(0.2f, 0.7f, 0.2f, 1f));
        
        // Back button
        backButton = CreateButton(buttonPanelObj, "BACK TO MENU", new Color(0.7f, 0.2f, 0.2f, 1f));
    }

    private void TryBindExistingUI()
    {
        var canvasObj = GameObject.Find("Character Selection Canvas");
        if (canvasObj == null) return;
        
        // Character Grid
        if (characterGridParent == null)
        {
            var grid = FindDeepChild(canvasObj.transform, "Character Grid");
            if (grid != null)
            {
                characterGridParent = grid.GetComponent<RectTransform>();
            }
        }
        
        // Title and Status
        if (titleText == null)
        {
            var title = FindDeepChild(canvasObj.transform, "Title");
            if (title != null) titleText = title.GetComponent<TextMeshProUGUI>();
        }
        if (statusText == null)
        {
            var status = FindDeepChild(canvasObj.transform, "Status");
            if (status != null) statusText = status.GetComponent<TextMeshProUGUI>();
        }
        
        // Buttons (attempt to match by label text)
        if (generateAllButton == null || backButton == null)
        {
            var panel = FindDeepChild(canvasObj.transform, "Button Panel");
            if (panel != null)
            {
                var buttons = panel.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    var textComp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (textComp == null) continue;
                    if (generateAllButton == null && textComp.text.Contains("GENERATE ALL", System.StringComparison.OrdinalIgnoreCase))
                        generateAllButton = b;
                    else if (backButton == null && textComp.text.Contains("BACK", System.StringComparison.OrdinalIgnoreCase))
                        backButton = b;
                }
            }
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
    
    private Button CreateButton(GameObject parent, string text, Color color)
    {
        GameObject btnObj = new GameObject($"Button_{text}");
        btnObj.transform.SetParent(parent.transform, false);
        
        Button button = btnObj.AddComponent<Button>();
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(250, 50);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 16;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }

    // Creates a simple 200x200 grass tile sprite procedurally for the preview background
    private Sprite CreateGrassTileSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color baseGreen = new Color(0.25f, 0.6f, 0.25f, 1f);
        Color lightGreen = new Color(0.3f, 0.7f, 0.3f, 1f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Subtle checker for texture
                bool alt = ((x / 8) + (y / 8)) % 2 == 0;
                tex.SetPixel(x, y, alt ? baseGreen : lightGreen);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
    
    private void LoadCharacterData()
    {
        if (useSpumRegistryForClasses && spumRegistry != null && spumRegistry.entries != null && spumRegistry.entries.Length > 0)
        {
            allClasses = BuildClassesFromRegistry(spumRegistry);
            Debug.Log($"✅ Loaded {allClasses.Count} SPUM classes from registry");
            PopulateRaceButtons();
            // Ensure a valid currentRace from races list
            if (string.IsNullOrWhiteSpace(currentRace))
            {
                var source = allClasses ?? new List<BlazedCharacterClass>();
                currentRace = source.Select(c => c.race).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r)) ?? "";
            }
            PopulateClassButtons();
        }
        else
        {
            allClasses = BlazedCharacterDatabase.GetAllClasses();
            Debug.Log($"✅ Loaded {allClasses.Count} character classes");
            // Avoid showing default DB classes when registry is intended; only populate if registry not used
            if (!useSpumRegistryForClasses)
                PopulateClassButtons();
        }
    }

    private List<BlazedCharacterClass> BuildClassesFromRegistry(SpumUnitRegistry registry)
    {
        var list = new List<BlazedCharacterClass>();
        foreach (var e in registry.entries)
        {
            if (e == null) continue;
            if (string.IsNullOrWhiteSpace(e.className) && string.IsNullOrWhiteSpace(e.prefabName)) continue;
            var cls = new BlazedCharacterClass
            {
                className = string.IsNullOrWhiteSpace(e.className) ? e.prefabName : e.className,
                race = string.IsNullOrWhiteSpace(e.race) ? "Hero" : e.race,
                description = e.description,
                visualCues = string.Empty,
                primaryStat = string.IsNullOrWhiteSpace(e.primaryStat) ? "Attack" : e.primaryStat,
                health = e.health,
                mana = e.mana,
                attack = e.attack,
                defense = e.defense,
                magic = e.magic,
                classColor = e.classColor
            };
            list.Add(cls);
        }
        return list;
    }
    
    private void CreateCharacterSlots()
    {
        if (characterGridParent == null)
        {
            // Try to bind to existing UI or create a grid under existing canvas
            TryBindExistingUI();
            if (characterGridParent == null)
            {
                var canvasObj = GameObject.Find("Character Selection Canvas");
                if (!showGrid)
                {
                    // grid disabled by layout settings
                    return;
                }
                if (canvasObj != null && canvasObj.transform.Find("Character Grid") == null)
                {
                    CreateCharacterGrid(canvasObj);
                    TryBindExistingUI();
                }
            }
            if (characterGridParent == null)
            {
                Debug.LogError("❌ Character grid parent is null!");
                return;
            }
        }
        
        foreach (var charClass in allClasses)
        {
            // Create male and female entries per class
            CreateCharacterSlot(charClass, true);
            CreateCharacterSlot(charClass, false);
        }
        
        Debug.Log($"✅ Created {characterSlots.Count} character slots");
    }
    
    private void CreateCharacterSlot(BlazedCharacterClass charClass, bool isMale)
    {
        // Create main slot container
        string genderLabel = isMale ? "Male" : "Female";
        GameObject slotObj = new GameObject($"Slot_{charClass.race}_{charClass.className}_{genderLabel}");
        slotObj.transform.SetParent(characterGridParent, false);
        
        // Add background
        Image slotBg = slotObj.AddComponent<Image>();
        slotBg.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        
        // Portrait area
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(slotObj.transform, false);
        
        Image portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = charClass.classColor;
        
        RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.1f, 0.5f);
        portraitRect.anchorMax = new Vector2(0.9f, 0.9f);
        portraitRect.offsetMin = Vector2.zero;
        portraitRect.offsetMax = Vector2.zero;
        
        // Add portrait loader
        CharacterPortraitLoader portraitLoader = portraitObj.AddComponent<CharacterPortraitLoader>();
        
        // Character name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(slotObj.transform, false);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = $"{charClass.className} ({genderLabel})";
        nameText.fontSize = 16;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontStyle = FontStyles.Bold;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.05f, 0.35f);
        nameRect.anchorMax = new Vector2(0.95f, 0.45f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // Race text
        GameObject raceObj = new GameObject("Race");
        raceObj.transform.SetParent(slotObj.transform, false);
        
        TextMeshProUGUI raceText = raceObj.AddComponent<TextMeshProUGUI>();
        raceText.text = charClass.race;
        raceText.fontSize = 14;
        raceText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        raceText.alignment = TextAlignmentOptions.Center;
        
        RectTransform raceRect = raceObj.GetComponent<RectTransform>();
        raceRect.anchorMin = new Vector2(0.05f, 0.28f);
        raceRect.anchorMax = new Vector2(0.95f, 0.35f);
        raceRect.offsetMin = Vector2.zero;
        raceRect.offsetMax = Vector2.zero;
        
        // Stats text
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(slotObj.transform, false);
        
        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = $"HP:{charClass.health} MP:{charClass.mana}\nATK:{charClass.attack} DEF:{charClass.defense} MAG:{charClass.magic}";
        statsText.fontSize = 11;
        statsText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        statsText.alignment = TextAlignmentOptions.Center;
        
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.05f, 0.15f);
        statsRect.anchorMax = new Vector2(0.95f, 0.25f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;
        
        // Generate button
        Button generateBtn = null;
        if (showGenerateControls)
        {
            generateBtn = CreateSlotButton(slotObj, "🎨 Generate", new Vector2(0.05f, 0.05f), new Vector2(0.45f, 0.12f), new Color(0.2f, 0.6f, 0.2f, 1f));
        }
        
        // Select button
        Button selectBtn = CreateSlotButton(slotObj, "▶ Select", new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.12f), new Color(0.2f, 0.2f, 0.6f, 1f));
        selectBtn.interactable = false; // Disabled until sprite is generated
        
        // Create character slot data
        CharacterSlot slot = new CharacterSlot
        {
            slotObject = slotObj,
            portraitImage = portraitImage,
            nameText = nameText,
            raceText = raceText,
            statsText = statsText,
            generateButton = generateBtn,
            selectButton = selectBtn,
            characterClass = charClass,
            portraitLoader = portraitLoader,
            isMale = isMale
        };
        
        // Setup button events
        if (generateBtn != null)
            generateBtn.onClick.AddListener(() => GenerateCharacterSprite(slot));
        selectBtn.onClick.AddListener(() => SelectCharacter(slot));
        
        characterSlots.Add(slot);

        // Try to auto-load existing portrait so users see their imported sprites immediately
        TryLoadExistingSprite(slot);
        if (slot.portraitImage.sprite != null)
        {
            slot.selectButton.interactable = true;
        }
    }
    
    private Button CreateSlotButton(GameObject parent, string text, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject btnObj = new GameObject($"Button_{text}");
        btnObj.transform.SetParent(parent.transform, false);
        
        Button button = btnObj.AddComponent<Button>();
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = anchorMin;
        btnRect.anchorMax = anchorMax;
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 12;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    private void SetupButtons()
    {
        if (generateAllButton != null)
        {
            generateAllButton.gameObject.SetActive(showGenerateControls);
            if (showGenerateControls)
                generateAllButton.onClick.AddListener(GenerateAllSprites);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMenu);
        }
    }

    private bool TryLoadExistingSprite(CharacterSlot slot)
    {
        string gender = slot.isMale ? "Male" : "Female";
        string resPath = $"Characters/New Characters/{slot.characterClass.className}/{gender}/{slot.characterClass.className}_{slot.characterClass.race}_{gender}_idle_64x64";
        var sprite = Resources.Load<Sprite>(resPath);
        if (sprite != null)
        {
            slot.portraitImage.sprite = sprite;
            slot.selectButton.interactable = true;
            return true;
        }
        return false;
    }
    
    private void GenerateCharacterSprite(CharacterSlot slot)
    {
        Debug.Log($"🎨 Generating sprite for {slot.characterClass.race} {slot.characterClass.className}");
        StartCoroutine(GenerateCharacterCoroutine(slot));
    }
    
    private IEnumerator GenerateCharacterCoroutine(CharacterSlot slot)
    {
        // Update status (optional)
        if (statusText != null)
        {
            statusText.text = $"🎨 Generating {slot.characterClass.race} {slot.characterClass.className}...";
            statusText.color = Color.yellow;
        }
        
        // Disable button during generation
        slot.generateButton.interactable = false;
        
        if (useRealPixelLAB)
        {
            // TODO: Real PixelLAB MCP call here
            yield return StartCoroutine(CallPixelLABMCP(slot));
        }
        else
        {
            // Simulate generation time
            yield return new WaitForSeconds(2f);
            
            // Try to load existing sprite, or create placeholder
            slot.portraitLoader.SetPortrait(slot.characterClass.race, slot.characterClass.className, slot.isMale);
        }
        
        // Enable select button
        slot.selectButton.interactable = true;
        slot.generateButton.interactable = true;
        
        if (statusText != null)
        {
            statusText.text = $"✅ Generated {slot.characterClass.race} {slot.characterClass.className}!";
            statusText.color = Color.green;
        }
        
        Debug.Log($"✅ Sprite generation complete for {slot.characterClass.race} {slot.characterClass.className}");
    }
    
    private IEnumerator CallPixelLABMCP(CharacterSlot slot)
    {
        // Generate the exact prompt from user's guidelines
        string description = GeneratePixelLABPrompt(slot.characterClass);
        
        Debug.Log($"🎯 Calling PixelLAB MCP for {slot.characterClass.className}...");
        Debug.Log($"🎯 Full Prompt: {description}");
        
        // Call the actual PixelLAB MCP function
        // Following user's guidelines: 64x64, SNES-style, front-facing idle + 8-direction sheet
        yield return StartCoroutine(GenerateCharacterWithPixelLAB(slot, description));
    }
    
    private string GeneratePixelLABPrompt(BlazedCharacterClass charClass)
    {
        // Generate the exact prompt format from user's guidelines
        string racePrefix = "";
        switch (charClass.race.ToLower())
        {
            case "human":
                racePrefix = "human character with realistic proportions, detailed features, ";
                break;
            case "devil":
                racePrefix = "devil character with horns, dark features, demonic appearance, ";
                break;
            case "skeleton":
                racePrefix = "skeleton character with bony structure, undead appearance, mystical aura, ";
                break;
        }
        
        // Use the exact prompt format from user's guidelines
        string prompt = $"{racePrefix}{charClass.description} Visual cues: {charClass.visualCues}. " +
                       $"64×64 pixels, SNES-style 16-bit pixel art, clean silhouette, readable gear, " +
                       $"full-body character, transparent background";
        
        return prompt;
    }
    
    private IEnumerator GenerateCharacterWithPixelLAB(CharacterSlot slot, string description)
    {
        Debug.Log($"🎨 Starting REAL PixelLAB generation for {slot.characterClass.className}...");
        Debug.Log($"📝 Prompt: {description}");
        
        // Show MCP function calls in console for verification
        Debug.Log($"🔥 CALLING MCP FUNCTION:");
        Debug.Log($"mcp_pixellab_create_character(description: \"{description}\", size: 64, n_directions: 8)");
        
        // Simulate the MCP call (replace with actual call when ready)
        yield return new WaitForSeconds(3f);
        
        // Try to load the generated sprite
        slot.portraitLoader.SetPortrait(slot.characterClass.race, slot.characterClass.className, slot.isMale);
        
        Debug.Log($"✅ PixelLAB generation complete for {slot.characterClass.className}");
    }
    
    private void SelectCharacter(CharacterSlot slot)
    {
        Debug.Log($"🎮 Selected character: {slot.characterClass.race} {slot.characterClass.className}");
        statusText.text = $"🎮 Selected: {slot.characterClass.race} {slot.characterClass.className}";
        statusText.color = Color.cyan;
        
        // TODO: Start the game with this character
    }
    
    private void GenerateAllSprites()
    {
        Debug.Log("🎨 Generating all character sprites...");
        StartCoroutine(GenerateAllSpritesCoroutine());
    }
    
    private IEnumerator GenerateAllSpritesCoroutine()
    {
        statusText.text = "🎨 Generating ALL character sprites with PixelLAB...";
        statusText.color = Color.magenta;
        
        foreach (var slot in characterSlots)
        {
            if (!slot.selectButton.interactable) // Only generate if not already done
            {
                yield return StartCoroutine(GenerateCharacterCoroutine(slot));
                yield return new WaitForSeconds(0.5f); // Small delay between generations
            }
        }
        
        statusText.text = "✅ All character sprites generated!";
        statusText.color = Color.green;
    }
    
    private void BackToMenu()
    {
        Debug.Log("⬅ Back to menu clicked");
        // TODO: Return to main menu
    }

    private SpriteRenderer FindRepresentativeSpriteRenderer(GameObject prefab)
    {
        var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers == null || renderers.Length == 0) return null;
        SpriteRenderer best = null; float bestScore = float.MinValue;
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i]; if (r == null || r.sprite == null) continue;
            string n = r.name.ToLowerInvariant();
            if (n.Contains("shadow") || n.Contains("fx") || n.Contains("effect") || n.Contains("projectile")) continue;
            if (n.Contains("weapon") || n.Contains("sword") || n.Contains("dagger") || n.Contains("bow")) continue;
            var rect = r.sprite.rect; float area = rect.width * rect.height;
            float score = area + (r.sortingOrder * 10f);
            if (score > bestScore) { bestScore = score; best = r; }
        }
        if (best == null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i]; if (r == null || r.sprite == null) continue;
                var rect = r.sprite.rect; float area = rect.width * rect.height;
                if (area > bestScore) { bestScore = area; best = r; }
            }
        }
        return best;
    }

    private void ShowSpumPreview(GameObject prefab)
    {
        // Ensure RawImage active and sprite Image hidden
        if (spumPreviewImage != null) spumPreviewImage.gameObject.SetActive(true);
        if (previewImage != null) previewImage.gameObject.SetActive(false);
        
        // Always kill any stray rigs from previous previews to avoid overlap
        var stray = GameObject.Find("SPUM_Preview_Rig");
        if (stray != null && stray != spumPreviewRig) { Destroy(stray); }
        
        // Create rig if needed
        if (spumPreviewRig == null)
        {
            spumPreviewRig = new GameObject("SPUM_Preview_Rig");
            spumPreviewRig.hideFlags = HideFlags.HideAndDontSave;
            spumPreviewCamera = spumPreviewRig.AddComponent<Camera>();
            spumPreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            spumPreviewCamera.backgroundColor = new Color(0,0,0,0);
            spumPreviewCamera.orthographic = true;
            spumPreviewCamera.cullingMask = ~0; // everything
            if (spumPreviewRT == null)
            {
                spumPreviewRT = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                spumPreviewRT.Create();
            }
            spumPreviewCamera.targetTexture = spumPreviewRT;
            spumPreviewImage.texture = spumPreviewRT;
        }
        else
        {
            // Reuse existing rig/RT
            if (spumPreviewCamera != null)
                spumPreviewCamera.targetTexture = spumPreviewRT;
            spumPreviewImage.texture = spumPreviewRT;
        }
        // Clear previous instance
        if (spumPreviewInstance != null) { Destroy(spumPreviewInstance); spumPreviewInstance = null; spumPreviewAnimator = null; }
        spumPreviewInstance = Instantiate(prefab, spumPreviewRig.transform);
        spumPreviewInstance.name = prefab.name + "_Preview";
        spumPreviewInstance.transform.position = Vector3.zero;
        spumPreviewInstance.transform.rotation = Quaternion.identity;
        spumPreviewInstance.transform.localScale = Vector3.one;
        spumPreviewAnimator = spumPreviewInstance.GetComponentInChildren<Animator>();
        // Frame camera to fit
        FrameCameraToObject();
        // Try to play move/walk animation
        TryPlayMoveAnimation();
        // Reset facing to default (east/right)
        currentDirIndex = 1; UpdateSpumFacingFromDir();
    }

    private void DestroySpumPreview()
    {
        if (spumPreviewImage != null) spumPreviewImage.gameObject.SetActive(false);
        if (previewImage != null) previewImage.gameObject.SetActive(true);
        if (spumPreviewInstance != null) { Destroy(spumPreviewInstance); spumPreviewInstance = null; spumPreviewAnimator = null; }
        // Keep camera/RT alive for reuse
    }

    private void CleanupPreviewResources()
    {
        try
        {
            if (spumPreviewImage != null) { spumPreviewImage.texture = null; spumPreviewImage.gameObject.SetActive(false); }
            if (spumPreviewCamera != null) spumPreviewCamera.targetTexture = null;
            if (spumPreviewRT != null) { spumPreviewRT.Release(); Destroy(spumPreviewRT); spumPreviewRT = null; }
            if (spumPreviewInstance != null) { Destroy(spumPreviewInstance); spumPreviewInstance = null; }
            if (spumPreviewRig != null) { Destroy(spumPreviewRig); spumPreviewRig = null; }
        }
        catch {}
    }

    private void OnDestroy()
    {
        // Cleanup preview resources to avoid leaks or carry-over
        if (spumPreviewInstance != null) Destroy(spumPreviewInstance);
        if (spumPreviewCamera != null) spumPreviewCamera.targetTexture = null;
        if (spumPreviewRT != null) { spumPreviewRT.Release(); Destroy(spumPreviewRT); }
        if (spumPreviewRig != null) Destroy(spumPreviewRig);
    }

    private void FrameCameraToObject()
    {
        if (spumPreviewCamera == null || spumPreviewInstance == null) return;
        var renderers = spumPreviewInstance.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0) return;
        Bounds b = new Bounds(renderers[0].bounds.center, Vector3.zero);
        for (int i = 0; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        spumPreviewCamera.transform.position = b.center + new Vector3(0, 0, -10f);
        float size = Mathf.Max(b.extents.x, b.extents.y) * 1.2f; // closer zoom for larger preview
        spumPreviewCamera.orthographicSize = Mathf.Max(0.1f, size);
    }

    private void TryPlayMoveAnimation()
    {
        if (spumPreviewAnimator == null) return;
        var controller = spumPreviewAnimator.runtimeAnimatorController;
        if (controller == null || controller.animationClips == null || controller.animationClips.Length == 0) return;
        AnimationClip chosen = null;
        chosen = controller.animationClips.FirstOrDefault(c => c.name.ToLower().Contains("move"))
                  ?? controller.animationClips.FirstOrDefault(c => c.name.ToLower().Contains("walk"))
                  ?? controller.animationClips.FirstOrDefault(c => c.name.ToLower().Contains("run"))
                  ?? controller.animationClips.First();
        if (chosen != null)
        {
            spumPreviewAnimator.Rebind();
            spumPreviewAnimator.Update(0f);
            spumPreviewAnimator.Play(chosen.name, 0, 0f);
        }
    }

    private void UpdateSpumFacingFromDir()
    {
        if (spumPreviewInstance == null) return;
        // Map east/right -> scale.x = 1, west/left -> -1, north/south keep 1
        int dir = currentDirIndex % 4;
        float sx = (dir == 3) ? -1f : 1f; // 3 = west
        spumPreviewInstance.transform.localScale = new Vector3(Mathf.Abs(spumPreviewInstance.transform.localScale.x) * sx,
                                                               spumPreviewInstance.transform.localScale.y,
                                                               spumPreviewInstance.transform.localScale.z);
    }

    private void CreateGenderButtons()
    {
        if (genderButtonsBar == null) return;
        for (int i = genderButtonsBar.childCount - 1; i >= 0; i--) Destroy(genderButtonsBar.GetChild(i).gameObject);
        var maleBtn = CreateButton(genderButtonsBar.gameObject, "Male", new Color(0.18f,0.45f,0.18f,1f));
        maleBtn.onClick.AddListener(() => { currentIsMale = true; RefreshPreview(); });
        var femaleBtn = CreateButton(genderButtonsBar.gameObject, "Female", new Color(0.45f,0.18f,0.45f,1f));
        femaleBtn.onClick.AddListener(() => { currentIsMale = false; RefreshPreview(); });
    }

    private bool HasActivePreview()
    {
        return (spumPreviewImage != null && spumPreviewImage.gameObject.activeSelf) || (currentPreviewFrames != null && currentPreviewFrames.Count > 0);
    }

    private void ClearPreviewUI()
    {
        DestroySpumPreview();
        currentPreviewFrames.Clear();
        currentFrameIndex = 0;
        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.gameObject.SetActive(false);
        }
        if (spumPreviewImage != null)
            spumPreviewImage.gameObject.SetActive(false);
        if (previewCaption != null) previewCaption.text = string.Empty;
        if (infoHeader != null) infoHeader.text = string.Empty;
        if (infoBody != null) infoBody.text = string.Empty;
        if (selectCurrentButton != null) selectCurrentButton.interactable = false;
    }
}
