using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Simple test script to verify MySQL database connection and operations
/// Attach this to a GameObject in the scene to test database functionality
/// </summary>
public class DatabaseTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool debugOutput = true;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            TestDatabaseConnection();
        }
    }
    
    /// <summary>
    /// Test basic database connection
    /// </summary>
    public async void TestDatabaseConnection()
    {
        if (debugOutput) Debug.Log("🧪 Starting database connection test...");
        
        try
        {
            // Test connection
            bool connected = await UnityWebDatabase.Instance.TestConnection();
            
            if (connected)
            {
                if (debugOutput) Debug.Log("✅ Database connection test passed!");
                
                // Run additional tests if connection works
                await RunBasicTests();
            }
            else
            {
                if (debugOutput) Debug.LogError("❌ Database connection test failed!");
            }
        }
        catch (System.Exception e)
        {
            if (debugOutput) Debug.LogError($"❌ Database test error: {e.Message}");
        }
    }
    
    /// <summary>
    /// Run basic CRUD tests on accounts and characters
    /// </summary>
    private async Task RunBasicTests()
    {
        if (debugOutput) Debug.Log("🧪 Running basic database tests...");
        
        try
        {
            // Test 1: Check if test username exists
            bool usernameExists = await UnityWebDatabase.Instance.CheckUsernameExists("testuser123");
            if (debugOutput) Debug.Log($"📝 Username 'testuser123' exists: {usernameExists}");
            
            // Test 2: Check if test email exists
            bool emailExists = await UnityWebDatabase.Instance.CheckEmailExists("test@example.com");
            if (debugOutput) Debug.Log($"📧 Email 'test@example.com' exists: {emailExists}");
            
            // Test 3: Check if character name exists
            bool charExists = await UnityWebDatabase.Instance.CheckCharacterNameExists("TestCharacter");
            if (debugOutput) Debug.Log($"🎭 Character name 'TestCharacter' exists: {charExists}");
            
            if (debugOutput) Debug.Log("✅ Basic database tests completed successfully!");
        }
        catch (System.Exception e)
        {
            if (debugOutput) Debug.LogError($"❌ Basic database tests failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Manual test button for Unity Inspector
    /// </summary>
    [ContextMenu("Test Database Connection")]
    public void ManualTestConnection()
    {
        TestDatabaseConnection();
    }
    
    /// <summary>
    /// Test account creation flow
    /// </summary>
    [ContextMenu("Test Account Creation")]
    public async void TestAccountCreation()
    {
        if (debugOutput) Debug.Log("🧪 Testing account creation...");
        
        try
        {
            var accountManager = AccountManager.Instance;
            
            var createRequest = new MMOCreateAccountRequest
            {
                username = "testuser" + System.DateTime.Now.Ticks.ToString().Substring(10),
                email = $"test{System.DateTime.Now.Ticks.ToString().Substring(10)}@example.com",
                password = "testpassword123"
            };
            
            bool success = await accountManager.CreateAccount(createRequest);
            
            if (success)
            {
                if (debugOutput) Debug.Log($"✅ Account creation test passed for {createRequest.username}!");
            }
            else
            {
                if (debugOutput) Debug.LogError("❌ Account creation test failed!");
            }
        }
        catch (System.Exception e)
        {
            if (debugOutput) Debug.LogError($"❌ Account creation test error: {e.Message}");
        }
    }
    
    /// <summary>
    /// Test character creation flow
    /// </summary>
    [ContextMenu("Test Character Creation")]
    public async void TestCharacterCreation()
    {
        if (debugOutput) Debug.Log("🧪 Testing character creation...");
        
        try
        {
            var characterManager = MMOCharacterManager.Instance;
            var accountManager = AccountManager.Instance;
            
            if (!accountManager.IsLoggedIn)
            {
                if (debugOutput) Debug.LogWarning("⚠️ Must be logged in to test character creation");
                return;
            }
            
            var createRequest = new MMOCreateCharacterRequest
            {
                characterName = "TestChar" + System.DateTime.Now.Ticks.ToString().Substring(12),
                characterClass = "Vanguard Knight",
                isMale = true,
                skinColor = "#FFDBAC",
                hairIndex = 0,
                faceIndex = 0,
                hairColor = "#8B4513",
                eyesColor = "#4169E1"
            };
            
            bool success = await characterManager.CreateCharacter(createRequest);
            
            if (success)
            {
                if (debugOutput) Debug.Log($"✅ Character creation test passed for {createRequest.characterName}!");
            }
            else
            {
                if (debugOutput) Debug.LogError("❌ Character creation test failed!");
            }
        }
        catch (System.Exception e)
        {
            if (debugOutput) Debug.LogError($"❌ Character creation test error: {e.Message}");
        }
    }
}