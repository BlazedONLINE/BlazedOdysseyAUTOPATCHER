using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Boots a lightweight in-game HUD with a Settings button and placeholder HUD regions.
/// - Top-right Settings button that toggles the Settings overlay without pausing gameplay
/// - Creates placeholder containers for Quests, Group Bars, Chat, and Action Bars
/// </summary>
public class GameUIBootstrap_DISABLED : MonoBehaviour
{
	private const string CanvasName = "GameHUDCanvas";
	private Canvas _canvas;
	private SettingsMenu _settingsMenu;
#if ENABLE_INPUT_SYSTEM
	[SerializeField] private Key settingsToggleKey = Key.F1; // Avoid F10 (Recorder default)
#endif

	private void Awake()
	{
		EnsureCanvas();
		EnsureHudPlaceholders();
		EnsureSettingsButton();
	}

	private void Update()
	{
		// DISABLED: Input handling moved to BlazedUIBootstrap
		// #if ENABLE_INPUT_SYSTEM
		// var kb = Keyboard.current;
		// if (kb != null)
		// {
		// 	var toggleKey = kb[settingsToggleKey];
		// 	if (toggleKey != null && toggleKey.wasPressedThisFrame)
		// 	{
		// 		if (_settingsMenu == null) EnsureSettingsMenu();
		// 		_settingsMenu.Toggle();
		// 	}
		// 	if (kb.escapeKey.wasPressedThisFrame)
		// 	{
		// 		if (_settingsMenu != null && _settingsMenu.IsShown)
		// 			_settingsMenu.TryClose();
		// 	}
		// }
		// #endif
	}

	private void EnsureCanvas()
	{
		var existing = GameObject.Find(CanvasName);
		if (existing != null)
		{
			_canvas = existing.GetComponent<Canvas>();
			if (_canvas != null) return;
		}

		var go = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		go.layer = LayerMask.NameToLayer("UI");
		_canvas = go.GetComponent<Canvas>();
		_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		_canvas.sortingOrder = 500; // Above gameplay

		var scaler = go.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		scaler.matchWidthOrHeight = 1f; // favor height
	}

	private RectTransform CreatePanel(string name, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		go.transform.SetParent(parent, false);
		var rt = go.GetComponent<RectTransform>();
		var img = go.GetComponent<Image>();
		// Make placeholders invisible and non-blocking so they don't add ugly overlays or block clicks
		img.color = new Color(0f, 0f, 0f, 0f);
		img.raycastTarget = false;
		return rt;
	}

	private void EnsureHudPlaceholders()
	{
		var root = _canvas.transform;

		// Quests panel (right side)
		var quests = CreatePanel("QuestsPanel", root);
		quests.anchorMin = new Vector2(1f, 0.35f);
		quests.anchorMax = new Vector2(1f, 0.85f);
		quests.pivot = new Vector2(1f, 1f);
		quests.sizeDelta = new Vector2(360f, 560f);
		quests.anchoredPosition = new Vector2(-24f, -24f);

		// Group bars (left side)
		var group = CreatePanel("GroupBars", root);
		group.anchorMin = new Vector2(0f, 0.65f);
		group.anchorMax = new Vector2(0f, 0.95f);
		group.pivot = new Vector2(0f, 1f);
		group.sizeDelta = new Vector2(360f, 520f);
		group.anchoredPosition = new Vector2(24f, -24f);

		// Chat root (bottom-left)
		var chat = CreatePanel("ChatRoot", root);
		chat.anchorMin = new Vector2(0f, 0f);
		chat.anchorMax = new Vector2(0.35f, 0.25f);
		chat.pivot = new Vector2(0f, 0f);
		chat.anchoredPosition = new Vector2(12f, 12f);

		// Action bars (bottom center)
		var actionBars = CreatePanel("ActionBars", root);
		actionBars.anchorMin = new Vector2(0.25f, 0f);
		actionBars.anchorMax = new Vector2(0.75f, 0.14f);
		actionBars.pivot = new Vector2(0.5f, 0f);
		actionBars.anchoredPosition = new Vector2(0f, 12f);
	}

	private void EnsureSettingsMenu()
	{
		// Ensure audio manager exists BEFORE building UI (sliders bind to it)
		if (FindAnyObjectByType<GameAudioManager>() == null)
		{
			var audioGo = new GameObject("GameAudioManager", typeof(GameAudioManager));
			DontDestroyOnLoad(audioGo);
		}
		if (_settingsMenu == null)
		{
			var go = new GameObject("SettingsMenu", typeof(SettingsMenu));
			go.transform.SetParent(_canvas.transform, false);
			_settingsMenu = go.GetComponent<SettingsMenu>();
			try { _settingsMenu.BuildUI(_canvas); }
			catch (System.Exception ex) { Debug.LogError($"Settings UI build failed: {ex.Message}\n{ex.StackTrace}"); }
			_settingsMenu.Hide();
		}
	}

	private void EnsureSettingsButton()
	{
		var go = new GameObject("SettingsButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
		go.transform.SetParent(_canvas.transform, false);
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = new Vector2(1f, 1f);
		rt.anchorMax = new Vector2(1f, 1f);
		rt.pivot = new Vector2(1f, 1f);
		rt.sizeDelta = new Vector2(46f, 46f);
		rt.anchoredPosition = new Vector2(-18f, -18f);

		var img = go.GetComponent<Image>();
		img.color = new Color(0f, 0f, 0f, 0.2f);
		img.raycastTarget = true;
		// Try load a cog/gear icon from Resources/Icons (Franuka pack)
		img.sprite = TryLoadCogSprite();
		img.preserveAspect = true;

		var btn = go.GetComponent<Button>();
		btn.onClick.AddListener(() =>
		{
			if (_settingsMenu == null) { EnsureSettingsMenu(); }
			if (_settingsMenu.IsShown) _settingsMenu.Hide(); else _settingsMenu.Show();
		});
	}

	private Sprite TryLoadCogSprite()
	{
		string[] candidateFolders = new []
		{
			"Icons",
			"Icons/GUI",
			"Icons/Fantasy RPG icon pack (by Franuka)",
			"Icons/Fantasy RPG icon pack (by Franuka)/GUI"
		};
		string[] keywords = new []{ "gear", "cog", "settings", "option" };
		foreach (var folder in candidateFolders)
		{
			var sprites = Resources.LoadAll<Sprite>(folder);
			if (sprites == null || sprites.Length == 0) continue;
			foreach (var k in keywords)
			{
				for (int i = 0; i < sprites.Length; i++)
				{
					if (sprites[i] != null && sprites[i].name.ToLower().Contains(k))
						return sprites[i];
				}
			}
		}
		return null;
	}
}


