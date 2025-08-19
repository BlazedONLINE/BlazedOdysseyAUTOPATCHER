using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BlazedOdyssey.UI.Common;
using System.Threading.Tasks;

namespace BlazedOdyssey.UI.Login
{
    public class LoginController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TMP_InputField usernameInput = default!;
        [SerializeField] private TMP_InputField passwordInput = default!;
        [SerializeField] private Toggle rememberMeToggle = default!;
        [SerializeField] private Button loginButton = default!;
        [SerializeField] private Button createAccountButton = default!;
        [SerializeField] private Button settingsButton = default!;
        [SerializeField] private Button exitButton = default!;
        [SerializeField] private GameObject signingInOverlay = default!;

        [Header("Optional Panels")]
        [SerializeField] private GameObject settingsPanelPrefab = default!;
        [SerializeField] private GameObject signupPanelPrefab = default!;

        private void Awake()
        {
            // Hydrate Remember Me
            if (rememberMeToggle)
            {
                rememberMeToggle.isOn = RememberMeStore.GetRememberMe();
                if (rememberMeToggle.isOn && usernameInput)
                    usernameInput.text = RememberMeStore.GetUser();
            }

            if (loginButton) loginButton.onClick.AddListener(OnLoginClicked);
            if (createAccountButton) createAccountButton.onClick.AddListener(OpenSignup);
            if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
            if (exitButton) exitButton.onClick.AddListener(ExitGame);
        }

        private void Update()
        {
            // Enter submits
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnLoginClicked();
            }
        }

        private void OnLoginClicked()
        {
            if (!loginButton || loginButton.interactable == false) return;
            
            string username = usernameInput?.text?.Trim() ?? "";
            string password = passwordInput?.text ?? "";
            
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Please enter your username.");
                return;
            }
            
            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password.");
                return;
            }
            
            loginButton.interactable = false;
            if (signingInOverlay) signingInOverlay.SetActive(true);
            
            PerformRealLogin(username, password);
        }

        private async void PerformRealLogin(string username, string password)
        {
            try
            {
                Debug.Log($"🔐 Attempting login for user: {username}");
                
                // Create login request
                var loginRequest = new MMOLoginRequest
                {
                    username = username,
                    password = password
                };
                
                // Attempt login through AccountManager
                bool loginSuccess = await AccountManager.Instance.Login(loginRequest);
                
                if (loginSuccess)
                {
                    Debug.Log("✅ Login successful!");
                    
                    // Handle Remember Me
                    if (rememberMeToggle && rememberMeToggle.isOn)
                    {
                        RememberMeStore.SetRememberMe(true);
                        RememberMeStore.SetUser(username);
                    }
                    else
                    {
                        RememberMeStore.SetRememberMe(false);
                    }
                    
                    // Hide overlay and proceed to character selection
                    if (signingInOverlay) signingInOverlay.SetActive(false);
                    SceneManager.LoadScene(SceneNames.CharacterSelectionScene);
                }
                else
                {
                    Debug.LogWarning("❌ Login failed");
                    ShowError("Invalid username or password.");
                    ResetLoginUI();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Login error: {e.Message}");
                ShowError("Login failed due to connection error.");
                ResetLoginUI();
            }
        }
        
        private void ResetLoginUI()
        {
            if (loginButton) loginButton.interactable = true;
            if (signingInOverlay) signingInOverlay.SetActive(false);
        }
        
        private void ShowError(string message)
        {
            Debug.LogWarning($"[Login] {message}");
            // TODO: Show error message in UI (could add error text component)
        }

        private GameObject settingsPanelInstance;

        private void OpenSettings()
        {
            if (settingsPanelInstance != null)
            {
                var spx = settingsPanelInstance.GetComponent<BlazedOdyssey.UI.SettingsPanel>();
                if (spx != null) spx.Show();
                return;
            }

            // Prefer our exact SettingsPanel prefab from UI/Prefabs during editor sessions
#if UNITY_EDITOR
            var preferredPath = "Assets/BlazedOdyssey/Prefabs/UI/SettingsPanel.prefab";
            var preferred = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(preferredPath);
            if (preferred != null) settingsPanelPrefab = preferred;
#endif
            if (!settingsPanelPrefab)
            {
                Debug.LogWarning("[Login] Settings panel prefab not assigned");
                return;
            }
            settingsPanelInstance = Instantiate(settingsPanelPrefab, transform.root);
            // Move the child Window rect down to align with login panel area
            var window = settingsPanelInstance.transform.Find("Window") as RectTransform;
            if (window != null) window.anchoredPosition = new Vector2(0f, -60f);
            var adj = settingsPanelInstance.GetComponent<Common.HudAdjustable>();
            if (!adj) adj = settingsPanelInstance.AddComponent<Common.HudAdjustable>();
            adj.WidgetId = "SettingsPanel";
            var sp = settingsPanelInstance.GetComponent<BlazedOdyssey.UI.SettingsPanel>();
            if (sp != null) sp.Show();
            else Debug.LogWarning("[Login] Spawned settings prefab does not contain BlazedOdyssey.UI.SettingsPanel. Ensure you're using Assets/BlazedOdyssey/Prefabs/UI/SettingsPanel.prefab");
        }

        private void OpenSignup()
        {
            if (!signupPanelPrefab)
            {
#if UNITY_EDITOR
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BlazedOdyssey/Prefabs/UI/SignupCanvas.prefab");
                if (prefab != null) signupPanelPrefab = prefab;
#endif
            }
            if (!signupPanelPrefab)
            {
                Debug.LogWarning("[Login] Signup prefab not assigned or found");
                return;
            }
            var signup = Instantiate(signupPanelPrefab);
            // Hide current login canvas while signup is active
            transform.root.gameObject.SetActive(false);
        }

        private void ExitGame()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
    }
}


