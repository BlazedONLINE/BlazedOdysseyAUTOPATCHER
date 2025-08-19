#nullable enable
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace BlazedOdyssey.UI
{
    /// <summary>
    /// Detects current scene state to determine when HUD elements should be visible
    /// </summary>
    public static class SceneStateDetector
    {
        public enum SceneType
        {
            Login,
            CharacterSelection,
            CharacterCreation,
            GameMap,
            Dungeon,
            Unknown
        }

        private static readonly HashSet<string> LoginScenes = new()
        {
            "login", "loginscene", "mmologinscene", "mmologin"
        };

        private static readonly HashSet<string> CharacterScenes = new()
        {
            "characterselection", "charselect", "mmocharacterselectionscene", 
            "charactercreation", "charcreate", "mmocharactercreationscene"
        };

        private static readonly HashSet<string> GameMapScenes = new()
        {
            "gameworldscene", "gameworld", "map", "world", "maingame", 
            "ingame", "gameplay", "mmomap", "mmogame", "startermapscene"
        };

        private static readonly HashSet<string> DungeonScenes = new()
        {
            "dungeon", "dungeonscene", "raid", "raidscene", "instance"
        };

        private static string? lastLoggedScene = null;
        
        public static SceneType GetCurrentSceneType()
        {
            var sceneName = SceneManager.GetActiveScene().name.ToLower();
            
            // Only log once per scene to reduce console spam
            if (lastLoggedScene != sceneName)
            {
                Debug.Log($"[SceneStateDetector] Scene changed to: '{sceneName}'");
                lastLoggedScene = sceneName;
            }
            
            if (LoginScenes.Contains(sceneName))
            {
                return SceneType.Login;
            }
            
            if (CharacterScenes.Contains(sceneName))
            {
                // Additional check for character creation vs selection
                if (sceneName.Contains("creation") || sceneName.Contains("create"))
                {
                    return SceneType.CharacterCreation;
                }
                return SceneType.CharacterSelection;
            }
            
            if (GameMapScenes.Contains(sceneName))
            {
                return SceneType.GameMap;
            }
            
            if (DungeonScenes.Contains(sceneName))
            {
                return SceneType.Dungeon;
            }

            // Check GameManager state if available
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.CurrentGameState switch
                {
                    GameManager.GameState.Login => SceneType.Login,
                    GameManager.GameState.CharacterSelection => SceneType.CharacterSelection,
                    GameManager.GameState.CharacterCreation => SceneType.CharacterCreation,
                    GameManager.GameState.InGame => SceneType.GameMap,
                    GameManager.GameState.Dungeon => SceneType.Dungeon,
                    GameManager.GameState.Raid => SceneType.Dungeon,
                    _ => SceneType.Unknown
                };
            }
            
            // Fallback: If it's not a known character/login scene, assume it's a gameplay scene
            // This handles custom scene names that aren't in our predefined lists
            if (sceneName.Contains("character") || sceneName.Contains("login") || sceneName.Contains("menu"))
            {
                return SceneType.CharacterSelection; // Default to character selection for UI scenes
            }
            
            return SceneType.GameMap; // Default to gameplay scene for unknown scenes
        }
        
        /// <summary>
        /// Add current scene to the appropriate scene list for better detection
        /// </summary>
        public static void AddCurrentSceneToGameplayScenes()
        {
            var sceneName = SceneManager.GetActiveScene().name.ToLower();
            GameMapScenes.Add(sceneName);
        }
        
        public static void AddCurrentSceneToCharacterScenes()
        {
            var sceneName = SceneManager.GetActiveScene().name.ToLower();
            CharacterScenes.Add(sceneName);
        }
        
        public static void LogCurrentSceneInfo()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            var sceneType = GetCurrentSceneType();
            Debug.Log($"[SceneStateDetector] Current Scene: '{sceneName}' | Detected Type: {sceneType} | Should Show HUD: {ShouldShowHUD()} | Should Show Chat: {ShouldShowChat()}");
        }

        public static bool ShouldShowHUD()
        {
            var sceneType = GetCurrentSceneType();
            return sceneType == SceneType.GameMap || sceneType == SceneType.Dungeon;
        }

        public static bool ShouldShowChat()
        {
            var sceneType = GetCurrentSceneType();
            return sceneType == SceneType.GameMap || sceneType == SceneType.Dungeon;
        }

        public static bool ShouldAllowHUDEditing()
        {
            var sceneType = GetCurrentSceneType();
            return sceneType == SceneType.GameMap || sceneType == SceneType.Dungeon;
        }

        public static string GetSceneTypeDisplayName()
        {
            return GetCurrentSceneType() switch
            {
                SceneType.Login => "Login",
                SceneType.CharacterSelection => "Character Selection",
                SceneType.CharacterCreation => "Character Creation",
                SceneType.GameMap => "Game Map",
                SceneType.Dungeon => "Dungeon/Raid",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Check if the current scene is a gameplay scene (map or dungeon)
        /// </summary>
        public static bool IsInGameplayScene()
        {
            return ShouldShowHUD();
        }
    }
}