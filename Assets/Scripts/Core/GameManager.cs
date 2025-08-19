using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Login,
        CharacterCreation,
        CharacterSelection,
        InGame,
        Dungeon,
        Raid
    }

    public GameState CurrentGameState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("🎮 Blazed Odyssey MMO Alpha - GameManager initialized!");
        }
    }

    public void ChangeGameState(GameState newState)
    {
        CurrentGameState = newState;
        Debug.Log($"🎮 Game State Changed to: {newState}");
        
        // Add scene loading logic here based on state
        switch (newState)
        {
            case GameState.Login:
                LoadSceneIfExists("Login");
                break;
            case GameState.CharacterCreation:
                Debug.Log("🎨 Creating Character Creation scene...");
                CreateModernCharacterCreationUI();
                break;
            case GameState.CharacterSelection:
                Debug.Log("🎉 LOGIN SUCCESSFUL! Loading Character Selection...");
                LoadCharacterSelectionScene();
                break;
            case GameState.InGame:
                LoadSceneIfExists("GameWorldScene");
                break;
            case GameState.Dungeon:
                LoadSceneIfExists("DungeonScene");
                break;
            case GameState.Raid:
                LoadSceneIfExists("RaidScene");
                break;
        }
    }
    
            private void LoadSceneIfExists(string sceneName)
        {
            // For now, just try to load the scene and catch the error gracefully
            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception)
            {
                // Scene doesn't exist - show helpful message
                Debug.Log($"⚠️ Scene '{sceneName}' not found!");
                Debug.Log($"📋 To create: File → New Scene → Save as '{sceneName}.unity' in Assets/Scenes/");
                Debug.Log($"🎮 For now, staying in current scene - this is normal during development!");
            }
        }
        
        private void LoadCharacterSelectionScene()
        {
            Debug.Log("🎭 Loading Character Selection Scene...");
            LoadSceneIfExists("CharacterSelection");
        }
        
        private void CreateCharacterSelectionUI()
        {
            Debug.Log("🎭 Creating Character Selection UI (fallback method)...");
            
            // Create main canvas for character selection
            GameObject canvasObj = new GameObject("CharacterSelectionCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add Canvas Scaler for pixel-perfect scaling
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Optionally add a controller here when available
            
            Debug.Log("✅ Character Selection UI created!");
        }
        
        private void CreateModernCharacterCreationUI()
        {
            Debug.Log("🎨 Creating Character Creation UI...");
            LoadSceneIfExists("CharacterCreation");
        }

    // Example: Call this from a UI button after successful login
    public void OnLoginSuccess()
    {
        ChangeGameState(GameState.CharacterSelection);
    }

    public void OnCharacterSelected()
    {
        ChangeGameState(GameState.InGame);
    }
    
    public void OnBackToCharacterSelection()
    {
        ChangeGameState(GameState.CharacterSelection);
    }
}
