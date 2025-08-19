using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lightweight settings overlay with placeholder categories and working volume sliders.
/// Non-blocking overlay keeps world/mini-map visible behind.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
	private Canvas _canvas;
	private RectTransform _root;
	public bool IsShown { get; private set; }
	private bool _dirty;

	// References when using prefab-driven (SharpUI) view
	private GameObject _viewInstance; // root of instantiated prefab
	private Transform _viewRoot; // cached transform to search controls
	private Button _closeButton;
	private Button _applyButton;

	// Attempts to build this settings UI from a prefab placed under Resources/UI/SettingsMenuView
	// This enables swapping to SharpUI by providing a prefab without changing logic.
	private bool TryBuildFromPrefab(Canvas canvas)
	{
		var prefab = Resources.Load<GameObject>("UI/SettingsMenuView");
		if (prefab == null)
		{
			return false;
		}

		_viewInstance = Instantiate(prefab, canvas.transform, false);
		_viewRoot = _viewInstance.transform;
		_root = _viewInstance.GetComponent<RectTransform>();

		// Wire standard controls if present by common name fragments
		_closeButton = FindButtonBySubstring(_viewRoot, "close");
		if (_closeButton != null) _closeButton.onClick.AddListener(Hide);

		_applyButton = FindButtonBySubstring(_viewRoot, "apply");
		if (_applyButton != null) _applyButton.onClick.AddListener(ApplySettings);

		return true;
	}

	private Button FindButtonBySubstring(Transform root, string nameFragment)
	{
		if (root == null || string.IsNullOrEmpty(nameFragment)) return null;
		nameFragment = nameFragment.ToLowerInvariant();
		var buttons = root.GetComponentsInChildren<Button>(true);
		for (int i = 0; i < buttons.Length; i++)
		{
			var b = buttons[i];
			if (b != null && b.name != null && b.name.ToLowerInvariant().Contains(nameFragment))
				return b;
		}
		return null;
	}

	public void BuildUI(Canvas canvas)
	{
		_canvas = canvas;

		// Try to build from a prefab if present (SharpUI-ready). Place prefab at: Resources/UI/SettingsMenuView.prefab
		if (TryBuildFromPrefab(canvas))
		{
			Hide();
			return;
		}
		// Fullscreen dim overlay with close functionality
		_root = new GameObject("SettingsOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
		_root.transform.SetParent(canvas.transform, false);
		var bg = _root.GetComponent<Image>();
		bg.color = new Color(0f, 0f, 0f, 0.55f);
		_root.anchorMin = new Vector2(0f, 0f);
		_root.anchorMax = new Vector2(1f, 1f);
		_root.offsetMin = Vector2.zero;
		_root.offsetMax = Vector2.zero;
		
		// Make background clickable to close
		var bgBtn = _root.GetComponent<Button>();
		bgBtn.targetGraphic = bg;
		bgBtn.onClick.AddListener(Hide);
		
		// Note: _root.gameObject is our main menu object

		// Centered card with click prevention
		var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
		card.transform.SetParent(_root, false);
		var cardImg = card.GetComponent<Image>();
		cardImg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);
		card.anchorMin = new Vector2(0.5f, 0.5f);
		card.anchorMax = new Vector2(0.5f, 0.5f);
		card.pivot = new Vector2(0.5f, 0.5f);
		card.sizeDelta = new Vector2(960f, 620f);
		card.anchoredPosition = Vector2.zero;
		
		// Prevent card clicks from closing menu
		var cardBtn = card.GetComponent<Button>();
		cardBtn.targetGraphic = cardImg;
		cardBtn.onClick.AddListener(() => {}); // Do nothing

		CreateHeader(card, "Settings");
		// Tabs and content panels
		var tabs = new GameObject("Tabs", typeof(RectTransform)).GetComponent<RectTransform>();
		tabs.transform.SetParent(card, false);
		tabs.anchorMin = new Vector2(0.04f, 0.87f);
		tabs.anchorMax = new Vector2(0.96f, 0.94f);
		tabs.offsetMin = Vector2.zero; tabs.offsetMax = Vector2.zero;
		var h = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
		h.spacing = 8f; h.padding = new RectOffset(4,4,0,0);

		var panels = new GameObject("Panels", typeof(RectTransform)).GetComponent<RectTransform>();
		panels.transform.SetParent(card, false);
		panels.anchorMin = new Vector2(0.04f, 0.08f);
		panels.anchorMax = new Vector2(0.96f, 0.86f);
		panels.offsetMin = Vector2.zero; panels.offsetMax = Vector2.zero;

		var audioPanel = CreateScrollContent(panels); audioPanel.gameObject.name = "AudioPanel"; CreateAudioSection(audioPanel);
		var displayPanel = CreateScrollContent(panels); displayPanel.gameObject.name = "DisplayPanel"; CreateDisplaySection(displayPanel);
		var chatPanel = CreateScrollContent(panels); chatPanel.gameObject.name = "ChatPanel"; CreateChatSection(chatPanel);
		var keybindsPanel = CreateScrollContent(panels); keybindsPanel.gameObject.name = "KeybindsPanel"; CreateKeybindsSection(keybindsPanel);

		CreateTabButton(tabs, "Audio", () => ShowOnly(audioPanel, displayPanel, chatPanel, keybindsPanel));
		CreateTabButton(tabs, "Display", () => ShowOnly(displayPanel, audioPanel, chatPanel, keybindsPanel));
		CreateTabButton(tabs, "General", () => ShowOnly(chatPanel, audioPanel, displayPanel, keybindsPanel));
		CreateTabButton(tabs, "Keybinds", () => ShowOnly(keybindsPanel, audioPanel, displayPanel, chatPanel));

		ShowOnly(audioPanel, displayPanel, chatPanel, keybindsPanel);
		CreateCloseButton(card);

		// Apply button row
		CreateApplyRow(card);
	}

	private void CreateHeader(RectTransform parent, string title)
	{
		var t = NewTMP(parent, title, 28, FontStyles.Bold);
		t.anchorMin = new Vector2(0.5f, 1f);
		t.anchorMax = new Vector2(0.5f, 1f);
		t.pivot = new Vector2(0.5f, 1f);
		t.anchoredPosition = new Vector2(0, -18);
	}

	private RectTransform CreateScrollContent(RectTransform parent)
	{
		var view = new GameObject("ScrollView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect), typeof(Mask));
		view.transform.SetParent(parent, false);
		var rt = view.GetComponent<RectTransform>();
		rt.anchorMin = new Vector2(0.04f, 0.08f);
		rt.anchorMax = new Vector2(0.96f, 0.86f);
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
		var viewImg = view.GetComponent<Image>();
		viewImg.color = new Color(1f,1f,1f,0.03f);
		view.GetComponent<Mask>().showMaskGraphic = false;

		var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
		content.transform.SetParent(rt, false);
		var layout = content.GetComponent<VerticalLayoutGroup>();
		layout.spacing = 15f; 
		layout.padding = new RectOffset(20, 20, 20, 20); 
		layout.childControlHeight = false; 
		layout.childForceExpandHeight = false;
		layout.childControlWidth = true;
		layout.childForceExpandWidth = true;
		content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

		var scroll = view.GetComponent<ScrollRect>();
		scroll.content = content;
		scroll.horizontal = false;

		return content;
	}

	private void CreateAudioSection(RectTransform parent)
	{
		var section = NewSection(parent, "Audio");
		
		// Load saved values or use defaults
		float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
		float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
		float ambienceVol = PlayerPrefs.GetFloat("AmbienceVolume", 0.6f);
		float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 0.7f);
		
		// Apply loaded values to audio manager
		GameAudioManager.Instance.SetMasterVolume(masterVol);
		GameAudioManager.Instance.SetMusicVolume(musicVol);
		GameAudioManager.Instance.SetAmbienceVolume(ambienceVol);
		GameAudioManager.Instance.SetSfxVolume(sfxVol);
		
		NewSlider(section, "Master Volume", GameAudioManager.Instance.SetMasterVolume, masterVol);
		NewSlider(section, "Music Volume", GameAudioManager.Instance.SetMusicVolume, musicVol);
		NewSlider(section, "Ambience Volume", GameAudioManager.Instance.SetAmbienceVolume, ambienceVol);
		NewSlider(section, "SFX Volume", GameAudioManager.Instance.SetSfxVolume, sfxVol);
	}

	private void CreateDisplaySection(RectTransform parent)
	{
		var section = NewSection(parent, "Display");
		
		// Load saved values
		bool fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
		int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", Screen.resolutions.Length - 1);
		
		NewDropdown(section, "Resolution", Screen.resolutions, r => $"{r.width}x{r.height}@{r.refreshRateRatio.value:F0}", i =>
		{
			var r = Screen.resolutions[i];
			Screen.SetResolution(r.width, r.height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
			PlayerPrefs.SetInt("ResolutionIndex", i);
		});
		
		NewToggle(section, "Fullscreen", fullscreen, v => {
			Screen.fullScreen = v;
			PlayerPrefs.SetInt("Fullscreen", v ? 1 : 0);
		});
		
		NewToggle(section, "VSync", QualitySettings.vSyncCount > 0, v => {
			QualitySettings.vSyncCount = v ? 1 : 0;
			PlayerPrefs.SetInt("VSync", v ? 1 : 0);
		});
	}

	private void CreateChatSection(RectTransform parent)
	{
		var section = NewSection(parent, "Chat Channels");
		
		// Load saved chat preferences
		bool general = PlayerPrefs.GetInt("Chat_General", 1) == 1;
		bool trade = PlayerPrefs.GetInt("Chat_Trade", 1) == 1;
		bool guild = PlayerPrefs.GetInt("Chat_Guild", 1) == 1;
		bool party = PlayerPrefs.GetInt("Chat_Party", 1) == 1;
		bool instance = PlayerPrefs.GetInt("Chat_Instance", 1) == 1;
		
		NewToggle(section, "General Chat", general, v => {
			PlayerPrefs.SetInt("Chat_General", v ? 1 : 0);
			Debug.Log($"General chat {(v ? "enabled" : "disabled")}");
		});
		NewToggle(section, "Trade Chat", trade, v => {
			PlayerPrefs.SetInt("Chat_Trade", v ? 1 : 0);
			Debug.Log($"Trade chat {(v ? "enabled" : "disabled")}");
		});
		NewToggle(section, "Guild Chat", guild, v => {
			PlayerPrefs.SetInt("Chat_Guild", v ? 1 : 0);
			Debug.Log($"Guild chat {(v ? "enabled" : "disabled")}");
		});
		NewToggle(section, "Party Chat", party, v => {
			PlayerPrefs.SetInt("Chat_Party", v ? 1 : 0);
			Debug.Log($"Party chat {(v ? "enabled" : "disabled")}");
		});
		NewToggle(section, "Instance Chat", instance, v => {
			PlayerPrefs.SetInt("Chat_Instance", v ? 1 : 0);
			Debug.Log($"Instance chat {(v ? "enabled" : "disabled")}");
		});
	}

	private void CreateKeybindsSection(RectTransform parent)
	{
		var section = NewSection(parent, "Key Bindings");
		
		// Create keybind display (not editable for now, just showing current bindings)
		NewTMP(section, "Settings Menu: F1", 16, FontStyles.Normal);
		NewTMP(section, "Close Menu: ESC", 16, FontStyles.Normal);
		NewTMP(section, "", 8, FontStyles.Normal); // spacer
		
		NewTMP(section, "Combat:", 18, FontStyles.Bold);
		NewTMP(section, "Attack: Left Click", 16, FontStyles.Normal);
		NewTMP(section, "Block: Right Click", 16, FontStyles.Normal);
		NewTMP(section, "Use Skill: 1-9", 16, FontStyles.Normal);
		NewTMP(section, "", 8, FontStyles.Normal); // spacer
		
		NewTMP(section, "Movement:", 18, FontStyles.Bold);
		NewTMP(section, "Move: WASD / Arrow Keys", 16, FontStyles.Normal);
		NewTMP(section, "Run: Hold Shift", 16, FontStyles.Normal);
		NewTMP(section, "", 8, FontStyles.Normal); // spacer
		
		NewTMP(section, "Interface:", 18, FontStyles.Bold);
		NewTMP(section, "Inventory: I", 16, FontStyles.Normal);
		NewTMP(section, "Character: C", 16, FontStyles.Normal);
		NewTMP(section, "Chat: Enter", 16, FontStyles.Normal);
		NewTMP(section, "Map: M", 16, FontStyles.Normal);
		
		NewTMP(section, "", 16, FontStyles.Normal); // spacer
		NewTMP(section, "Note: Keybind customization coming soon!", 14, FontStyles.Italic);
	}

	private void CreateCloseButton(RectTransform parent)
	{
		var go = new GameObject("Close", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
		go.transform.SetParent(parent, false);
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = new Vector2(1f, 1f);
		rt.anchorMax = new Vector2(1f, 1f);
		rt.pivot = new Vector2(1f, 1f);
		rt.sizeDelta = new Vector2(32f, 32f);
		rt.anchoredPosition = new Vector2(-12f, -12f);
		go.GetComponent<Image>().color = new Color(0.2f,0.2f,0.2f,0.8f);
		// Try to use an 'X' or close icon from icons pack
		var closeSprite = Resources.LoadAll<Sprite>("Icons");
		for (int i=0;i<closeSprite.Length;i++)
		{
			if (closeSprite[i].name.ToLower().Contains("close") || closeSprite[i].name.ToLower().Contains("x"))
			{
				go.GetComponent<Image>().sprite = closeSprite[i];
				break;
			}
		}
		go.GetComponent<Button>().onClick.AddListener(Hide);
	}

	private void CreateTabButton(RectTransform tabsRoot, string label, System.Action onClick)
	{
		var btnGo = new GameObject(label+"Tab", typeof(RectTransform), typeof(Image), typeof(Button));
		btnGo.transform.SetParent(tabsRoot, false);
		var rt = btnGo.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(140f, 40f);
		
		var img = btnGo.GetComponent<Image>();
		img.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);
		
		// Create text with proper centering
		var textGo = new GameObject("Text", typeof(TextMeshProUGUI));
		textGo.transform.SetParent(btnGo.transform, false);
		var textRT = textGo.GetComponent<RectTransform>();
		textRT.anchorMin = Vector2.zero;
		textRT.anchorMax = Vector2.one;
		textRT.sizeDelta = Vector2.zero;
		textRT.anchoredPosition = Vector2.zero;
		
		var textTMP = textGo.GetComponent<TextMeshProUGUI>();
		textTMP.text = label;
		textTMP.fontSize = 18;
		textTMP.fontStyle = FontStyles.Bold;
		textTMP.color = Color.white;
		textTMP.alignment = TextAlignmentOptions.Center;
		textTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
		textTMP.raycastTarget = false;
		
		// Add hover effects
		var button = btnGo.GetComponent<Button>();
		var colors = button.colors;
		colors.normalColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
		colors.highlightedColor = new Color(0.25f, 0.25f, 0.3f, 0.9f);
		colors.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
		button.colors = colors;
		button.targetGraphic = img;
		
		button.onClick.AddListener(() => onClick?.Invoke());
	}

	private void ShowOnly(RectTransform show, RectTransform a, RectTransform b, RectTransform c)
	{
		show.gameObject.SetActive(true);
		a.gameObject.SetActive(false);
		b.gameObject.SetActive(false);
		c.gameObject.SetActive(false);
	}

	private RectTransform NewSection(RectTransform parent, string title)
	{
		var panel = new GameObject(title+"_Section", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
		panel.transform.SetParent(parent, false);
		panel.GetComponent<Image>().color = new Color(1f,1f,1f,0.04f);
		
		// Fix layout settings for proper spacing
		var layout = panel.GetComponent<VerticalLayoutGroup>();
		layout.spacing = 12f; 
		layout.padding = new RectOffset(20, 20, 16, 16); 
		layout.childControlHeight = false; 
		layout.childForceExpandHeight = false;
		layout.childControlWidth = false;
		layout.childForceExpandWidth = true;
		layout.childAlignment = TextAnchor.UpperLeft;
		
		// Add ContentSizeFitter to make section resize properly
		var fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		
		NewTMP(panel, title, 22, FontStyles.Bold);
		return panel;
	}

	private RectTransform NewTMP(Transform parent, string text, int size, FontStyles style)
	{
		var go = new GameObject("Text", typeof(TextMeshProUGUI));
		go.transform.SetParent(parent, false);
		var tmp = go.GetComponent<TextMeshProUGUI>();
		tmp.text = text; 
		tmp.fontSize = size; 
		tmp.fontStyle = style; 
		tmp.color = Color.white;
		tmp.raycastTarget = false; // Don't block clicks
		
		// Set proper size for text elements
		var rt = tmp.rectTransform;
		rt.sizeDelta = new Vector2(0f, size + 4f); // Height based on font size
		
		// Add LayoutElement for better spacing control
		var layoutElement = go.AddComponent<LayoutElement>();
		layoutElement.minHeight = size + 4f;
		layoutElement.preferredHeight = size + 4f;
		
		return rt;
	}

	private void NewSlider(RectTransform parent, string label, System.Action<float> onChanged, float defaultValue)
	{
		// Create container for label and slider
		var container = new GameObject(label+"_Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
		container.transform.SetParent(parent, false);
		var containerRT = container.GetComponent<RectTransform>();
		containerRT.sizeDelta = new Vector2(0f, 50f);
		
		var containerLayout = container.GetComponent<VerticalLayoutGroup>();
		containerLayout.spacing = 5f;
		containerLayout.childControlHeight = false;
		containerLayout.childControlWidth = true;
		containerLayout.childForceExpandWidth = true;
		
		// Add LayoutElement to container
		var containerLayoutElement = container.AddComponent<LayoutElement>();
		containerLayoutElement.minHeight = 50f;
		containerLayoutElement.preferredHeight = 50f;
		
		// Create label
		var labelRT = NewTMP(container.transform, label, 16, FontStyles.Normal);
		
		// Create slider container
		var go = new GameObject(label+"_Slider", typeof(RectTransform));
		go.transform.SetParent(container.transform, false);
		var rt = go.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(0f, 25f);
		
		// Add LayoutElement to slider
		var sliderLayoutElement = go.AddComponent<LayoutElement>();
		sliderLayoutElement.minHeight = 25f;
		sliderLayoutElement.preferredHeight = 25f;
		
		// Create background
		var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
		bg.transform.SetParent(go.transform, false);
		var bgRT = bg.GetComponent<RectTransform>();
		bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
		bgRT.sizeDelta = Vector2.zero; bgRT.anchoredPosition = Vector2.zero;
		bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
		
		// Create fill area
		var fillArea = new GameObject("Fill Area", typeof(RectTransform));
		fillArea.transform.SetParent(go.transform, false);
		var fillAreaRT = fillArea.GetComponent<RectTransform>();
		fillAreaRT.anchorMin = Vector2.zero; fillAreaRT.anchorMax = Vector2.one;
		fillAreaRT.sizeDelta = new Vector2(-20f, 0f); fillAreaRT.anchoredPosition = Vector2.zero;
		
		// Create fill
		var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
		fill.transform.SetParent(fillArea.transform, false);
		var fillRT = fill.GetComponent<RectTransform>();
		fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(0f, 1f);
		fillRT.sizeDelta = Vector2.zero; fillRT.anchoredPosition = Vector2.zero;
		fill.GetComponent<Image>().color = new Color(0.9f, 0.7f, 0.2f, 0.8f);
		
		// Create handle slide area
		var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
		handleArea.transform.SetParent(go.transform, false);
		var handleAreaRT = handleArea.GetComponent<RectTransform>();
		handleAreaRT.anchorMin = Vector2.zero; handleAreaRT.anchorMax = Vector2.one;
		handleAreaRT.sizeDelta = new Vector2(-20f, 0f); handleAreaRT.anchoredPosition = Vector2.zero;
		
		// Create handle
		var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
		handle.transform.SetParent(handleArea.transform, false);
		var handleRT = handle.GetComponent<RectTransform>();
		handleRT.sizeDelta = new Vector2(20f, 20f);
		handle.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
		
		// Add slider component and configure
		var slider = go.AddComponent<Slider>();
		slider.fillRect = fillRT;
		slider.handleRect = handleRT;
		slider.targetGraphic = handle.GetComponent<Image>();
		slider.direction = Slider.Direction.LeftToRight;
		slider.minValue = 0f;
		slider.maxValue = 1f;
		slider.wholeNumbers = false;
		slider.value = defaultValue;
		
		slider.onValueChanged.AddListener(v => { 
			onChanged?.Invoke(v); 
			_dirty = true; 
		});
		
		onChanged?.Invoke(defaultValue);
	}

	private void NewToggle(RectTransform parent, string label, bool defaultOn, System.Action<bool> onChanged)
	{
		// Create toggle container
		var go = new GameObject(label+"_Toggle", typeof(RectTransform), typeof(HorizontalLayoutGroup));
		go.transform.SetParent(parent, false);
		var rt = go.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(0f, 30f);
		
		// Configure layout
		var layout = go.GetComponent<HorizontalLayoutGroup>();
		layout.spacing = 10f;
		layout.childControlHeight = false;
		layout.childControlWidth = false;
		layout.childForceExpandWidth = false;
		layout.childAlignment = TextAnchor.MiddleLeft;
		
		// Add LayoutElement
		var layoutElement = go.AddComponent<LayoutElement>();
		layoutElement.minHeight = 30f;
		layoutElement.preferredHeight = 30f;
		
		// Create checkbox background
		var checkboxBg = new GameObject("Checkbox", typeof(RectTransform), typeof(Image));
		checkboxBg.transform.SetParent(go.transform, false);
		var cbBgRT = checkboxBg.GetComponent<RectTransform>();
		cbBgRT.sizeDelta = new Vector2(22f, 22f);
		checkboxBg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
		
		// Add LayoutElement to checkbox
		var cbLayoutElement = checkboxBg.AddComponent<LayoutElement>();
		cbLayoutElement.minWidth = 22f;
		cbLayoutElement.preferredWidth = 22f;
		cbLayoutElement.minHeight = 22f;
		cbLayoutElement.preferredHeight = 22f;
		
		// Create checkmark
		var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
		checkmark.transform.SetParent(checkboxBg.transform, false);
		var checkRT = checkmark.GetComponent<RectTransform>();
		checkRT.anchorMin = Vector2.zero; checkRT.anchorMax = Vector2.one;
		checkRT.sizeDelta = Vector2.zero; checkRT.anchoredPosition = Vector2.zero;
		checkmark.GetComponent<Image>().color = new Color(0.9f, 0.7f, 0.2f, 1f);
		checkmark.SetActive(defaultOn);
		
		// Create label
		var labelObj = new GameObject("Label", typeof(TextMeshProUGUI));
		labelObj.transform.SetParent(go.transform, false);
		var labelRT = labelObj.GetComponent<RectTransform>();
		labelRT.sizeDelta = new Vector2(200f, 30f);
		var labelTMP = labelObj.GetComponent<TextMeshProUGUI>();
		labelTMP.text = label;
		labelTMP.fontSize = 16;
		labelTMP.fontStyle = FontStyles.Normal;
		labelTMP.color = Color.white;
		labelTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
		labelTMP.raycastTarget = false;
		
		// Add toggle component and configure
		var toggle = go.AddComponent<Toggle>();
		toggle.targetGraphic = checkboxBg.GetComponent<Image>();
		toggle.graphic = checkmark.GetComponent<Image>();
		toggle.isOn = defaultOn;
		
		toggle.onValueChanged.AddListener(v => { 
			onChanged?.Invoke(v); 
			_dirty = true; 
		});
	}

	private void NewDropdown<T>(RectTransform parent, string label, T[] options, System.Func<T,string> toText, System.Action<int> onSelected)
	{
		// Create label
		var labelRT = NewTMP(parent, label, 18, FontStyles.Normal);
		labelRT.sizeDelta = new Vector2(200f, 24f);
		
		// Create dropdown container
		var go = new GameObject(label+"_Dropdown", typeof(RectTransform), typeof(Image));
		go.transform.SetParent(parent, false);
		var rt = go.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(300f, 30f);
		go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
		
		// Create label for dropdown
		var dropdownLabel = new GameObject("Label", typeof(TextMeshProUGUI));
		dropdownLabel.transform.SetParent(go.transform, false);
		var dropdownLabelRT = dropdownLabel.GetComponent<RectTransform>();
		dropdownLabelRT.anchorMin = Vector2.zero; dropdownLabelRT.anchorMax = Vector2.one;
		dropdownLabelRT.sizeDelta = new Vector2(-50f, 0f); dropdownLabelRT.anchoredPosition = Vector2.zero;
		var dropdownLabelTMP = dropdownLabel.GetComponent<TextMeshProUGUI>();
		dropdownLabelTMP.text = options.Length > 0 ? toText(options[0]) : "None";
		dropdownLabelTMP.fontSize = 16;
		dropdownLabelTMP.color = Color.white;
		dropdownLabelTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
		dropdownLabelTMP.margin = new Vector4(10f, 0f, 0f, 0f);
		
		// Create arrow
		var arrow = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
		arrow.transform.SetParent(go.transform, false);
		var arrowRT = arrow.GetComponent<RectTransform>();
		arrowRT.anchorMin = new Vector2(1f, 0.5f); arrowRT.anchorMax = new Vector2(1f, 0.5f);
		arrowRT.pivot = new Vector2(1f, 0.5f);
		arrowRT.sizeDelta = new Vector2(20f, 20f);
		arrowRT.anchoredPosition = new Vector2(-10f, 0f);
		arrow.GetComponent<Image>().color = Color.white;
		
		// Create template (for dropdown items)
		var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
		template.transform.SetParent(go.transform, false);
		template.SetActive(false);
		var templateRT = template.GetComponent<RectTransform>();
		templateRT.anchorMin = new Vector2(0f, 0f); templateRT.anchorMax = new Vector2(1f, 0f);
		templateRT.pivot = new Vector2(0f, 1f);
		templateRT.sizeDelta = new Vector2(0f, 150f);
		templateRT.anchoredPosition = new Vector2(0f, 0f);
		template.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
		
		// Create content for template
		var content = new GameObject("Content", typeof(RectTransform));
		content.transform.SetParent(template.transform, false);
		
		// Create item for template
		var item = new GameObject("Item", typeof(RectTransform), typeof(Image), typeof(Toggle));
		item.transform.SetParent(content.transform, false);
		
		// Add dropdown component and configure
		var dd = go.AddComponent<TMP_Dropdown>();
		dd.targetGraphic = go.GetComponent<Image>();
		dd.captionText = dropdownLabelTMP;
		dd.template = templateRT;
		dd.options.Clear();
		for (int i = 0; i < options.Length; i++) 
			dd.options.Add(new TMP_Dropdown.OptionData(toText(options[i])));
		
		dd.onValueChanged.AddListener((int idx) => { 
			if (onSelected != null) onSelected(idx); 
			_dirty = true; 
		});
		
		if (options.Length > 0) onSelected?.Invoke(0);
	}

	public void Show(){ if (_root != null) _root.gameObject.SetActive(true); else gameObject.SetActive(true); IsShown = true; }
	public void Hide(){ if (_root != null) _root.gameObject.SetActive(false); else gameObject.SetActive(false); IsShown = false; }
	public void Toggle(){ if (IsShown) TryClose(); else Show(); }

	public void TryClose()
	{
		if (!_dirty) { Hide(); return; }
		// Simple confirm overlay
		var confirm = new GameObject("ConfirmClose", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
		confirm.transform.SetParent(_root, false);
		confirm.anchorMin = new Vector2(0f,0f); confirm.anchorMax = new Vector2(1f,1f);
		confirm.offsetMin = Vector2.zero; confirm.offsetMax = Vector2.zero;
		confirm.GetComponent<Image>().color = new Color(0f,0f,0f,0.6f);
		var box = new GameObject("Box", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
		box.transform.SetParent(confirm, false);
		box.sizeDelta = new Vector2(480f, 180f);
		box.anchorMin = box.anchorMax = new Vector2(0.5f,0.5f);
		box.GetComponent<Image>().color = new Color(0.1f,0.1f,0.14f,0.95f);
		NewTMP(box, "Discard changes?", 24, FontStyles.Bold);
		var row = new GameObject("Row", typeof(RectTransform)).GetComponent<RectTransform>();
		row.transform.SetParent(box, false);
		row.anchorMin = new Vector2(0.5f,0f); row.anchorMax = new Vector2(0.5f,0f); row.pivot = new Vector2(0.5f,0f);
		row.anchoredPosition = new Vector2(0, 16);
		var yes = new GameObject("Discard", typeof(RectTransform), typeof(Image), typeof(Button));
		yes.transform.SetParent(row, false); yes.GetComponent<Image>().color = new Color(0.8f,0.3f,0.3f,0.9f);
		NewTMP(yes.transform, "Discard", 18, FontStyles.Bold);
		yes.GetComponent<Button>().onClick.AddListener(() => { _dirty = false; Hide(); Destroy(confirm.gameObject); });
		var apply = new GameObject("Apply", typeof(RectTransform), typeof(Image), typeof(Button));
		apply.transform.SetParent(row, false); apply.GetComponent<Image>().color = new Color(0.3f,0.8f,0.4f,0.9f);
		NewTMP(apply.transform, "Apply", 18, FontStyles.Bold);
		apply.GetComponent<Button>().onClick.AddListener(() => { ApplySettings(); Destroy(confirm.gameObject); Hide(); });
	}

	private void CreateApplyRow(RectTransform parent)
	{
		var row = new GameObject("ApplyRow", typeof(RectTransform)).GetComponent<RectTransform>();
		row.transform.SetParent(parent, false);
		row.anchorMin = new Vector2(1f, 0f); row.anchorMax = new Vector2(1f, 0f); row.pivot = new Vector2(1f, 0f);
		row.anchoredPosition = new Vector2(-16f, 16f);
		row.sizeDelta = new Vector2(260f, 40f);
		var apply = new GameObject("Apply", typeof(RectTransform), typeof(Image), typeof(Button));
		apply.transform.SetParent(row, false);
		apply.GetComponent<Image>().color = new Color(0.3f,0.8f,0.4f,0.9f);
		NewTMP(apply.transform, "Apply", 18, FontStyles.Bold);
		apply.GetComponent<Button>().onClick.AddListener(ApplySettings);
		var save = new GameObject("Save", typeof(RectTransform), typeof(Image), typeof(Button));
		save.transform.SetParent(row, false);
		save.GetComponent<Image>().color = new Color(0.4f,0.5f,0.9f,0.9f);
		NewTMP(save.transform, "Save", 18, FontStyles.Bold);
		save.GetComponent<Button>().onClick.AddListener(ApplySettings);
	}

	private void ApplySettings()
	{
		// Save all current audio settings
		if (GameAudioManager.Instance != null)
		{
			PlayerPrefs.SetFloat("MasterVolume", GameAudioManager.Instance.GetMasterVolume());
			PlayerPrefs.SetFloat("MusicVolume", GameAudioManager.Instance.GetMusicVolume());
			PlayerPrefs.SetFloat("AmbienceVolume", GameAudioManager.Instance.GetAmbienceVolume());
			PlayerPrefs.SetFloat("SfxVolume", GameAudioManager.Instance.GetSfxVolume());
		}
		
		// Display settings are saved immediately when changed
		// Chat settings are saved immediately when changed
		
		PlayerPrefs.Save();
		_dirty = false;
		
		Debug.Log("✅ Settings saved successfully!");
	}


}


