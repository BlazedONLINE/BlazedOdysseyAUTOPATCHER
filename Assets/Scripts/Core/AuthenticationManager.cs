using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Security.Cryptography;

// Certificate handler to allow insecure connections for development
public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true; // Accept all certificates for development
    }
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string passwordHash;
}

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string username;
    public string passwordHash;
}

[System.Serializable]
public class AuthResponse
{
    public bool success;
    public string message;
    public string token;
    public UserData userData;
}

[System.Serializable]
public class UserData
{
    public int userId;
    public string username;
    public string email;
    public string createdAt;
}

public class AuthenticationManager : MonoBehaviour
{
    [Header("Server Configuration")]
    public string serverBaseURL = "http://129.212.181.87:3000/api"; // Your live DigitalOcean server
    public float requestTimeout = 5f; // Shorter timeout for development
    public bool developmentMode = true; // Skip server and use local auth for testing
    
    [Header("Security")]
    public string clientSalt = "BlazedOdyssey2024!"; // Change this in production
    
    private string currentAuthToken = "";
    public UserData CurrentUser { get; private set; }
    
    void Awake()
    {
        // Persist authentication manager across scenes
        DontDestroyOnLoad(gameObject);
        Debug.Log("🔐 Authentication Manager initialized!");
        
        // Load saved auth token if available
        LoadSavedAuthToken();
    }
    
    public IEnumerator LoginUser(string email, string password)
    {
        Debug.Log($"🔐 Attempting login for: {email}");
        
        // In development mode, skip server and use local authentication
        if (developmentMode)
        {
            Debug.Log("🛠️ Development mode enabled - using local authentication");
            
            if (IsTestCredentials(email, password))
            {
                Debug.Log("🧪 Valid test credentials - simulating successful login");
                SimulateSuccessfulLogin(email);
                yield break;
            }
            else
            {
                Debug.LogWarning("❌ Invalid test credentials in development mode");
                ClearAuthData();
                yield break;
            }
        }
        
        // Production mode - try server authentication
        Debug.Log("🌐 Production mode - attempting server connection...");
        
        // Hash password before sending (never send plain text passwords!)
        string passwordHash = HashPassword(password, email);
        
        LoginRequest loginData = new LoginRequest
        {
            email = email,
            passwordHash = passwordHash
        };
        
        string jsonData = JsonUtility.ToJson(loginData);
        
        using (UnityWebRequest request = new UnityWebRequest($"{serverBaseURL}/auth/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)requestTimeout;
            
            // Allow insecure connections for development
            request.certificateHandler = new AcceptAllCertificates();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                    
                    if (response.success)
                    {
                        currentAuthToken = response.token;
                        CurrentUser = response.userData;
                        
                        // Save auth token for auto-login
                        SaveAuthToken(currentAuthToken);
                        
                        Debug.Log($"✅ Login successful for: {CurrentUser.username}");
                        yield break;
                    }
                    else
                    {
                        Debug.LogWarning($"❌ Login failed: {response.message}");
                        ClearAuthData();
                        yield break;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Failed to parse login response: {e.Message}");
                    ClearAuthData();
                    yield break;
                }
            }
            else
            {
                // For development/testing - simulate successful login if server isn't available
                if (IsTestCredentials(email, password))
                {
                    Debug.Log("🧪 Using test credentials - simulating successful login");
                    SimulateSuccessfulLogin(email);
                    yield break;
                }
                else
                {
                    Debug.LogError($"❌ Login request failed: {request.error}");
                    ClearAuthData();
                    yield break;
                }
            }
        }
    }
    
    public IEnumerator RegisterUser(string email, string username, string password)
    {
        Debug.Log($"📝 Attempting registration for: {email}");
        
        // In development mode, automatically succeed
        if (developmentMode)
        {
            Debug.Log("🛠️ Development mode enabled - simulating successful registration");
            Debug.Log($"✅ Registration successful for: {username}");
            yield break;
        }
        
        // Production mode - try server registration
        Debug.Log("🌐 Production mode - attempting server registration...");
        
        // Hash password before sending
        string passwordHash = HashPassword(password, email);
        
        RegisterRequest registerData = new RegisterRequest
        {
            email = email,
            username = username,
            passwordHash = passwordHash
        };
        
        string jsonData = JsonUtility.ToJson(registerData);
        
        using (UnityWebRequest request = new UnityWebRequest($"{serverBaseURL}/auth/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)requestTimeout;
            
            // Allow insecure connections for development
            request.certificateHandler = new AcceptAllCertificates();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                    
                    if (response.success)
                    {
                        Debug.Log($"✅ Registration successful for: {username}");
                        yield break;
                    }
                    else
                    {
                        Debug.LogWarning($"❌ Registration failed: {response.message}");
                        yield break;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Failed to parse registration response: {e.Message}");
                    yield break;
                }
            }
            else
            {
                Debug.LogError($"❌ Registration request failed: {request.error}");
                yield break;
            }
        }
    }
    
    /// <summary>
    /// Securely hash password with email as additional salt
    /// This ensures even if two users have the same password, their hashes will be different
    /// </summary>
    private string HashPassword(string password, string email)
    {
        // Combine password with email and client salt for additional security
        string combined = password + email.ToLower() + clientSalt;
        
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combined));
            
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
    
    private bool IsTestCredentials(string email, string password)
    {
        // Test credentials for development
        return (email == "test@blazed.com" && password == "testpass123") ||
               (email == "dev@blazed.com" && password == "devpass123");
    }
    
    private void SimulateSuccessfulLogin(string email)
    {
        CurrentUser = new UserData
        {
            userId = 999,
            username = email.Contains("dev") ? "DevTester" : "TestPlayer",
            email = email,
            createdAt = System.DateTime.Now.ToString()
        };
        
        currentAuthToken = "test_token_" + System.DateTime.Now.Ticks;
    }
    
    public void Logout()
    {
        ClearAuthData();
        Debug.Log("🚪 User logged out successfully");
    }
    
    private void ClearAuthData()
    {
        currentAuthToken = "";
        CurrentUser = null;
        
        // Clear saved auth token
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.Save();
    }
    
    private void SaveAuthToken(string token)
    {
        PlayerPrefs.SetString("AuthToken", token);
        PlayerPrefs.Save();
    }
    
    private void LoadSavedAuthToken()
    {
        string savedToken = PlayerPrefs.GetString("AuthToken", "");
        if (!string.IsNullOrEmpty(savedToken))
        {
            currentAuthToken = savedToken;
            // In a full implementation, you'd validate this token with the server
            Debug.Log("🔄 Loaded saved authentication token");
        }
    }
    
    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(currentAuthToken) && CurrentUser != null;
    }
    
    public string GetAuthToken()
    {
        return currentAuthToken;
    }
}
