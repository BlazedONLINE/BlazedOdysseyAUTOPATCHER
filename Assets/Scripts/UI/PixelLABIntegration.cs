using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using Newtonsoft.Json;

[System.Serializable]
public class PixelLABRequest
{
    public string prompt;
    public string style;
    public int width;
    public int height;
    public string format;
}

[System.Serializable]
public class PixelLABResponse
{
    public string status;
    public string image_url;
    public string error;
}

public class PixelLABIntegration : MonoBehaviour
{
    [Header("PixelLAB Configuration")]
    public string apiKey = "YOUR_PIXELLAB_API_KEY_HERE";
    public string apiEndpoint = "https://api.pixellab.ai/v1/generate";
    
    [Header("Character Generation")]
    public string characterStyle = "pixel art, 16-bit, RPG character, fantasy";
    public int characterWidth = 64;
    public int characterHeight = 64;
    
    [Header("UI References")]
    public TextMeshProUGUI statusText;
    public Image characterPreviewImage;
    
    private void Start()
    {
        Debug.Log("🎨 PixelLAB Integration initialized");
        
        // Load API key from PlayerPrefs if saved
        if (PlayerPrefs.HasKey("PixelLAB_API_Key"))
        {
            apiKey = PlayerPrefs.GetString("PixelLAB_API_Key");
        }
    }
    
    public void GenerateCharacter(string characterClass, string characterName)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_PIXELLAB_API_KEY_HERE")
        {
            Debug.LogWarning("⚠️ PixelLAB API key not set!");
            if (statusText != null)
            {
                statusText.text = "Please set your PixelLAB API key in the inspector";
                statusText.color = Color.red;
            }
            return;
        }
        
        string prompt = GenerateCharacterPrompt(characterClass, characterName);
        StartCoroutine(GenerateCharacterCoroutine(prompt));
    }
    
    private string GenerateCharacterPrompt(string characterClass, string characterName)
    {
        string basePrompt = $"{characterStyle}, {characterClass} class";
        
        switch (characterClass.ToLower())
        {
            case "warrior":
                basePrompt += ", armored, sword, shield, strong, heroic";
                break;
            case "mage":
                basePrompt += ", robes, staff, magical, mystical, intelligent";
                break;
            case "archer":
                basePrompt += ", leather armor, bow, arrows, agile, precise";
                break;
            case "rogue":
                basePrompt += ", dark clothing, daggers, stealthy, cunning";
                break;
            default:
                basePrompt += ", adventurer";
                break;
        }
        
        if (!string.IsNullOrEmpty(characterName))
        {
            basePrompt += $", named {characterName}";
        }
        
        return basePrompt;
    }
    
    private IEnumerator GenerateCharacterCoroutine(string prompt)
    {
        Debug.Log($"🎨 Generating character with prompt: {prompt}");
        
        if (statusText != null)
        {
            statusText.text = "🎨 Generating character with AI...";
            statusText.color = Color.magenta;
        }
        
        // Create request payload
        PixelLABRequest request = new PixelLABRequest
        {
            prompt = prompt,
            style = "pixel art",
            width = characterWidth,
            height = characterHeight,
            format = "png"
        };
        
        string jsonRequest = JsonConvert.SerializeObject(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
        
        // Create UnityWebRequest
        using (UnityWebRequest webRequest = new UnityWebRequest(apiEndpoint, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            
            // Send request
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Character generated successfully!");
                
                PixelLABResponse response = null;
                bool parseSuccess = false;
                
                try
                {
                    response = JsonConvert.DeserializeObject<PixelLABResponse>(webRequest.downloadHandler.text);
                    parseSuccess = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Failed to parse API response: {e.Message}");
                    if (statusText != null)
                    {
                        statusText.text = "Failed to parse API response";
                        statusText.color = Color.red;
                    }
                }
                
                if (parseSuccess && response != null)
                {
                    if (response.status == "success" && !string.IsNullOrEmpty(response.image_url))
                    {
                        // Download and display the generated image
                        yield return StartCoroutine(LoadCharacterImage(response.image_url));
                        
                        if (statusText != null)
                        {
                            statusText.text = "🎨 Character generated successfully!";
                            statusText.color = Color.green;
                        }
                    }
                    else
                    {
                        Debug.LogError($"❌ PixelLAB API error: {response.error}");
                        if (statusText != null)
                        {
                            statusText.text = $"API Error: {response.error}";
                            statusText.color = Color.red;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"❌ Character generation failed: {webRequest.error}");
                if (statusText != null)
                {
                    statusText.text = $"Generation failed: {webRequest.error}";
                    statusText.color = Color.red;
                }
            }
        }
    }
    
    private IEnumerator LoadCharacterImage(string imageUrl)
    {
        Debug.Log($"🖼️ Loading character image from: {imageUrl}");
        
        using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return imageRequest.SendWebRequest();
            
            if (imageRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                
                if (characterPreviewImage != null)
                {
                    // Convert texture to sprite
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    characterPreviewImage.sprite = sprite;
                    
                    Debug.Log("✅ Character image loaded and displayed!");
                }
                else
                {
                    Debug.LogWarning("⚠️ Character preview image reference not set");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to load character image: {imageRequest.error}");
            }
        }
    }
    
    // Public method to set API key
    public void SetAPIKey(string newApiKey)
    {
        apiKey = newApiKey;
        PlayerPrefs.SetString("PixelLAB_API_Key", newApiKey);
        PlayerPrefs.Save();
        Debug.Log("🔑 PixelLAB API key updated and saved");
    }
    
    // Method to test API connection
    public void TestAPIConnection()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_PIXELLAB_API_KEY_HERE")
        {
            Debug.LogWarning("⚠️ Please set your PixelLAB API key first!");
            return;
        }
        
        Debug.Log("🧪 Testing PixelLAB API connection...");
        StartCoroutine(TestConnectionCoroutine());
    }
    
    private IEnumerator TestConnectionCoroutine()
    {
        string testPrompt = "simple pixel art test, 16x16";
        
        PixelLABRequest request = new PixelLABRequest
        {
            prompt = testPrompt,
            style = "pixel art",
            width = 16,
            height = 16,
            format = "png"
        };
        
        string jsonRequest = JsonConvert.SerializeObject(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(apiEndpoint, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ PixelLAB API connection successful!");
                if (statusText != null)
                {
                    statusText.text = "✅ API connection successful!";
                    statusText.color = Color.green;
                }
            }
            else
            {
                Debug.LogError($"❌ PixelLAB API connection failed: {webRequest.error}");
                if (statusText != null)
                {
                    statusText.text = $"API connection failed: {webRequest.error}";
                    statusText.color = Color.red;
                }
            }
        }
    }
    
    // Editor method to set API key
    [ContextMenu("Set API Key")]
    public void SetAPIKeyFromEditor()
    {
        Debug.Log("🔑 Use SetAPIKey() method to set your PixelLAB API key");
        Debug.Log("🔑 Or set it directly in the inspector");
    }
}
