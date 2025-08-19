using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Account data structure
/// </summary>
[System.Serializable]
public class MMOAccountData
{
    public int id;
    public string username;
    public string email;
    public int characterSlots;
    public bool isActive;
    public bool isAdmin;
    public DateTime lastLogin;
    public string lastIP;
    public DateTime createdAt;
}

/// <summary>
/// Account creation request
/// </summary>
[System.Serializable]
public class MMOCreateAccountRequest
{
    public string username;
    public string email;
    public string password;
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(username) && 
               !string.IsNullOrEmpty(email) && 
               !string.IsNullOrEmpty(password) &&
               username.Length >= 3 && username.Length <= 50 &&
               email.Contains("@") && email.Length <= 255 &&
               password.Length >= 6;
    }
}

/// <summary>
/// Login request
/// </summary>
[System.Serializable]
public class MMOLoginRequest
{
    public string username;
    public string password;
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }
}

/// <summary>
/// Account management with secure password hashing
/// </summary>
public class AccountManager : MonoBehaviour
{
    [Header("Security Settings")]
    [SerializeField] private int saltLength = 32;
    [SerializeField] private int hashIterations = 10000;
    [SerializeField] private int sessionTimeout = 3600; // 1 hour
    
    private static AccountManager _instance;
    public static AccountManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AccountManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AccountManager");
                    _instance = go.AddComponent<AccountManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // Current logged in account
    public MMOAccountData CurrentAccount { get; private set; }
    public bool IsLoggedIn => CurrentAccount != null;
    
    // Events
    public event System.Action<MMOAccountData> OnAccountLogin;
    public event System.Action OnAccountLogout;
    public event System.Action<string> OnAccountError;
    
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
    
    /// <summary>
    /// Create a new account
    /// </summary>
    public async Task<bool> CreateAccount(MMOCreateAccountRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                OnAccountError?.Invoke("Invalid account data. Check username, email, and password requirements.");
                return false;
            }
            
            Debug.Log($"🆕 Creating account for username: {request.username}");
            
            // Generate salt and hash password
            string salt = GenerateSalt();
            string passwordHash = HashPassword(request.password, salt);
            
            // Check if username/email already exists in database (handle server errors gracefully)
            try
            {
                if (await CheckUsernameExists(request.username))
                {
                    OnAccountError?.Invoke("Username already exists.");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Username check failed (server error), proceeding with account creation: {e.Message}");
            }
            
            try
            {
                if (await CheckEmailExists(request.email))
                {
                    OnAccountError?.Invoke("Email already registered.");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Email check failed (server error), proceeding with account creation: {e.Message}");
            }
            
            // TODO: Insert into database
            bool success = await InsertAccountToDatabase(request.username, request.email, passwordHash, salt);
            
            if (success)
            {
                Debug.Log($"✅ Account created successfully for {request.username}");
                return true;
            }
            else
            {
                OnAccountError?.Invoke("Failed to create account. Please try again.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error creating account: {e.Message}");
            OnAccountError?.Invoke("Account creation failed due to server error.");
            return false;
        }
    }
    
    /// <summary>
    /// Login with username and password
    /// </summary>
    public async Task<bool> Login(MMOLoginRequest request)
    {
        try
        {
            if (!request.IsValid())
            {
                OnAccountError?.Invoke("Please enter both username and password.");
                return false;
            }
            
            Debug.Log($"🔐 Login attempt for username: {request.username}");
            
            // TODO: Get account data from database
            var accountData = await GetAccountFromDatabase(request.username);
            
            if (accountData == null)
            {
                OnAccountError?.Invoke("Invalid username or password.");
                return false;
            }
            
            // TODO: Get stored password hash and salt
            var (storedHash, salt) = await GetPasswordData(accountData.id);
            
            // Verify password
            string inputHash = HashPassword(request.password, salt);
            if (inputHash != storedHash)
            {
                OnAccountError?.Invoke("Invalid username or password.");
                return false;
            }
            
            // Check if account is active
            if (!accountData.isActive)
            {
                OnAccountError?.Invoke("Account is disabled. Please contact support.");
                return false;
            }
            
            // Update last login
            await UpdateLastLogin(accountData.id);
            
            // Set current account
            CurrentAccount = accountData;
            OnAccountLogin?.Invoke(CurrentAccount);
            
            Debug.Log($"✅ Login successful for {request.username}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error during login: {e.Message}");
            OnAccountError?.Invoke("Login failed due to server error.");
            return false;
        }
    }
    
    /// <summary>
    /// Logout current account
    /// </summary>
    public void Logout()
    {
        if (CurrentAccount != null)
        {
            Debug.Log($"🚪 Logging out user: {CurrentAccount.username}");
            CurrentAccount = null;
            OnAccountLogout?.Invoke();
        }
    }
    
    /// <summary>
    /// Generate a cryptographically secure salt
    /// </summary>
    private string GenerateSalt()
    {
        byte[] saltBytes = new byte[saltLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }
    
    /// <summary>
    /// Hash password with salt using PBKDF2
    /// </summary>
    private string HashPassword(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, hashIterations))
        {
            byte[] hashBytes = pbkdf2.GetBytes(32); // 256-bit hash
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    // Unity-compatible web database methods using UnityWebDatabase
    private async Task<bool> CheckUsernameExists(string username)
    {
        return await UnityWebDatabase.Instance.CheckUsernameExists(username);
    }
    
    private async Task<bool> CheckEmailExists(string email)
    {
        return await UnityWebDatabase.Instance.CheckEmailExists(email);
    }
    
    private async Task<bool> InsertAccountToDatabase(string username, string email, string passwordHash, string salt)
    {
        return await UnityWebDatabase.Instance.CreateAccount(username, email, passwordHash, salt);
    }
    
    private async Task<MMOAccountData> GetAccountFromDatabase(string username)
    {
        return await UnityWebDatabase.Instance.GetAccount(username);
    }
    
    private async Task<(string hash, string salt)> GetPasswordData(int accountId)
    {
        return await UnityWebDatabase.Instance.GetPasswordData(accountId);
    }
    
    private async Task UpdateLastLogin(int accountId)
    {
        await UnityWebDatabase.Instance.UpdateLastLogin(accountId);
    }
    
    void Start()
    {
        Debug.Log("🔐 AccountManager initialized with secure password hashing");
    }
}