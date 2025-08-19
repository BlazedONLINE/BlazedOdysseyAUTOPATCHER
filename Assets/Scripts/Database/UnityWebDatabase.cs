using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

/// <summary>
/// Unity-compatible database operations using UnityWebRequest
/// This uses HTTP API calls instead of direct MySQL connection to avoid dependency issues
/// </summary>
public class UnityWebDatabase : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiBaseUrl = "https://api.blazedodyssey.com/api";
    [SerializeField] private bool enableDatabase = true;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private float requestTimeout = 30f;
    
    private static UnityWebDatabase _instance;
    
    public static UnityWebDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnityWebDatabase>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("UnityWebDatabase");
                    _instance = go.AddComponent<UnityWebDatabase>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Force production MySQL database URL (override any Inspector values)
        if (!apiBaseUrl.Contains("api.blazedodyssey.com"))
        {
            apiBaseUrl = "https://api.blazedodyssey.com/api";
            Debug.Log($"🔧 API URL set to production MySQL database: {apiBaseUrl}");
        }
        
        if (enableDatabase)
        {
            TestConnectionAsync();
        }
        else
        {
            Debug.Log("🔒 Web database is disabled");
        }
    }
    
    /// <summary>
    /// Test API connection
    /// </summary>
    public async void TestConnectionAsync()
    {
        bool connected = await TestConnection();
        if (connected)
        {
            Debug.Log("✅ Web API connection successful!");
        }
        else
        {
            Debug.LogError("❌ Web API connection failed!");
        }
    }
    
    /// <summary>
    /// Test database connection via API
    /// </summary>
    public async Task<bool> TestConnection()
    {
        if (!enableDatabase) return false;
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/health"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (debugMode) Debug.Log($"✅ API Health Check: {request.downloadHandler.text}");
                    return true;
                }
                else
                {
                    if (debugMode) Debug.LogError($"❌ API Health Check Failed: {request.error}");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ API connection test failed: {e.Message}");
            return false;
        }
    }
    
    #region Account Operations
    
    /// <summary>
    /// Check if username exists
    /// </summary>
    public async Task<bool> CheckUsernameExists(string username)
    {
        if (!enableDatabase) return false;
        
        try
        {
            Debug.Log($"🔍 Checking if username '{username}' exists at {apiBaseUrl}/accounts/check-username/{username}");
            using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/accounts/check-username/{username}"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BoolResponse>(request.downloadHandler.text);
                    Debug.Log($"✅ Username check result: {response.exists} (Response: {request.downloadHandler.text})");
                    return response.exists;
                }
                else
                {
                    Debug.LogWarning($"⚠️ CheckUsernameExists failed (server error): {request.error} - assuming username is available");
                    return false;  // Assume username doesn't exist if server check fails
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CheckUsernameExists failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Check if email exists
    /// </summary>
    public async Task<bool> CheckEmailExists(string email)
    {
        if (!enableDatabase) return false;
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/accounts/check-email/{email}"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BoolResponse>(request.downloadHandler.text);
                    return response.exists;
                }
                else
                {
                    Debug.LogWarning($"⚠️ CheckEmailExists failed (server error): {request.error} - assuming email is available");
                    return false;  // Assume email doesn't exist if server check fails
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CheckEmailExists failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Create new account
    /// </summary>
    public async Task<bool> CreateAccount(string username, string email, string passwordHash, string salt)
    {
        if (!enableDatabase) return false;
        
        try
        {
            Debug.Log($"🆕 Creating account: {username}, {email}");
            var accountData = new CreateAccountRequest
            {
                username = username,
                email = email,
                passwordHash = passwordHash,
                salt = salt
            };
            
            string jsonData = JsonUtility.ToJson(accountData);
            Debug.Log($"📤 Sending POST to {apiBaseUrl}/accounts with data: {jsonData}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/accounts", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)requestTimeout;
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ Account created successfully: {request.downloadHandler.text}");
                    return true;
                }
                else
                {
                    Debug.LogError($"❌ CreateAccount failed: {request.error} | Response: {request.downloadHandler.text}");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CreateAccount failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get account by username
    /// </summary>
    public async Task<MMOAccountData> GetAccount(string username)
    {
        if (!enableDatabase) return null;
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/accounts/{username}"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<MMOAccountData>(request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"❌ GetAccount failed: {request.error}");
                    return null;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ GetAccount failed: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get password hash and salt for account
    /// </summary>
    public async Task<(string hash, string salt)> GetPasswordData(int accountId)
    {
        if (!enableDatabase) return ("", "");
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/accounts/{accountId}/password"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<PasswordDataResponse>(request.downloadHandler.text);
                    return (response.hash, response.salt);
                }
                else
                {
                    Debug.LogError($"❌ GetPasswordData failed: {request.error}");
                    return ("", "");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ GetPasswordData failed: {e.Message}");
            return ("", "");
        }
    }
    
    /// <summary>
    /// Update last login time and IP
    /// </summary>
    public async Task<bool> UpdateLastLogin(int accountId, string ipAddress = "")
    {
        if (!enableDatabase) return false;
        
        try
        {
            var loginData = new UpdateLoginRequest
            {
                accountId = accountId,
                ipAddress = ipAddress
            };
            
            string jsonData = JsonUtility.ToJson(loginData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/accounts/login", "PUT"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)requestTimeout;
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                return request.result == UnityWebRequest.Result.Success;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ UpdateLastLogin failed: {e.Message}");
            return false;
        }
    }
    
    #endregion
    
    #region Character Operations
    
    /// <summary>
    /// Check if character name exists
    /// </summary>
    public async Task<bool> CheckCharacterNameExists(string characterName)
    {
        if (!enableDatabase) return false;
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/characters/check-name/{characterName}"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BoolResponse>(request.downloadHandler.text);
                    return response.exists;
                }
                else
                {
                    Debug.LogError($"❌ CheckCharacterNameExists failed: {request.error}");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CheckCharacterNameExists failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get characters for account
    /// </summary>
    public async Task<List<MMOCharacterData>> GetCharactersForAccount(int accountId)
    {
        var characters = new List<MMOCharacterData>();
        
        if (!enableDatabase) return characters;
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/characters/account/{accountId}"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<CharacterListResponse>(request.downloadHandler.text);
                    return response.characters;
                }
                else
                {
                    Debug.LogError($"❌ GetCharactersForAccount failed: {request.error}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ GetCharactersForAccount failed: {e.Message}");
        }
        
        return characters;
    }
    
    /// <summary>
    /// Create new character
    /// </summary>
    public async Task<int> CreateCharacter(MMOCharacterData character)
    {
        if (!enableDatabase) return -1;
        
        try
        {
            string jsonData = JsonUtility.ToJson(character);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/characters", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)requestTimeout;
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<CreateCharacterResponse>(request.downloadHandler.text);
                    return response.characterId;
                }
                else
                {
                    Debug.LogError($"❌ CreateCharacter failed: {request.error}");
                    return -1;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CreateCharacter failed: {e.Message}");
            return -1;
        }
    }
    
    /// <summary>
    /// Update existing character
    /// </summary>
    public async Task<bool> UpdateCharacter(MMOCharacterData character)
    {
        if (!enableDatabase) return false;
        
        try
        {
            string jsonData = JsonUtility.ToJson(character);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/characters/{character.id}", "PUT"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)requestTimeout;
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                return request.result == UnityWebRequest.Result.Success;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ UpdateCharacter failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Delete character
    /// </summary>
    public async Task<bool> DeleteCharacter(int characterId)
    {
        if (!enableDatabase) return false;
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Delete($"{apiBaseUrl}/characters/{characterId}"))
            {
                request.timeout = (int)requestTimeout;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                return request.result == UnityWebRequest.Result.Success;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ DeleteCharacter failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Login (server-side verification)
    /// </summary>
    public async Task<bool> Login(string username, string password)
    {
        if (!enableDatabase) return false;
        try
        {
            var obj = new LoginPayload { username = username, password = password };
            string jsonData = JsonUtility.ToJson(obj);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            // Support /api/login on same base URL
            using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl.Replace("/api","")}/auth/login", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)requestTimeout;

                var op = request.SendWebRequest();
                while (!op.isDone) { await Task.Yield(); }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var resp = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    return resp != null && resp.success;
                }
                else
                {
                    if (debugMode) Debug.LogError($"❌ Login failed: {request.error}");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Login failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Create account with plain password (server hashes securely)
    /// </summary>
    public async Task<bool> CreateAccountPlain(string username, string email, string password)
    {
        if (!enableDatabase) return false;
        try
        {
            var payload = new SignupPayload { username = username, email = email, password = password };
            string jsonData = JsonUtility.ToJson(payload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            // Support /api/signup on same base URL
            using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl.Replace("/api","")}/auth/signup", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)requestTimeout;

                var op = request.SendWebRequest();
                while (!op.isDone) { await Task.Yield(); }

                return request.result == UnityWebRequest.Result.Success;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ CreateAccountPlain failed: {e.Message}");
            return false;
        }
    }

    #endregion
}

#region API Response Classes

[System.Serializable]
public class BoolResponse
{
    public bool exists;
}

[System.Serializable]
public class CreateAccountRequest
{
    public string username;
    public string email;
    public string passwordHash;
    public string salt;
}

[System.Serializable]
public class PasswordDataResponse
{
    public string hash;
    public string salt;
}

[System.Serializable]
public class UpdateLoginRequest
{
    public int accountId;
    public string ipAddress;
}

[System.Serializable]
public class CharacterListResponse
{
    public List<MMOCharacterData> characters;
}

[System.Serializable]
public class CreateCharacterResponse
{
    public int characterId;
}

[System.Serializable]
public class LoginPayload { public string username; public string password; }

[System.Serializable]
public class SignupPayload { public string username; public string email; public string password; }

[System.Serializable]
public class LoginResponse { public bool success; public string error; }

#endregion