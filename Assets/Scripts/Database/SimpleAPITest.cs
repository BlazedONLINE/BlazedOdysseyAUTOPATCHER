using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Simple script to test our SQLite API server connection
/// Attach this to any GameObject and it will test the API endpoints
/// </summary>
public class SimpleAPITest : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiBaseUrl = "https://api.blazedodyssey.com/api";
    [SerializeField] private bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            StartCoroutine(TestAllEndpoints());
        }
    }
    
    [ContextMenu("Test API Connection")]
    public void TestAPIConnection()
    {
        StartCoroutine(TestAllEndpoints());
    }
    
    private IEnumerator TestAllEndpoints()
    {
        Debug.Log("🧪 Starting API tests...");
        
        // Test 1: Health check
        yield return StartCoroutine(TestHealthCheck());
        
        // Test 2: Check username
        yield return StartCoroutine(TestUsernameCheck("testuser123"));
        
        // Test 3: Check character name
        yield return StartCoroutine(TestCharacterNameCheck("TestCharacter"));
        
        Debug.Log("🎉 API tests completed!");
    }
    
    private IEnumerator TestHealthCheck()
    {
        Debug.Log("🔍 Testing health check...");
        
        using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/health"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Health check passed: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ Health check failed: {request.error}");
            }
        }
    }
    
    private IEnumerator TestUsernameCheck(string username)
    {
        Debug.Log($"🔍 Testing username check for '{username}'...");
        
        using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/accounts/check-username/{username}"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Username check result: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ Username check failed: {request.error}");
            }
        }
    }
    
    private IEnumerator TestCharacterNameCheck(string characterName)
    {
        Debug.Log($"🔍 Testing character name check for '{characterName}'...");
        
        using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/characters/check-name/{characterName}"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Character name check result: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ Character name check failed: {request.error}");
            }
        }
    }
    
    [ContextMenu("Test Account Creation")]
    public void TestAccountCreation()
    {
        StartCoroutine(CreateTestAccount());
    }
    
    private IEnumerator CreateTestAccount()
    {
        Debug.Log("🧪 Testing account creation...");
        
        var accountData = new
        {
            username = "testuser" + System.DateTime.Now.Ticks.ToString().Substring(10),
            email = $"test{System.DateTime.Now.Ticks.ToString().Substring(10)}@example.com",
            passwordHash = "hashedpassword123",
            salt = "randomsalt123"
        };
        
        string jsonData = JsonUtility.ToJson(accountData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/accounts", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Account creation successful: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ Account creation failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }
}