using UnityEngine;
using UnityEngine.UI;
using TMPro; // Requires TextMeshPro
using UnityEngine.InputSystem; // New Input System
using System.Collections;

public class LoginManager : MonoBehaviour
{
	[Header("Login Panel")]
	public GameObject loginPanel;
	public TMP_InputField usernameLoginInput;
	public TMP_InputField passwordLoginInput;
	public Button loginButton;
	public Button goToRegisterButton;

	[Header("Register Panel")]
	public GameObject registerPanel;
	public TMP_InputField usernameRegisterInput;
	public TMP_InputField passwordRegisterInput;
	public TMP_InputField confirmPasswordRegisterInput;
	public TMP_InputField emailRegisterInput;
	public Button registerButton;
	public Button goToLoginButton;

	[Header("Status Messages")]
	public TextMeshProUGUI statusText;
	public float statusMessageDisplayTime = 3f;

	private Coroutine statusMessageCoroutine;
	private Canvas mainCanvas;

	void Awake()
	{
		Debug.Log("🎮 LoginManager initializing...");

		// If scalable Login UI exists, this legacy builder should step aside
		var scalableLogin = FindFirstObjectByType<BlazedOdyssey.UI.Login.LoginController>();
		if (scalableLogin != null)
		{
			Debug.Log("✅ Detected scalable Login UI (LoginController). Disabling legacy LoginManager UI generation.");
			enabled = false;
			return;
		}

		// Always set up basic UI when scene loads to ensure something is visible
		SetupBasicUI();
		
		// Ensure only login panel is active at start
		ShowLoginPanel();

		// Assign button listeners if buttons exist
		SetupButtonListeners();
	}

	void Update()
	{
		// Quick login for testing using new Input System
		if (Keyboard.current != null)
		{
			if (Keyboard.current.f1Key.wasPressedThisFrame)
			{
				QuickLogin("TestPlayer");
			}
			
			if (Keyboard.current.f2Key.wasPressedThisFrame)
			{
				QuickLogin("DevTester");
			}
			
			// Submit on Enter key
			if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
			{
				if (loginPanel != null && loginPanel.activeInHierarchy)
				{
					OnLoginButtonClicked();
				}
				else if (registerPanel != null && registerPanel.activeInHierarchy)
				{
					OnRegisterButtonClicked();
				}
			}
		}
	}
	
	void SetupBasicUI()
	{
		// Build a minimal UI so legacy scenes show something immediately
		Debug.Log("🎨 Setting up basic login UI structure...");
		
		// Ensure we have a proper camera with CORRECT settings
		Camera cam = Camera.main;
		if (cam == null)
		{
			Debug.Log("🎨 No MainCamera found, creating one...");
			GameObject camGO = new GameObject("Main Camera");
			cam = camGO.AddComponent<Camera>();
			cam.tag = "MainCamera";
			cam.orthographic = true;
			cam.orthographicSize = 5f;
			cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
			cam.clearFlags = CameraClearFlags.SolidColor;
			cam.transform.position = new Vector3(0, 0, -10);
		}
		else
		{
			Debug.Log($"🎨 Found existing camera: {cam.name} at {cam.transform.position}");
			// FORCE the camera to use our settings
			cam.orthographic = true;
			cam.orthographicSize = 5f; // This was 0.08660255 - WAY too small!
			cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
			cam.clearFlags = CameraClearFlags.SolidColor;
			cam.transform.position = new Vector3(0, 0, -10);
		}
		
		// Create or find canvas
		mainCanvas = FindFirstObjectByType<Canvas>();
		if (mainCanvas == null)
		{
			Debug.Log("🎨 No Canvas found, creating one...");
			GameObject canvasObj = new GameObject("Login Canvas");
			mainCanvas = canvasObj.AddComponent<Canvas>();
			mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			mainCanvas.sortingOrder = 1;
			
			// Add canvas scaler
			var scaler = canvasObj.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			scaler.matchWidthOrHeight = 0.5f;
			
			// Add graphic raycaster
			canvasObj.AddComponent<GraphicRaycaster>();
		}
		
		// Ensure mainCanvas is not null before proceeding
		if (mainCanvas == null)
		{
			Debug.LogError("❌ Failed to create Canvas! Cannot proceed with UI creation.");
			return;
		}
		
		Debug.Log($"✅ Canvas ready: {mainCanvas.name}");
		
		// Create login panel if it doesn't exist
		if (loginPanel == null)
		{
			CreateLoginPanel();
		}
		
		// Create register panel if it doesn't exist
		if (registerPanel == null)
		{
			CreateRegisterPanel();
		}
		
		// Create status text if it doesn't exist
		if (statusText == null)
		{
			CreateStatusText();
		}
		
		Debug.Log("🎨 Basic UI setup complete!");
	}
	
	void CreateLoginPanel()
	{
		Debug.Log("🔐 Creating login panel...");
		
		loginPanel = new GameObject("Login Panel");
		loginPanel.transform.SetParent(mainCanvas.transform, false);
		
		// Create background
		var background = CreatePanel(loginPanel, new Vector2(400, 500), new Color(0.05f, 0.05f, 0.1f, 0.95f));
		
		// Create title
		CreateText(loginPanel, "BLAZED ODYSSEY", new Vector2(0, 180), 32, new Color(1f, 0.6f, 0.0f, 1f));
		
		// Create username input
		usernameLoginInput = CreateInputField(loginPanel, "Username", new Vector2(0, 80));
		
		// Create password input
		passwordLoginInput = CreateInputField(loginPanel, "Password", new Vector2(0, 20));
		passwordLoginInput.contentType = TMP_InputField.ContentType.Password;
		
		// Create login button
		loginButton = CreateButton(loginPanel, "LOGIN", new Vector2(0, -80), new Color(0.2f, 0.6f, 0.2f));
		
		// Create register button
		goToRegisterButton = CreateButton(loginPanel, "REGISTER", new Vector2(0, -120), new Color(0.2f, 0.4f, 0.8f));
		
		// Create exit button
		var exitButton = CreateButton(loginPanel, "EXIT", new Vector2(0, -160), new Color(0.6f, 0.2f, 0.2f));
		exitButton.onClick.AddListener(() => {
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		});
		
		Debug.Log("🔐 Login panel created successfully!");
	}
	
	void CreateRegisterPanel()
	{
		Debug.Log("📝 Creating register panel...");
		
		registerPanel = new GameObject("Register Panel");
		registerPanel.transform.SetParent(mainCanvas.transform, false);
		registerPanel.SetActive(false);
		
		// Create background
		var background = CreatePanel(registerPanel, new Vector2(400, 600), new Color(0.05f, 0.05f, 0.1f, 0.95f));
		
		// Create title
		CreateText(registerPanel, "CREATE ACCOUNT", new Vector2(0, 240), 28, new Color(1f, 0.6f, 0.0f, 1f));
		
		// Create input fields
		usernameRegisterInput = CreateInputField(registerPanel, "Username", new Vector2(0, 140));
		passwordRegisterInput = CreateInputField(registerPanel, "Password", new Vector2(0, 80));
		passwordRegisterInput.contentType = TMP_InputField.ContentType.Password;
		confirmPasswordRegisterInput = CreateInputField(registerPanel, "Confirm Password", new Vector2(0, 20));
		confirmPasswordRegisterInput.contentType = TMP_InputField.ContentType.Password;
		emailRegisterInput = CreateInputField(registerPanel, "Email", new Vector2(0, -40));
		
		// Create buttons
		registerButton = CreateButton(registerPanel, "CREATE ACCOUNT", new Vector2(0, -120), new Color(0.2f, 0.6f, 0.2f));
		goToLoginButton = CreateButton(registerPanel, "BACK TO LOGIN", new Vector2(0, -160), new Color(0.4f, 0.4f, 0.4f));
		
		Debug.Log("📝 Register panel created successfully!");
	}
	
	void CreateStatusText()
	{
		Debug.Log("💬 Creating status text...");
		
		var statusObj = new GameObject("Status Text");
		statusObj.transform.SetParent(mainCanvas.transform, false);
		
		statusText = statusObj.AddComponent<TextMeshProUGUI>();
		statusText.text = "";
		statusText.fontSize = 18;
		statusText.color = Color.white;
		statusText.alignment = TextAlignmentOptions.Center;
		
		var statusRect = statusObj.AddComponent<RectTransform>();
		statusRect.anchoredPosition = new Vector2(0, -250);
		statusRect.sizeDelta = new Vector2(600, 50);
		
		Debug.Log("💬 Status text created successfully!");
	}
	
	// Helper methods for creating UI elements
	GameObject CreatePanel(GameObject parent, Vector2 size, Color color)
	{
		var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
		panel.transform.SetParent(parent.transform, false);
		
		var image = panel.GetComponent<Image>();
		image.color = color;
		
		var rect = panel.GetComponent<RectTransform>();
		rect.anchoredPosition = Vector2.zero;
		rect.sizeDelta = size;
		
		return panel;
	}
	
	TMP_InputField CreateInputField(GameObject parent, string placeholder, Vector2 position)
	{
		var inputObj = new GameObject("Input Field");
		inputObj.transform.SetParent(parent.transform, false);
		
		// CRITICAL: Add RectTransform first before any other components
		var inputRect = inputObj.GetComponent<RectTransform>();
		if (inputRect == null) inputRect = inputObj.AddComponent<RectTransform>();
		inputRect.anchoredPosition = position;
		inputRect.sizeDelta = new Vector2(300, 40);
		
		var input = inputObj.AddComponent<TMP_InputField>();
		
		// Create background
		var background = new GameObject("Background");
		background.transform.SetParent(inputObj.transform, false);
		var bgImage = background.AddComponent<Image>();
		bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
		
		var bgRect = background.AddComponent<RectTransform>();
		bgRect.anchoredPosition = Vector2.zero;
		bgRect.sizeDelta = new Vector2(300, 40);
		
		// Create text area
		var textArea = new GameObject("Text Area");
		textArea.transform.SetParent(background.transform, false);
		var textAreaRect = textArea.AddComponent<RectTransform>();
		textAreaRect.anchoredPosition = Vector2.zero;
		textAreaRect.sizeDelta = new Vector2(280, 30);
		
		// Create text component
		var textComponent = new GameObject("Text");
		textComponent.transform.SetParent(textArea.transform, false);
		var text = textComponent.AddComponent<TextMeshProUGUI>();
		text.text = "";
		text.color = Color.white;
		text.fontSize = 18;
		
		var textRect = textComponent.GetComponent<RectTransform>();
		if (textRect == null) textRect = textComponent.AddComponent<RectTransform>();
		textRect.anchoredPosition = Vector2.zero;
		textRect.sizeDelta = new Vector2(280, 30);
		
		// Create placeholder
		var placeholderObj = new GameObject("Placeholder");
		placeholderObj.transform.SetParent(textArea.transform, false);
		var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
		placeholderText.text = placeholder;
		placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
		placeholderText.fontSize = 18;
		
		var placeholderRect = placeholderObj.GetComponent<RectTransform>();
		if (placeholderRect == null) placeholderRect = placeholderObj.AddComponent<RectTransform>();
		placeholderRect.anchoredPosition = Vector2.zero;
		placeholderRect.sizeDelta = new Vector2(280, 30);
		
		// Setup input field
		input.textComponent = text;
		input.placeholder = placeholderText;
		input.textViewport = textAreaRect;
		
		return input;
	}
	
	Button CreateButton(GameObject parent, string text, Vector2 position, Color color)
	{
		var buttonObj = new GameObject("Button");
		buttonObj.transform.SetParent(parent.transform, false);
		
		var button = buttonObj.AddComponent<Button>();
		var image = buttonObj.AddComponent<Image>();
		image.color = color;
		
		// Create text
		var textObj = new GameObject("Text");
		textObj.transform.SetParent(buttonObj.transform, false);
		var buttonText = textObj.AddComponent<TextMeshProUGUI>();
		buttonText.text = text;
		buttonText.color = Color.white;
		buttonText.fontSize = 18;
		buttonText.alignment = TextAlignmentOptions.Center;
		buttonText.fontStyle = FontStyles.Bold;
		
		var textRect = textObj.GetComponent<RectTransform>();
		if (textRect == null) textRect = textObj.AddComponent<RectTransform>();
		textRect.anchoredPosition = Vector2.zero;
		textRect.sizeDelta = new Vector2(200, 40);
		
		var buttonRect = buttonObj.GetComponent<RectTransform>();
		if (buttonRect == null) buttonRect = buttonObj.AddComponent<RectTransform>();
		buttonRect.anchoredPosition = position;
		buttonRect.sizeDelta = new Vector2(200, 40);
		
		return button;
	}
	
	TextMeshProUGUI CreateText(GameObject parent, string text, Vector2 position, int fontSize, Color color)
	{
		var textObj = new GameObject("Text");
		textObj.transform.SetParent(parent.transform, false);
		
		var textComponent = textObj.AddComponent<TextMeshProUGUI>();
		textComponent.text = text;
		textComponent.fontSize = fontSize;
		textComponent.color = color;
		textComponent.alignment = TextAlignmentOptions.Center;
		textComponent.fontStyle = FontStyles.Bold;
		
		var textRect = textObj.AddComponent<RectTransform>();
		textRect.anchoredPosition = position;
		textRect.sizeDelta = new Vector2(400, 50);
		
		return textComponent;
	}

	void SetupButtonListeners()
	{
		Debug.Log("🔗 Setting up button listeners...");
		
		if (loginButton != null)
		{
			loginButton.onClick.AddListener(OnLoginButtonClicked);
			Debug.Log("🔗 Login button listener added");
		}
		
		if (goToRegisterButton != null)
		{
			goToRegisterButton.onClick.AddListener(ShowRegisterPanel);
			Debug.Log("🔗 Go to register button listener added");
		}
		
		if (registerButton != null)
		{
			registerButton.onClick.AddListener(OnRegisterButtonClicked);
			Debug.Log("🔗 Register button listener added");
		}
		
		if (goToLoginButton != null)
		{
			goToLoginButton.onClick.AddListener(ShowLoginPanel);
			Debug.Log("🔗 Go to login button listener added");
		}
	}

	void ShowLoginPanel()
	{
		if (loginPanel != null) loginPanel.SetActive(true);
		if (registerPanel != null) registerPanel.SetActive(false);
		ClearStatusMessage();
		Debug.Log("📋 Showing login panel");
	}

	void ShowRegisterPanel()
	{
		if (loginPanel != null) loginPanel.SetActive(false);
		if (registerPanel != null) registerPanel.SetActive(true);
		ClearStatusMessage();
		Debug.Log("📋 Showing register panel");
	}

	void OnLoginButtonClicked()
	{
		string username = usernameLoginInput?.text ?? "";
		string password = passwordLoginInput?.text ?? "";

		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			ShowStatusMessage("Username and password cannot be empty.", true);
			return;
		}

		// TODO: Implement actual server login logic here
		// For now, simulate success/failure
		if (username == "testuser" && password == "password")
		{
			ShowStatusMessage("Login successful!", false);
			StartCoroutine(DelayedSceneTransition());
		}
		else
		{
			ShowStatusMessage("Invalid username or password.", true);
		}
	}

	void OnRegisterButtonClicked()
	{
		string username = usernameRegisterInput?.text ?? "";
		string password = passwordRegisterInput?.text ?? "";
		string confirmPassword = confirmPasswordRegisterInput?.text ?? "";
		string email = emailRegisterInput?.text ?? "";

		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || 
			string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(email))
		{
			ShowStatusMessage("All fields are required.", true);
			return;
		}

		if (password != confirmPassword)
		{
			ShowStatusMessage("Passwords do not match.", true);
			return;
		}

		// TODO: Implement actual server registration logic here
		ShowStatusMessage("Registration successful! You can now log in.", false);
		ShowLoginPanel();
	}

	void QuickLogin(string username)
	{
		Debug.Log($"🎮 Quick login as {username}...");
		ShowStatusMessage($"Quick login successful as {username}!", false);
		
		// Simulate successful login
		StartCoroutine(DelayedSceneTransition());
	}
	
	IEnumerator DelayedSceneTransition()
	{
		yield return new WaitForSeconds(1f);
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnLoginSuccess();
		}
		else
		{
			Debug.Log("🎮 Would transition to character selection (GameManager not found)");
		}
	}

	void ShowStatusMessage(string message, bool isError)
	{
		if (statusText == null) return;
		
		if (statusMessageCoroutine != null)
		{
			StopCoroutine(statusMessageCoroutine);
		}
		statusText.color = isError ? Color.red : Color.green;
		statusText.text = message;
		statusMessageCoroutine = StartCoroutine(ClearStatusMessageAfterDelay());
		
		Debug.Log($"💬 {(isError ? "Error" : "Info")}: {message}");
	}

	void ClearStatusMessage()
	{
		if (statusMessageCoroutine != null)
		{
			StopCoroutine(statusMessageCoroutine);
		}
		if (statusText != null)
		{
			statusText.text = "";
		}
	}

	IEnumerator ClearStatusMessageAfterDelay()
	{
		yield return new WaitForSeconds(statusMessageDisplayTime);
		if (statusText != null)
		{
			statusText.text = "";
		}
	}
}
