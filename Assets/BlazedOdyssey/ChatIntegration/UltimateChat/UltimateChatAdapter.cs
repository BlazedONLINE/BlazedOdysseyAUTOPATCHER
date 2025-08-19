using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using BlazedOdyssey.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Lightweight adapter to integrate with Tank & Healer Studio's Ultimate Chat Box without
/// taking a direct compile-time dependency. It locates the chat box at runtime and
/// toggles its input field with Enter/Escape. Gameplay input is blocked by your controller
/// while any TMP_InputField is focused (already implemented).
/// 
/// Reference: Ultimate Chat Box docs (Getting Started)
/// https://docs.tankandhealerstudio.com/assets/ultimatechatbox/introduction/getting-started
/// </summary>
[DefaultExecutionOrder(-10000)]
public class UltimateChatAdapter : MonoBehaviour
{
	[SerializeField]
	private string localPlayerName = "Player";
	[SerializeField]
	private bool persistAcrossScenes = false; // uncheck to edit per-scene; check to carry chat across scenes
	[SerializeField]
	private bool onlyShowInGameplayScenes = true; // Only show chat in gameplay scenes, not character selection
	
	[Header("Emoji Configuration")]
	[SerializeField]
	private bool enableEmojis = true; // Enable emoji support in chat
	[SerializeField]
	private bool enableEmojiWindow = true; // Enable clickable emoji selection window
	[SerializeField]
	private TMP_SpriteAsset emojiAsset; // Assign the Ultimate Emojis asset here if using emojis

	private Component chatBox; // TankAndHealerStudioAssets.UltimateChatBox instance
	private MethodInfo enableInputFieldMethod;
	private MethodInfo disableInputFieldMethod;
	private MethodInfo toggleInputFieldMethod;
	private MethodInfo registerChatMethod; // Optional, for manual sends if needed
	private MethodInfo enableMethod;
	private MethodInfo disableMethod;
	private PropertyInfo interactableProperty;
	private PropertyInfo inputFieldEnabledProperty;
	private FieldInfo inputFieldEnabledField;
	private FieldInfo inputFieldField;
	private FieldInfo useInputFieldField;
	private EventInfo onSubmittedEvent;
	private TMP_InputField fallbackInputField; // Fallback focus target
	[SerializeField] private bool showDebugLogs = false;

    // Runtime mapping from text emoticons (":)") to emoji sprite names in the assigned TMP Sprite Asset
    private readonly Dictionary<string, string[]> _emojiKeywordMap = new()
    {
        { ":)", new[]{ "smile", "slight", "happy" } },
        { ":D", new[]{ "grin", "grinning" } },
        { ";)", new[]{ "wink" } },
        { ":P", new[]{ "tongue", "stuck_out" } },
        { ":p", new[]{ "tongue", "stuck_out" } },
        { ":(", new[]{ "frown", "sad" } },
        { ":o", new[]{ "open_mouth", "astonish", "surprise" } },
        { ":O", new[]{ "open_mouth", "astonish", "surprise" } },
        { "XD", new[]{ "laugh", "joy", "laughing" } },
        { "<3", new[]{ "heart" } }
    };
    private readonly Dictionary<string, string> _resolvedSpriteNameByEmoticon = new();

	void Awake()
	{
		// Pre-disable emoji features early to avoid UltimateChatBox.Start NRE if no emoji asset is assigned
		TryPreDisableEmojiIfMissingAsset();

		// Check if we should show chat in this scene
		if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowChat())
		{
			if (showDebugLogs)
				Debug.Log($"[UltimateChatAdapter] Disabling chat - not in gameplay scene (Current: {SceneStateDetector.GetSceneTypeDisplayName()})");
			gameObject.SetActive(false);
			return;
		}

		if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
		LocateChatBox();
		if (persistAcrossScenes) PersistChatRoot();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	/// <summary>
	/// Run as early as possible to ensure UltimateChatBox won't access a null emoji asset in Start.
	/// If emojiAsset is missing, force-disable both text emoji and emoji window flags on all chat boxes found.
	/// </summary>
	private void TryPreDisableEmojiIfMissingAsset()
	{
		try
		{
			// Find UltimateChatBox type
			Type chatType = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
				.FirstOrDefault(t => t.FullName != null && t.FullName.EndsWith("UltimateChatBox", StringComparison.Ordinal));
			if (chatType == null) return;

			// Iterate all existing instances (even if inactive) and disable emoji features if no asset
			var instances = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
				.Where(m => m != null && chatType.IsInstanceOfType(m));
			var useEmojiWindowField = chatType.GetField("useEmojiWindow", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			var useTextEmojiField = chatType.GetField("useTextEmoji", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			var emojiAssetField = chatType.GetField("emojiAsset", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var inst in instances)
			{
				var currentEmoji = emojiAssetField != null ? emojiAssetField.GetValue(inst) as TMP_SpriteAsset : null;
				if (currentEmoji == null)
				{
					if (useEmojiWindowField != null) useEmojiWindowField.SetValue(inst, false);
					if (useTextEmojiField != null) useTextEmojiField.SetValue(inst, false);
					if (showDebugLogs) Debug.Log("[UltimateChatAdapter] Pre-disabled emoji features to avoid NRE (no emoji asset)");
				}
			}
		}
		catch { }
	}

	void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		UnsubscribeSubmitted();
	}

	void Update()
	{
		// Check if we should show chat in this scene
		if (onlyShowInGameplayScenes && !SceneStateDetector.ShouldShowChat())
		{
			return; // Don't process input if not in gameplay scene
		}

		EnsureEventSystem();
		// Lazy-locate chat box on demand if missing
		if (chatBox == null)
		{
			LocateChatBox();
		}

		bool uiTyping = IsAnyInputFieldFocused();

		// Enter: toggle into typing, or submit then exit typing next frame
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			if (!uiTyping)
			{
				TryOpenInputFieldRobust();
			}
			else
			{
				// Let the chat box submit first, then unfocus on the following frame
				StartCoroutine(DisableInputFieldNextFrame());
			}
		}

		// Esc: cancel typing
		if (uiTyping && Input.GetKeyDown(KeyCode.Escape))
		{
			DisableInputField();
		}
	}

	private void LocateChatBox()
	{
		// CRITICAL: Ensure we have a proper Canvas before trying to instantiate chat box
		EnsureChatCanvas();
		
		// Find UltimateChatBox type across loaded assemblies
		Type chatType = AppDomain.CurrentDomain
			.GetAssemblies()
			.SelectMany(a => {
				try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
			})
			.FirstOrDefault(t => t.FullName != null && t.FullName.EndsWith("UltimateChatBox", StringComparison.Ordinal));

		if (chatType == null)
		{
			if (showDebugLogs) Debug.LogWarning("UltimateChatAdapter: Could not find type 'UltimateChatBox'. Ensure the prefab is in the scene.");
			return;
		}

		// Find instance in scene
		var monos = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		chatBox = monos.FirstOrDefault(m => m != null && chatType.IsInstanceOfType(m));
		if (chatBox == null)
		{
			if (showDebugLogs) Debug.LogWarning("UltimateChatAdapter: No UltimateChatBox instance found in the scene. Attempting to create one from prefab...");
			
			// Try to instantiate a chat box prefab
			chatBox = TryInstantiateChatBoxPrefab();
			
			if (chatBox == null)
			{
				if (showDebugLogs) Debug.LogWarning("UltimateChatAdapter: Could not create chat box. Ensure a ChatBox prefab exists in Resources or scene.");
				return;
			}
		}

		// Cache methods (match actual signatures from docs/code)
		enableInputFieldMethod = chatType.GetMethod("EnableInputField", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);
		disableInputFieldMethod = chatType.GetMethod("DisableInputField", BindingFlags.Public | BindingFlags.Instance);
		toggleInputFieldMethod = chatType.GetMethod("ToggleInputField", BindingFlags.Public | BindingFlags.Instance);
		registerChatMethod = chatType.GetMethod("RegisterChat", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(string) }, null);
		enableMethod = chatType.GetMethod("Enable", BindingFlags.Public | BindingFlags.Instance);
		disableMethod = chatType.GetMethod("Disable", BindingFlags.Public | BindingFlags.Instance);
		interactableProperty = chatType.GetProperty("Interactable", BindingFlags.Public | BindingFlags.Instance);
		inputFieldEnabledProperty = chatType.GetProperty("InputFieldEnabled", BindingFlags.Public | BindingFlags.Instance);
		inputFieldEnabledField = chatType.GetField("InputFieldEnabled", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		inputFieldField = chatType.GetField("inputField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		useInputFieldField = chatType.GetField("useInputField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		onSubmittedEvent = chatType.GetEvent("OnInputFieldSubmitted", BindingFlags.Public | BindingFlags.Instance);

		// Fallback input field reference
		fallbackInputField = (chatBox as Component)?.GetComponentInChildren<TMP_InputField>(true);

		// Ensure emoji config is safe before Start runs
		EnsureEmojiConfig(chatType);

		// Ensure input field usage enabled, enabled & interactable and subscribe
		EnsureUseInputFieldTrue();
		EnsureEnabledInteractable();
		SubscribeSubmitted();
	}

	private void PersistChatRoot()
	{
		var comp = chatBox as Component;
		if (comp == null) return;
		var root = comp.transform.root;
		if (root != null) DontDestroyOnLoad(root.gameObject);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Re-evaluate if we should show chat in the new scene
		if (onlyShowInGameplayScenes)
		{
			bool shouldShow = SceneStateDetector.ShouldShowChat();
			gameObject.SetActive(shouldShow);
			
			if (showDebugLogs)
				Debug.Log($"[UltimateChatAdapter] Scene loaded: {scene.name}, Chat active: {shouldShow}");
				
			if (!shouldShow)
				return; // Don't try to locate chat box if we shouldn't show in this scene
		}

		EnsureEventSystem();
		if (chatBox == null)
		{
			LocateChatBox();
			if (persistAcrossScenes) PersistChatRoot();
		}
		else
		{
			EnsureEnabledInteractable();
		}
	}

	private bool IsAnyInputFieldFocused()
	{
		var es = EventSystem.current;
		if (es == null) return false;
		var go = es.currentSelectedGameObject;
		if (go == null) return false;
		return go.GetComponent<TMP_InputField>() != null;
	}

	private void EnableInputField()
	{
		bool invoked = false;
		try { enableInputFieldMethod?.Invoke(chatBox, new object[] { string.Empty }); invoked = enableInputFieldMethod != null; } catch (Exception e) { if (showDebugLogs) Debug.LogWarning($"UltimateChatAdapter: EnableInputField failed - {e.Message}"); }
		if (!invoked)
		{
			// Fallback to directly focusing TMP input field
			if (fallbackInputField != null)
			{
				fallbackInputField.Select();
				fallbackInputField.ActivateInputField();
			}
		}
	}

	private void TryOpenInputFieldRobust()
	{
		// Ensure the chat box is enabled and interactable
		EnsureUseInputFieldTrue();
		EnsureEnabledInteractable();
		try { enableMethod?.Invoke(chatBox, null); } catch { }

		// Attempt direct EnableInputField
		EnableInputField();

		// Verify if input is enabled; if not, attempt ToggleInputField
		if (!IsInputFieldEnabled())
		{
			try { toggleInputFieldMethod?.Invoke(chatBox, null); } catch { }
		}

		// If still not enabled, try to directly enable the TMP_InputField
		if (!IsInputFieldEnabled())
		{
			var tf = GetChatInputField();
			if (tf != null)
			{
				// Make sure it's enabled and interactable
				var comp = tf as Behaviour;
				if (comp != null) comp.enabled = true;
				tf.interactable = true;
				var es = EventSystem.current;
				if (es != null) es.SetSelectedGameObject(tf.gameObject);
				// Multi-frame ensure focus
				StartCoroutine(EnsureFocusedRoutine(tf));
			}
		}
	}

	private System.Collections.IEnumerator EnsureFocusedRoutine(TMP_InputField tf)
	{
		for (int i = 0; i < 5; i++)
		{
			yield return null;
			if (tf == null) yield break;
			var comp = tf as Behaviour;
			if (comp != null) comp.enabled = true;
			tf.interactable = true;
			var es = EventSystem.current;
			if (es != null) es.SetSelectedGameObject(tf.gameObject);
			tf.Select();
			tf.ActivateInputField();
			// Exit early if chat reports enabled
			if (IsInputFieldEnabled()) yield break;
		}
	}

	private bool IsInputFieldEnabled()
	{
		try
		{
			if (inputFieldEnabledProperty != null)
				return (bool)inputFieldEnabledProperty.GetValue(chatBox);
			if (inputFieldEnabledField != null)
				return (bool)inputFieldEnabledField.GetValue(chatBox);
		}
		catch { }
		return false;
	}

	private TMP_InputField GetChatInputField()
	{
		if (fallbackInputField != null) return fallbackInputField;
		try
		{
			if (inputFieldField != null)
				return inputFieldField.GetValue(chatBox) as TMP_InputField;
		}
		catch { }
		return null;
	}

	private void DisableInputField()
	{
		bool invoked = false;
		try { disableInputFieldMethod?.Invoke(chatBox, null); invoked = disableInputFieldMethod != null; } catch (Exception e) { if (showDebugLogs) Debug.LogWarning($"UltimateChatAdapter: DisableInputField failed - {e.Message}"); }
		if (!invoked)
		{
			var es = EventSystem.current;
			if (es != null && es.currentSelectedGameObject != null)
				es.SetSelectedGameObject(null);
			if (fallbackInputField != null)
				fallbackInputField.DeactivateInputField();
		}
	}

	private System.Collections.IEnumerator DisableInputFieldNextFrame()
	{
		yield return null; // allow chat to process submit this frame
		DisableInputField();
	}

	/// <summary>
	/// Optional helper to register a message directly to the chat box.
	/// Call this from your networking layer when receiving a chat message.
	/// </summary>
	public void RegisterMessage(string sender, string message)
	{
		try { registerChatMethod?.Invoke(chatBox, new object[] { sender, message }); } catch (Exception e) { if (showDebugLogs) Debug.LogWarning($"UltimateChatAdapter: RegisterChat failed - {e.Message}"); }
	}

	private void EnsureEventSystem()
	{
		if (EventSystem.current == null)
		{
			var es = new GameObject("EventSystem", typeof(EventSystem));
			es.AddComponent<StandaloneInputModule>();
			if (showDebugLogs) Debug.Log("UltimateChatAdapter: Created EventSystem.");
		}
	}

	private void EnsureEnabledInteractable()
	{
		try { enableMethod?.Invoke(chatBox, null); } catch { }
		try { interactableProperty?.SetValue(chatBox, true); } catch { }
	}

	private void EnsureUseInputFieldTrue()
	{
		try { if (useInputFieldField != null) useInputFieldField.SetValue(chatBox, true); } catch { }
	}

	// Subscribe to OnInputFieldSubmitted to forward messages via RegisterChat
	private Delegate cachedSubmitDelegate;
	private void SubscribeSubmitted()
	{
		if (onSubmittedEvent == null || chatBox == null) return;
		try
		{
			// event Action<string> OnInputFieldSubmitted
			var handlerMethod = typeof(UltimateChatAdapter).GetMethod(nameof(OnChatSubmitted), BindingFlags.NonPublic | BindingFlags.Instance);
			cachedSubmitDelegate = Delegate.CreateDelegate(onSubmittedEvent.EventHandlerType, this, handlerMethod);
			onSubmittedEvent.AddEventHandler(chatBox, cachedSubmitDelegate);
		}
		catch (Exception e)
		{
			if (showDebugLogs) Debug.LogWarning($"UltimateChatAdapter: SubscribeSubmitted failed - {e.Message}");
		}
	}

	private void UnsubscribeSubmitted()
	{
		if (onSubmittedEvent == null || chatBox == null || cachedSubmitDelegate == null) return;
		try { onSubmittedEvent.RemoveEventHandler(chatBox, cachedSubmitDelegate); } catch { }
		cachedSubmitDelegate = null;
	}

	private void OnChatSubmitted(string value)
	{
		if (string.IsNullOrWhiteSpace(value)) return;
		// Replace common text emoticons with emoji shortcodes recognizable by the emoji asset
		var normalized = ReplaceTextEmoticonsWithSpriteTags(value);
		var sender = string.IsNullOrEmpty(localPlayerName) ? "Player" : localPlayerName;
		RegisterMessage(sender, normalized);
	}

	// Convert emoticons to TMP sprite tags using the actual sprite names present in the assigned emoji asset
	private string ReplaceTextEmoticonsWithSpriteTags(string input)
	{
		if (string.IsNullOrEmpty(input)) return input;
		var emoji = GetAssignedEmojiAsset();
		if (emoji == null) return input;

		// Build/remember a mapping from emoticon -> actual sprite name present in the asset
		foreach (var kv in _emojiKeywordMap)
		{
			if (_resolvedSpriteNameByEmoticon.ContainsKey(kv.Key)) continue;
			var spriteName = FindSpriteNameByKeywords(emoji, kv.Value);
			if (!string.IsNullOrEmpty(spriteName))
				_resolvedSpriteNameByEmoticon[kv.Key] = spriteName;
		}

		// Replace occurrences with TMP sprite tags: <sprite name="...">
		// Use regex to replace standalone tokens and common punctuation boundaries
		foreach (var kv in _resolvedSpriteNameByEmoticon)
		{
			var emoticon = Regex.Escape(kv.Key);
			var tag = $"<sprite name=\"{kv.Value}\">";
			input = Regex.Replace(input, emoticon, tag);
		}
		return input;
	}

	private TMP_SpriteAsset GetAssignedEmojiAsset()
	{
		if (emojiAsset != null) return emojiAsset;
		if (chatBox == null) return null;
		try
		{
			var f = chatBox.GetType().GetField("emojiAsset", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (f != null) return f.GetValue(chatBox) as TMP_SpriteAsset;
		}
		catch { }
		return null;
	}

	private string FindSpriteNameByKeywords(TMP_SpriteAsset asset, string[] keywords)
	{
		if (asset == null || asset.spriteCharacterTable == null) return null;
		foreach (var ch in asset.spriteCharacterTable)
		{
			var n = ch.name?.ToLower();
			if (string.IsNullOrEmpty(n)) continue;
			for (int i = 0; i < keywords.Length; i++)
			{
				if (n.Contains(keywords[i])) return ch.name; // return actual name used in the asset
			}
		}
		return null;
	}

	private void EnsureEmojiConfig(Type chatType)
	{
		// Configure emoji support based on adapter settings
		try
		{
			var emojiAssetField = chatType.GetField("emojiAsset", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			var useEmojiWindowField = chatType.GetField("useEmojiWindow", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			var useTextEmojiField = chatType.GetField("useTextEmoji", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			
			// Apply emoji settings from adapter configuration
			if (useTextEmojiField != null)
				useTextEmojiField.SetValue(chatBox, enableEmojis);
				
			if (useEmojiWindowField != null)
				useEmojiWindowField.SetValue(chatBox, enableEmojis && enableEmojiWindow);
			
			// Try to load and assign the emoji asset if emojis are enabled
			if (enableEmojis && emojiAssetField != null)
			{
				var currentEmojiAsset = emojiAssetField.GetValue(chatBox) as TMP_SpriteAsset;
				
				// If no emoji asset is assigned, try to load the default one
				if (currentEmojiAsset == null)
				{
					var defaultEmojiAsset = LoadDefaultEmojiAsset();
					if (defaultEmojiAsset != null)
					{
						emojiAssetField.SetValue(chatBox, defaultEmojiAsset);
						if (showDebugLogs) Debug.Log("UltimateChatAdapter: Loaded default emoji asset and enabled emoji features.");
					}
					else
					{
						// Disable emoji features if we can't load the asset
						if (useEmojiWindowField != null)
							useEmojiWindowField.SetValue(chatBox, false);
						if (useTextEmojiField != null)
							useTextEmojiField.SetValue(chatBox, false);
						if (showDebugLogs) Debug.LogWarning("UltimateChatAdapter: Could not load emoji asset. Emoji features disabled.");
					}
				}
				else
				{
					if (showDebugLogs) Debug.Log("UltimateChatAdapter: Emoji asset already assigned. Emoji features enabled.");
				}
			}
			else if (!enableEmojis)
			{
				// Explicitly disable emoji features if disabled in adapter
				if (useEmojiWindowField != null)
					useEmojiWindowField.SetValue(chatBox, false);
				if (useTextEmojiField != null)
					useTextEmojiField.SetValue(chatBox, false);
				if (showDebugLogs) Debug.Log("UltimateChatAdapter: Emoji features disabled via adapter configuration.");
			}
		}
		catch (System.Exception e)
		{
			if (showDebugLogs) Debug.LogWarning($"UltimateChatAdapter: Failed to configure emoji settings: {e.Message}");
		}
	}

	private TMP_SpriteAsset LoadDefaultEmojiAsset()
	{
		// First, try to use the manually assigned emoji asset
		if (emojiAsset != null)
		{
			if (showDebugLogs) Debug.Log("UltimateChatAdapter: Using manually assigned emoji asset");
			return emojiAsset;
		}

		// Try to auto-find the default Ultimate Chat emoji asset
		try
		{
			#if UNITY_EDITOR
			// In editor, try using AssetDatabase to find the emoji asset
			string[] guids = UnityEditor.AssetDatabase.FindAssets("Ultimate Emojis t:TMP_SpriteAsset");
			if (guids.Length > 0)
			{
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
				var foundEmojiAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(assetPath);
				if (foundEmojiAsset != null)
				{
					if (showDebugLogs) Debug.Log($"UltimateChatAdapter: Auto-found emoji asset via AssetDatabase at {assetPath}");
					return foundEmojiAsset;
				}
			}
			#endif

			if (showDebugLogs) Debug.LogWarning("UltimateChatAdapter: Could not auto-find Ultimate Emojis asset. Please assign it manually in the adapter.");
			return null;
		}
		catch (System.Exception e)
		{
			if (showDebugLogs) Debug.LogWarning($"UltimateChatAdapter: Error loading emoji asset: {e.Message}");
			return null;
		}
	}

	private Component TryInstantiateChatBoxPrefab()
	{
		// Try to load the chat box prefab using direct asset path
		#if UNITY_EDITOR
		// In editor, we can use AssetDatabase to find the prefab
		string[] assetPaths = UnityEditor.AssetDatabase.FindAssets("ChatBox_Outline t:Prefab");
		if (assetPaths.Length > 0)
		{
			string prefabPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetPaths[0]);
			var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			if (prefab != null)
			{
				if (showDebugLogs) Debug.Log($"UltimateChatAdapter: Found prefab via AssetDatabase at {prefabPath}, instantiating...");
				
				// Let the Ultimate Chat Box handle its own canvas detection/creation
				// According to docs: "automatically set itself as a child of a UI Canvas or create one if necessary"
				var instance = Instantiate(prefab);
				
				// Give it a frame to initialize before trying to position it
				StartCoroutine(PositionChatBoxAfterFrame(instance));
				
				var chatComponent = instance.GetComponent<MonoBehaviour>();
				
				// Check if this is actually an UltimateChatBox
				if (chatComponent != null && chatComponent.GetType().FullName.EndsWith("UltimateChatBox"))
				{
					if (showDebugLogs) Debug.Log($"UltimateChatAdapter: Successfully created chat box from prefab");
					return chatComponent;
				}
				else
				{
					// Not the right component, destroy and try next
					DestroyImmediate(instance);
				}
			}
		}
		#endif

		// Fallback: Try Resources folder
		string[] prefabPaths = {
			"ChatBox_Outline",
			"ChatBox_Rounded", 
			"ChatBox_Square"
		};

		foreach (var path in prefabPaths)
		{
			var prefab = Resources.Load<GameObject>(path);
			if (prefab != null)
			{
				if (showDebugLogs) Debug.Log($"UltimateChatAdapter: Found prefab in Resources at {path}, instantiating...");
				
				// Let the Ultimate Chat Box handle its own canvas detection/creation
				// According to docs: "automatically set itself as a child of a UI Canvas or create one if necessary"
				var instance = Instantiate(prefab);
				
				// Give it a frame to initialize before trying to position it
				StartCoroutine(PositionChatBoxAfterFrame(instance));
				
				var chatComponent = instance.GetComponent<MonoBehaviour>();
				
				// Check if this is actually an UltimateChatBox
				if (chatComponent != null && chatComponent.GetType().FullName.EndsWith("UltimateChatBox"))
				{
					if (showDebugLogs) Debug.Log($"UltimateChatAdapter: Successfully created chat box from Resources");
					return chatComponent;
				}
				else
				{
					// Not the right component, destroy and try next
					DestroyImmediate(instance);
				}
			}
		}

		if (showDebugLogs) Debug.LogWarning("UltimateChatAdapter: Could not find any ChatBox prefab. Please manually drag a ChatBox prefab into the scene or move one to a Resources folder.");
		return null;
	}

	private void EnsureChatCanvas()
	{
		// Check if there's already a suitable canvas for Ultimate Chat Box
		var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (var canvas in canvases)
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				if (showDebugLogs) Debug.Log($"UltimateChatAdapter: Found existing overlay canvas: {canvas.name}");
				return; // We have a suitable canvas
			}
		}

		// If we don't have any overlay canvas, create one specifically for chat
		// This is what Ultimate Chat Box expects to find
		if (showDebugLogs) Debug.Log("UltimateChatAdapter: Creating dedicated chat canvas...");
		var chatCanvasGO = new GameObject("UltimateChatCanvas");
		var chatCanvas = chatCanvasGO.AddComponent<Canvas>();
		chatCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		chatCanvas.sortingOrder = 1000; // Above everything else
		
		// Add required components for UI interaction
		chatCanvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
		
		// Add CanvasScaler for proper scaling
		var scaler = chatCanvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
		scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		scaler.matchWidthOrHeight = 0.5f;
		
		if (showDebugLogs) Debug.Log("UltimateChatAdapter: Created dedicated chat canvas for Ultimate Chat Box");
	}

	private System.Collections.IEnumerator PositionChatBoxAfterFrame(GameObject chatBoxInstance)
	{
		// Wait a frame for the Ultimate Chat Box to complete its own initialization
		yield return null;
		
		if (chatBoxInstance == null) yield break;

		// Now try to position it - the chat box should have found/created its own canvas by now
		var rectTransform = chatBoxInstance.GetComponent<RectTransform>();
		if (rectTransform != null)
		{
			// Only adjust position if the chat box has been properly initialized
			if (rectTransform.parent != null)
			{
				rectTransform.anchorMin = new Vector2(0, 0);
				rectTransform.anchorMax = new Vector2(0, 0);
				rectTransform.pivot = new Vector2(0, 0);
				rectTransform.anchoredPosition = new Vector2(10, 10); // 10px from bottom-left
				
				if (showDebugLogs) Debug.Log($"UltimateChatAdapter: Positioned chat box at bottom-left corner under {rectTransform.parent.name}");
			}
			else
			{
				if (showDebugLogs) Debug.LogWarning("UltimateChatAdapter: Chat box has no parent after initialization - Ultimate Chat Box failed to find/create canvas");
			}
		}
	}
}


