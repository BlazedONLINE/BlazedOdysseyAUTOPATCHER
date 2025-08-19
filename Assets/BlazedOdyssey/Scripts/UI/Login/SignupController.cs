using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlazedOdyssey.UI.Common;
using System.Threading.Tasks;

namespace BlazedOdyssey.UI.Login
{
    public class SignupController : MonoBehaviour
    {
        private TMP_InputField _username;
        private TMP_InputField _email;
        private TMP_InputField _password;
        private TMP_InputField _confirmPassword;
        private TMP_InputField _characterName;
        private Button _createBtn;
        private Button _backBtn;

        void Awake()
        {
            Debug.Log("🆕 SignupController.Awake() called");
        }

        void Start()
        {
            Debug.Log("🆕 SignupController.Start() called");
            
            // Auto-wire buttons if not already bound
            if (_createBtn == null || _backBtn == null)
            {
                Debug.Log("🔧 Buttons not bound, attempting auto-wire...");
                AutoWireComponents();
            }
        }
        
        private void AutoWireComponents()
        {
            // Find components by searching the SignupController's GameObject and children
            var inputFields = GetComponentsInChildren<TMP_InputField>();
            var buttons = GetComponentsInChildren<Button>();
            
            Debug.Log($"🔍 Found {inputFields.Length} input fields and {buttons.Length} buttons");
            
            // Map input fields by their placeholder text or parent name
            foreach (var field in inputFields)
            {
                string fieldName = "";
                if (field.placeholder != null && field.placeholder is TextMeshProUGUI placeholder)
                {
                    fieldName = placeholder.text.ToLower();
                }
                else
                {
                    fieldName = field.transform.parent.name.ToLower();
                }
                
                if (fieldName.Contains("username")) _username = field;
                else if (fieldName.Contains("email")) _email = field;
                else if (fieldName.Contains("password") && !fieldName.Contains("confirm")) _password = field;
                else if (fieldName.Contains("confirm")) _confirmPassword = field;
                else if (fieldName.Contains("character")) _characterName = field;
            }
            
            // Map buttons by their text content
            foreach (var button in buttons)
            {
                var text = button.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string buttonText = text.text.ToLower();
                    if (buttonText.Contains("create"))
                    {
                        _createBtn = button;
                        _createBtn.onClick.AddListener(OnCreateClicked);
                        Debug.Log("✅ Auto-wired Create Account button");
                    }
                    else if (buttonText.Contains("back"))
                    {
                        _backBtn = button;
                        _backBtn.onClick.AddListener(OnBackClicked);
                        Debug.Log("✅ Auto-wired Back to Login button");
                    }
                }
            }
            
            Debug.Log($"🔧 Auto-wire complete - CreateBtn: {_createBtn != null}, BackBtn: {_backBtn != null}");
            Debug.Log($"🔧 Input fields - Username: {_username != null}, Email: {_email != null}, Password: {_password != null}, Confirm: {_confirmPassword != null}, CharName: {_characterName != null}");
        }

        void Update()
        {
            // Test hotkey to verify SignupController is active
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("🔧 F9 pressed - SignupController is active!");
                TestButtonClicks();
            }
        }

        private void TestButtonClicks()
        {
            Debug.Log($"🔧 Testing button references - CreateBtn: {_createBtn != null}, BackBtn: {_backBtn != null}");
            if (_createBtn != null)
            {
                Debug.Log($"🔧 CreateBtn interactable: {_createBtn.interactable}, listener count: {_createBtn.onClick.GetPersistentEventCount()}");
            }
            if (_backBtn != null)
            {
                Debug.Log($"🔧 BackBtn interactable: {_backBtn.interactable}, listener count: {_backBtn.onClick.GetPersistentEventCount()}");
            }
        }

        public void Bind(TMP_InputField username, TMP_InputField email, TMP_InputField password, TMP_InputField confirmPassword, TMP_InputField characterName,
                         Button createBtn, Button backBtn)
        {
            Debug.Log("🔗 SignupController.Bind() method called!");
            
            _username = username; _email = email; _password = password; _confirmPassword = confirmPassword; _characterName = characterName;
            _createBtn = createBtn; _backBtn = backBtn;
            
            Debug.Log($"🔗 SignupController.Bind() parameters - CreateBtn: {_createBtn != null}, BackBtn: {_backBtn != null}");
            Debug.Log($"🔗 Username field: {_username != null}, Email field: {_email != null}");
            
            if (_createBtn) 
            {
                _createBtn.onClick.AddListener(OnCreateClicked);
                Debug.Log("✅ Create button click listener added");
            }
            else
            {
                Debug.LogError("❌ Create button is null in Bind()!");
            }
            
            if (_backBtn) 
            {
                _backBtn.onClick.AddListener(OnBackClicked);
                Debug.Log("✅ Back button click listener added");
            }
            else
            {
                Debug.LogError("❌ Back button is null in Bind()!");
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("🔙 Back to Login clicked");
            
            // If we're in a separate scene, load login scene
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneNames.LoginScene)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.LoginScene);
                return;
            }
            
            // Try multiple approaches to find and reactivate the login canvas
            GameObject loginCanvas = null;
            
            // Method 1: Find by tag (handle missing tag gracefully)
            try
            {
                loginCanvas = GameObject.FindGameObjectWithTag("LoginCanvas");
            }
            catch (UnityException)
            {
                Debug.Log("🏷️ LoginCanvas tag not defined, trying other methods...");
                loginCanvas = null;
            }
            
            // Method 2: Find by name variations
            if (loginCanvas == null)
            {
                loginCanvas = GameObject.Find("LoginCanvas") ?? GameObject.Find("Login Canvas") ?? GameObject.Find("LoginPanel");
            }
            
            // Method 3: Find any canvas with LoginController component
            if (loginCanvas == null)
            {
                var loginController = Object.FindFirstObjectByType<LoginController>();
                if (loginController != null)
                {
                    loginCanvas = loginController.transform.root.gameObject;
                }
            }
            
            // Method 4: Find any deactivated canvas in the scene
            if (loginCanvas == null)
            {
                var allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (var canvas in allCanvases)
                {
                    if (canvas.name.ToLower().Contains("login") && !canvas.gameObject.activeInHierarchy)
                    {
                        loginCanvas = canvas.gameObject;
                        break;
                    }
                }
            }
            
            if (loginCanvas != null)
            {
                loginCanvas.SetActive(true);
                Debug.Log($"✅ Login canvas '{loginCanvas.name}' reactivated");
                
                // Destroy the signup panel
                if (gameObject != null)
                {
                    Debug.Log("🗑️ Destroying signup canvas");
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning("❌ Could not find login canvas to reactivate.");
                
                // Try a more aggressive search for any login-related UI
                var allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
                Debug.Log($"🔍 Found {allCanvases.Length} total canvases in scene");
                
                foreach (var canvas in allCanvases)
                {
                    Debug.Log($"   📋 Canvas: {canvas.name} (active: {canvas.gameObject.activeInHierarchy})");
                }
                
                // Just destroy the signup canvas and let the user manually navigate
                if (gameObject != null)
                {
                    Debug.Log("🗑️ Destroying signup canvas (no login canvas found)");
                    Destroy(gameObject);
                }
                
                Debug.Log("ℹ️ Signup screen closed. Please manually return to login if needed.");
            }
        }

        private void OnCreateClicked()
        {
            Debug.Log("🔘 Create Account button clicked!");
            
            var username = _username?.text?.Trim() ?? string.Empty;
            var email = _email?.text?.Trim() ?? string.Empty;
            var password = _password?.text ?? string.Empty;
            var confirm = _confirmPassword?.text ?? string.Empty;
            var charName = _characterName?.text?.Trim() ?? string.Empty;

            Debug.Log($"📝 Form data - Username: '{username}', Email: '{email}', Password Length: {password.Length}, Character: '{charName}'");

            if (string.IsNullOrEmpty(username)) { ShowError("Username is required."); return; }
            if (string.IsNullOrEmpty(email)) { ShowError("Email is required."); return; }
            if (string.IsNullOrEmpty(password)) { ShowError("Password is required."); return; }
            if (password != confirm) { ShowError("Passwords do not match."); return; }
            if (charName.Length < 3) { ShowError("Character name must be at least 3 letters."); return; }
            
            // Additional validation
            if (username.Length < 3) { ShowError("Username must be at least 3 characters."); return; }
            if (password.Length < 6) { ShowError("Password must be at least 6 characters."); return; }
            if (!email.Contains("@")) { ShowError("Please enter a valid email address."); return; }

            Debug.Log($"✅ Validation passed! Creating account for {username} with character '{charName}'");
            
            // Disable button to prevent double-clicks
            if (_createBtn) 
            {
                _createBtn.interactable = false;
                Debug.Log("🔒 Create button disabled");
            }
            
            CreateAccountAsync(username, email, password, charName);
        }
        
        private async void CreateAccountAsync(string username, string email, string password, string characterName)
        {
            try
            {
                Debug.Log($"🆕 Creating account: {username}");
                
                // Create account request
                var createRequest = new MMOCreateAccountRequest
                {
                    username = username,
                    email = email,
                    password = password
                };
                
                // Attempt to create account through AccountManager
                bool accountCreated = await AccountManager.Instance.CreateAccount(createRequest);
                
                if (accountCreated)
                {
                    Debug.Log("✅ Account created successfully!");
                    ShowSuccess($"Account '{username}' created successfully! Returning to login...");
                    
                    // Wait a moment then return to login
                    await Task.Delay(1500);
                    ReturnToLogin();
                }
                else
                {
                    Debug.LogWarning("❌ Account creation failed");
                    ShowError("Failed to create account. Username or email may already be taken.");
                    ResetCreateButton();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Account creation error: {e.Message}");
                ShowError("Account creation failed due to connection error.");
                ResetCreateButton();
            }
        }
        
        private void ResetCreateButton()
        {
            if (_createBtn) _createBtn.interactable = true;
        }
        
        private void ShowSuccess(string message)
        {
            Debug.Log($"[Signup] SUCCESS: {message}");
            // TODO: Show success message in UI (could add success text component)
        }

        private void ShowError(string message)
        {
            Debug.LogWarning($"[Signup] {message}");
        }
        
        private void ReturnToLogin()
        {
            Debug.Log("🔄 Returning to login screen...");
            
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"📍 Current scene: {currentScene}");
            
            // If we're already in the LoginScene, just use the back button logic (UI navigation)
            if (currentScene == SceneNames.LoginScene || currentScene == "LoginScene")
            {
                Debug.Log("🎯 Already in LoginScene, using UI navigation...");
                OnBackClicked();
            }
            else
            {
                // We're in a different scene, load the login scene
                try
                {
                    Debug.Log($"🔄 Loading LoginScene from {currentScene}...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.LoginScene);
                    Debug.Log("✅ Login scene loaded successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Failed to load login scene: {e.Message}");
                    // Still try the UI navigation as fallback
                    OnBackClicked();
                }
            }
        }
    }
}


