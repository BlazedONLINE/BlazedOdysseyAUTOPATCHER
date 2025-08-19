using UnityEngine;
using UnityEngine.SceneManagement;

public class MMOSceneSetup : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string loginSceneName = "MMOLoginScene";
    public string characterSelectionSceneName = "MMOCharacterSelectionScene";
    
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    public bool createTestObjects = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }
    
    void SetupScene()
    {
        Debug.Log("🎮 Setting up MMO scene...");
        
        // Check if we're in the right scene
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Current scene: {currentScene}");
        
        // Create test objects if enabled
        if (createTestObjects)
        {
            CreateTestObjects();
        }
        
        // Ensure proper lighting for 2D
        SetupLighting();
        
        Debug.Log("✅ Scene setup complete!");
    }
    
    void CreateTestObjects()
    {
        // Create a simple test object to verify the scene is working
        GameObject testObj = new GameObject("Test Object");
        testObj.transform.position = Vector3.zero;
        
        // Add a simple sprite renderer if we have sprites
        var spriteRenderer = testObj.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
        
        Debug.Log("🧪 Test object created");
    }
    
    void SetupLighting()
    {
        // Ensure we have proper lighting for 2D
        if (FindObjectOfType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.color = Color.white;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            Debug.Log("💡 Lighting setup complete");
        }
    }
    
    [ContextMenu("Setup Scene")]
    void SetupSceneFromMenu()
    {
        SetupScene();
    }
    
    [ContextMenu("Load Login Scene")]
    void LoadLoginScene()
    {
        SceneManager.LoadScene(loginSceneName);
    }
    
    [ContextMenu("Load Character Selection Scene")]
    void LoadCharacterSelectionScene()
    {
        SceneManager.LoadScene(characterSelectionSceneName);
    }
}
