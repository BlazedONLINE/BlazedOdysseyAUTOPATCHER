#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BlazedOdyssey.UI
{
    [Serializable]
    public class HudElementState { public string id=""; public Vector2 pos; public Vector2 size; }
    [Serializable]
    public class HudLayout { public List<HudElementState> items = new(); }
    
    /// <summary>
    /// Container for HUD element transform data (position and size)
    /// </summary>
    [Serializable]
    public struct HudTransformData
    {
        public Vector2 position;
        public Vector2 size;
        
        public HudTransformData(Vector2 pos, Vector2 sz)
        {
            position = pos;
            size = sz;
        }
    }

    public static class HudLayoutStore
    {
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, "hud_layout.json");
        
        public static void Save(Dictionary<string, Vector2> map)
        {
            try
            {
                var layout = new HudLayout();
                foreach (var kv in map)
                    layout.items.Add(new HudElementState { id = kv.Key, pos = kv.Value });
                
                var json = JsonUtility.ToJson(layout, true);
                File.WriteAllText(Path, json);
#if UNITY_EDITOR
                Debug.Log($"[HUD] Saved layout → {Path}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HUD] Failed to save layout: {ex.Message}");
            }
        }

        public static bool Load(out Dictionary<string, Vector2> map)
        {
            map = new Dictionary<string, Vector2>();
            
            try
            {
                if (!File.Exists(Path)) return false;
                
                var json = File.ReadAllText(Path);
                if (string.IsNullOrEmpty(json)) return false;
                
                var layout = JsonUtility.FromJson<HudLayout>(json);
                if (layout?.items == null) return false;
                
                foreach (var it in layout.items)
                {
                    if (!string.IsNullOrEmpty(it.id))
                        map[it.id] = it.pos;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HUD] Failed to load layout: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Save HUD layout with both position and size data
        /// </summary>
        public static void SaveComplete(Dictionary<string, HudTransformData> transformMap)
        {
            try
            {
                var layout = new HudLayout();
                foreach (var kv in transformMap)
                    layout.items.Add(new HudElementState { id = kv.Key, pos = kv.Value.position, size = kv.Value.size });
                
                var json = JsonUtility.ToJson(layout, true);
                File.WriteAllText(Path, json);
#if UNITY_EDITOR
                Debug.Log($"[HUD] Saved complete layout (position + size) → {Path}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HUD] Failed to save complete layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Load HUD layout with both position and size data
        /// </summary>
        public static bool LoadComplete(out Dictionary<string, HudTransformData> transformMap)
        {
            transformMap = new Dictionary<string, HudTransformData>();
            
            try
            {
                if (!File.Exists(Path)) return false;
                
                var json = File.ReadAllText(Path);
                if (string.IsNullOrEmpty(json)) return false;
                
                var layout = JsonUtility.FromJson<HudLayout>(json);
                if (layout?.items == null) return false;
                
                foreach (var it in layout.items)
                {
                    if (!string.IsNullOrEmpty(it.id))
                        transformMap[it.id] = new HudTransformData(it.pos, it.size);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HUD] Failed to load complete layout: {ex.Message}");
                return false;
            }
        }
    }
}