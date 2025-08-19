using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SpumSetupHelper : EditorWindow
{
    [MenuItem("Blazed Odyssey/SPUM Setup Helper")]
    public static void ShowWindow()
    {
        GetWindow<SpumSetupHelper>("SPUM Setup Helper");
    }

    private SpumUnitRegistry registry;
    private Vector2 scrollPosition;

    // Preset definitions
    private struct ClassPreset
    {
        public string presetName;
        public string className;
        public string race;
        public SpumUnitRegistry.Role role;
        public string description;
        public string primaryStat;
        public int health, mana, attack, defense, magic;
        public Color color;
    }

    private static ClassPreset[] presets = new ClassPreset[]
    {
        new ClassPreset{ presetName="Human / Vanguard Knight", className="Vanguard Knight", race="Human", role=SpumUnitRegistry.Role.Tank, description="Disciplined frontline defender wielding sword and shield, sworn to protect allies from harm.", primaryStat="Defense", health=120, mana=60, attack=10, defense=14, magic=6, color=ParseHtmlColor("#708090") },
        new ClassPreset{ presetName="Human / Luminar Priest", className="Luminar Priest", race="Human", role=SpumUnitRegistry.Role.Healer, description="A benevolent healer channeling divine light to restore allies and smite the unholy.", primaryStat="Magic", health=90, mana=120, attack=6, defense=8, magic=14, color=ParseHtmlColor("#FFD700") },
        new ClassPreset{ presetName="Human / Falcon Archer", className="Falcon Archer", race="Human", role=SpumUnitRegistry.Role.DPS, description="Swift and precise ranged specialist, striking from afar with deadly accuracy.", primaryStat="Attack", health=100, mana=80, attack=14, defense=8, magic=6, color=ParseHtmlColor("#228B22") },
        new ClassPreset{ presetName="Human / Shadowblade Rogue", className="Shadowblade Rogue", race="Human", role=SpumUnitRegistry.Role.DPS, description="Cunning assassin who thrives in stealth, delivering swift and critical strikes.", primaryStat="Attack", health=95, mana=70, attack=15, defense=7, magic=6, color=ParseHtmlColor("#191970") },
        new ClassPreset{ presetName="Devil / Infernal Warlord", className="Infernal Warlord", race="Devil", role=SpumUnitRegistry.Role.DPS, description="Demonic knight fueled by dark power, excelling in brutal melee combat.", primaryStat="Attack", health=125, mana=50, attack=15, defense=12, magic=5, color=ParseHtmlColor("#DC143C") },
        new ClassPreset{ presetName="Devil / Nightfang Stalker", className="Nightfang Stalker", race="Devil", role=SpumUnitRegistry.Role.DPS, description="Agile demon thief who hunts prey with lethal precision and supernatural speed.", primaryStat="Attack", health=95, mana=70, attack=16, defense=6, magic=6, color=ParseHtmlColor("#8A2BE2") },
        new ClassPreset{ presetName="Devil / Abyssal Oracle", className="Abyssal Oracle", race="Devil", role=SpumUnitRegistry.Role.Healer, description="Dark priest who channels infernal magic to heal allies and curse enemies.", primaryStat="Magic", health=90, mana=125, attack=7, defense=7, magic=15, color=ParseHtmlColor("#301934") },
        new ClassPreset{ presetName="Skeleton / Bonecaster", className="Bonecaster", race="Skeleton", role=SpumUnitRegistry.Role.DPS, description="Necromantic wizard who bends the arcane to raise minions and unleash deadly spells.", primaryStat="Magic", health=80, mana=140, attack=5, defense=6, magic=16, color=ParseHtmlColor("#7B9FCB") },
        new ClassPreset{ presetName="Skeleton / Grave Knight", className="Grave Knight", race="Skeleton", role=SpumUnitRegistry.Role.Tank, description="Undead warrior bound by cursed armor, a relentless tank on the battlefield.", primaryStat="Defense", health=130, mana=50, attack=11, defense=15, magic=5, color=ParseHtmlColor("#E3DAC9") },
    };

    private static Color ParseHtmlColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
        return Color.white;
    }

    private void OnGUI()
    {
        GUILayout.Label("SPUM Unit Registry Setup", EditorStyles.boldLabel);
        
        // Registry assignment
        registry = (SpumUnitRegistry)EditorGUILayout.ObjectField("SPUM Unit Registry", registry, typeof(SpumUnitRegistry), false);
        
        if (registry == null)
        {
            EditorGUILayout.HelpBox("Please assign a SpumUnitRegistry asset first.", MessageType.Warning);
            if (GUILayout.Button("Create New Registry"))
            {
                CreateNewRegistry();
            }
            return;
        }

        EditorGUILayout.Space();
        
        // Auto-scan button
        if (GUILayout.Button("🔍 Scan SPUM Units Folder (Respects Locked)"))
        {
            ScanSpumUnitsRespectingLocks();
        }
        
        // Debug button to inspect prefabs
        if (GUILayout.Button("🔬 Debug: Inspect SPUM Prefabs"))
        {
            DebugInspectPrefabs();
        }
        
        EditorGUILayout.Space();
        
        // Manual entry
        GUILayout.Label("Add Manual Entry", EditorStyles.boldLabel);
        if (GUILayout.Button("➕ Add Entry"))
        {
            AddNewEntry();
        }
        
        EditorGUILayout.Space();
        
        // Display current entries
        GUILayout.Label("Current Entries", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (registry.entries != null)
        {
            for (int i = 0; i < registry.entries.Length; i++)
            {
                var entry = registry.entries[i];
                if (entry == null) continue;
                
                EditorGUILayout.BeginVertical("box");
                
                // Preset picker row
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Class Preset", GUILayout.Width(90));
                string[] presetNames = presets.Select(p => p.presetName).Prepend("None / Custom").ToArray();
                int selected = EditorGUILayout.Popup(0, presetNames);
                if (selected > 0)
                {
                    ApplyClassPreset(entry, presets[selected - 1]);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                // Preview row
                EditorGUILayout.BeginHorizontal();
                var previewTex = GetPreviewTexture(entry);
                GUILayout.Box(previewTex != null ? previewTex : Texture2D.grayTexture, GUILayout.Width(96), GUILayout.Height(96));
                EditorGUILayout.BeginVertical();
                entry.className = EditorGUILayout.TextField("Class Name", entry.className);
                entry.race = EditorGUILayout.TextField("Race (optional)", entry.race);
                entry.gender = (SpumUnitRegistry.Gender)EditorGUILayout.EnumPopup("Gender", entry.gender);
                entry.prefabName = EditorGUILayout.TextField("Prefab Name", entry.prefabName);
                entry.locked = EditorGUILayout.ToggleLeft("Lock (skip on Scan)", entry.locked);
                
                // Presentation
                EditorGUILayout.LabelField("Presentation", EditorStyles.boldLabel);
                entry.description = EditorGUILayout.TextArea(entry.description, GUILayout.MinHeight(40));
                entry.primaryStat = EditorGUILayout.TextField("Primary Stat", entry.primaryStat);
                entry.classColor = EditorGUILayout.ColorField("Class Color", entry.classColor);
                
                // Role & Stats
                EditorGUILayout.LabelField("Role & Stats", EditorStyles.boldLabel);
                var newRole = (SpumUnitRegistry.Role)EditorGUILayout.EnumPopup("Role Preset", entry.role);
                if (newRole != entry.role)
                {
                    entry.role = newRole;
                    ApplyRolePreset(entry);
                }
                if (GUILayout.Button("Apply Preset Now"))
                {
                    ApplyRolePreset(entry);
                }
                entry.health = EditorGUILayout.IntField("Health", entry.health);
                entry.mana = EditorGUILayout.IntField("Mana", entry.mana);
                entry.attack = EditorGUILayout.IntField("Attack", entry.attack);
                entry.defense = EditorGUILayout.IntField("Defense", entry.defense);
                entry.magic = EditorGUILayout.IntField("Magic", entry.magic);
                EditorGUILayout.EndVertical();
                
                // Actions
                if (GUILayout.Button("Ping", GUILayout.Width(50), GUILayout.Height(22)))
                {
                    var obj = LoadPrefabObject(entry.prefabName);
                    if (obj != null)
                        EditorGUIUtility.PingObject(obj);
                }
                if (GUILayout.Button("Preview", GUILayout.Width(70), GUILayout.Height(22)))
                {
                    var obj = LoadPrefabObject(entry.prefabName);
                    if (obj != null)
                    {
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = obj; // selects asset to show Inspector preview
                        EditorGUIUtility.PingObject(obj);
                    }
                }
                if (GUILayout.Button("❌", GUILayout.Width(30), GUILayout.Height(22)))
                {
                    RemoveEntry(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        // Save button
        if (GUILayout.Button("💾 Save Registry"))
        {
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log("SPUM Unit Registry saved!");
        }
    }

    private Texture2D GetPreviewTexture(SpumUnitRegistry.Entry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.prefabName)) return null;
        string path = $"Assets/SPUM/Resources/Units/{entry.prefabName}.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return null;

        // Find the most representative sprite renderer
        var sr = FindRepresentativeSpriteRenderer(prefab);
        if (sr != null && sr.sprite != null)
        {
            var tex = AssetPreview.GetAssetPreview(sr.sprite);
            if (tex == null)
            {
                // Ask for another repaint until the preview is generated
                Repaint();
            }
            if (tex != null) return tex;
        }
        
        // Fallback to prefab preview
        var prefabPreview = AssetPreview.GetAssetPreview(prefab);
        if (prefabPreview == null) Repaint();
        return prefabPreview;
    }

    private SpriteRenderer FindRepresentativeSpriteRenderer(GameObject prefab)
    {
        var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers == null || renderers.Length == 0) return null;

        SpriteRenderer best = null;
        float bestScore = float.MinValue;
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null || r.sprite == null) continue;
            string n = r.name.ToLowerInvariant();
            // Ignore obvious non-body parts
            if (n.Contains("shadow") || n.Contains("fx") || n.Contains("effect") || n.Contains("projectile")) continue;
            if (n.Contains("weapon") || n.Contains("sword") || n.Contains("dagger") || n.Contains("bow")) continue;
            
            var rect = r.sprite.rect;
            float area = rect.width * rect.height;
            // Prefer higher sorting order slightly (front-most)
            float score = area + (r.sortingOrder * 10f);
            if (score > bestScore)
            {
                bestScore = score;
                best = r;
            }
        }
        // If everything was filtered out, just pick the largest sprite
        if (best == null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || r.sprite == null) continue;
                var rect = r.sprite.rect; float area = rect.width * rect.height;
                if (area > bestScore) { bestScore = area; best = r; }
            }
        }
        return best;
    }

    private Object LoadPrefabObject(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;
        string path = $"Assets/SPUM/Resources/Units/{prefabName}.prefab";
        return AssetDatabase.LoadAssetAtPath<Object>(path);
    }

    private void CreateNewRegistry()
    {
        var newRegistry = CreateInstance<SpumUnitRegistry>();
        string path = "Assets/SPUM/Config/SpumUnitRegistry.asset";
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        
        AssetDatabase.CreateAsset(newRegistry, path);
        AssetDatabase.SaveAssets();
        
        registry = newRegistry;
        Debug.Log($"Created new SPUM Unit Registry at {path}");
    }

    private void ScanSpumUnitsRespectingLocks()
    {
        if (registry == null) return;
        string unitsPath = "Assets/SPUM/Resources/Units";
        if (!Directory.Exists(unitsPath))
        {
            EditorUtility.DisplayDialog("Error", $"SPUM Units folder not found at {unitsPath}", "OK");
            return;
        }

        var prefabFiles = Directory.GetFiles(unitsPath, "*.prefab", SearchOption.TopDirectoryOnly);
        var prefabNames = prefabFiles.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();

        // Start from existing entries and append/update unlocked ones only
        var existing = registry.entries ?? new SpumUnitRegistry.Entry[0];

        // Map existing by prefab name for quick lookup
        System.Collections.Generic.Dictionary<string, int> prefabNameToIndex = new System.Collections.Generic.Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < existing.Length; i++)
        {
            var e = existing[i]; if (e == null) continue;
            if (!string.IsNullOrEmpty(e.prefabName) && !prefabNameToIndex.ContainsKey(e.prefabName))
                prefabNameToIndex.Add(e.prefabName, i);
        }

        var list = existing.ToList();
        foreach (var name in prefabNames)
        {
            if (prefabNameToIndex.TryGetValue(name, out var idx))
            {
                // Update only if not locked
                var e = list[idx];
                if (e.locked) continue;
                e.className = string.IsNullOrEmpty(e.className) || e.className == "Unknown" ? GetSuggestedClassName(name) : e.className;
                if (string.IsNullOrEmpty(e.race)) e.race = "Hero";
                if (string.IsNullOrEmpty(e.prefabName)) e.prefabName = name;
            }
            else
            {
                // Append new entry
                list.Add(new SpumUnitRegistry.Entry
                {
                    className = GetSuggestedClassName(name),
                    race = "Hero",
                    gender = SpumUnitRegistry.Gender.Any,
                    prefabName = name,
                    locked = false
                });
            }
        }

        registry.entries = list.ToArray();
        EditorUtility.SetDirty(registry);
        Debug.Log($"Scan complete. Entries: {registry.entries.Length}. Locked entries preserved.");
    }

    private void DebugInspectPrefabs()
    {
        string unitsPath = "Assets/SPUM/Resources/Units";
        if (!Directory.Exists(unitsPath))
        {
            EditorUtility.DisplayDialog("Error", $"SPUM Units folder not found at {unitsPath}", "OK");
            return;
        }

        var prefabFiles = Directory.GetFiles(unitsPath, "*.prefab", SearchOption.TopDirectoryOnly);
        Debug.Log($"🔬 Inspecting {prefabFiles.Length} SPUM prefabs:");
        
        for (int i = 0; i < prefabFiles.Length; i++)
        {
            string prefabPath = prefabFiles[i];
            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            
            // Load the prefab to inspect its contents
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                Debug.Log($"\n📦 {prefabName}:");
                
                // Check for SpriteRenderer
                var spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>();
                if (spriteRenderers.Length > 0)
                {
                    Debug.Log($"  ✅ Has {spriteRenderers.Length} SpriteRenderer(s)");
                    for (int j = 0; j < spriteRenderers.Length; j++)
                    {
                        var sr = spriteRenderers[j];
                        string spriteName = sr.sprite != null ? sr.sprite.name : "NULL";
                        Debug.Log($"    - {sr.name}: {spriteName}");
                    }
                }
                else
                {
                    Debug.Log($"  ❌ No SpriteRenderer found!");
                }
                
                // Check for Animator
                var animator = prefab.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    Debug.Log($"  ✅ Has Animator: {animator.name}");
                }
                else
                {
                    Debug.Log($"  ❌ No Animator found!");
                }
                
                // Check for other components
                var components = prefab.GetComponentsInChildren<Component>();
                Debug.Log($"  📋 Total components: {components.Length}");
            }
            else
            {
                Debug.LogError($"❌ Failed to load prefab: {prefabPath}");
            }
        }
        
        Debug.Log($"\n🔬 SPUM prefab inspection complete. Check Console for details.");
    }

    private string GetSuggestedClassName(string prefabName)
    {
        // Try to extract class name from prefab name with better logic
        string lowerName = prefabName.ToLowerInvariant();
        
        // Look for specific class indicators in the prefab name
        if (lowerName.Contains("warrior") || lowerName.Contains("fighter") || lowerName.Contains("knight")) 
            return "Warrior";
        if (lowerName.Contains("mage") || lowerName.Contains("wizard") || lowerName.Contains("sorcerer")) 
            return "Mage";
        if (lowerName.Contains("rogue") || lowerName.Contains("assassin") || lowerName.Contains("thief")) 
            return "Rogue";
        if (lowerName.Contains("archer") || lowerName.Contains("ranger") || lowerName.Contains("hunter")) 
            return "Archer";
        if (lowerName.Contains("cleric") || lowerName.Contains("priest") || lowerName.Contains("paladin")) 
            return "Cleric";
        if (lowerName.Contains("monk") || lowerName.Contains("martial")) 
            return "Monk";
        if (lowerName.Contains("druid") || lowerName.Contains("shaman")) 
            return "Druid";
        
        // If no clear class name found, try to extract from the timestamp format
        // SPUM_20250815164302851 -> try to find a meaningful prefix
        if (prefabName.StartsWith("SPUM_"))
        {
            // Try to find any text before the timestamp that might indicate class
            string[] parts = prefabName.Split('_');
            if (parts.Length > 2)
            {
                // Look for any descriptive text between SPUM and the timestamp
                for (int i = 1; i < parts.Length - 1; i++)
                {
                    string part = parts[i].ToLowerInvariant();
                    if (part.Length > 2 && !part.All(char.IsDigit))
                    {
                        // Found a non-timestamp part, use it as class name
                        return char.ToUpper(part[0]) + part.Substring(1);
                    }
                }
            }
        }
        
        // Last resort: use first part after SPUM_ if it's not a timestamp
        string[] segments = prefabName.Split('_');
        if (segments.Length > 1)
        {
            string firstPart = segments[1];
            if (!firstPart.All(char.IsDigit) && firstPart.Length > 2)
            {
                return char.ToUpper(firstPart[0]) + firstPart.Substring(1);
            }
        }
        
        // Default fallback
        return "Unknown";
    }

    private void AddNewEntry()
    {
        if (registry.entries == null)
            registry.entries = new SpumUnitRegistry.Entry[0];
        
        var newEntries = new SpumUnitRegistry.Entry[registry.entries.Length + 1];
        registry.entries.CopyTo(newEntries, 0);
        newEntries[newEntries.Length - 1] = new SpumUnitRegistry.Entry();
        registry.entries = newEntries;
        
        EditorUtility.SetDirty(registry);
    }

    private void RemoveEntry(int index)
    {
        if (registry.entries == null || index < 0 || index >= registry.entries.Length) return;
        
        var newEntries = new SpumUnitRegistry.Entry[registry.entries.Length - 1];
        for (int i = 0, j = 0; i < registry.entries.Length; i++)
        {
            if (i != index)
                newEntries[j++] = registry.entries[i];
        }
        registry.entries = newEntries;
        
        EditorUtility.SetDirty(registry);
    }

    private void ApplyRolePreset(SpumUnitRegistry.Entry entry)
    {
        if (entry == null) return;
        switch (entry.role)
        {
            case SpumUnitRegistry.Role.Tank:
                entry.primaryStat = string.IsNullOrEmpty(entry.primaryStat) ? "Defense" : entry.primaryStat;
                entry.health = Mathf.Max(entry.health, 120);
                entry.mana = Mathf.Max(entry.mana, 60);
                entry.attack = Mathf.Max(entry.attack, 10);
                entry.defense = Mathf.Max(entry.defense, 14);
                entry.magic = Mathf.Max(entry.magic, 5);
                if (entry.classColor == default) entry.classColor = new Color(0.44f, 0.50f, 0.56f); // steel gray
                break;
            case SpumUnitRegistry.Role.Healer:
                entry.primaryStat = string.IsNullOrEmpty(entry.primaryStat) ? "Magic" : entry.primaryStat;
                entry.health = Mathf.Max(entry.health, 90);
                entry.mana = Mathf.Max(entry.mana, 120);
                entry.attack = Mathf.Max(entry.attack, 6);
                entry.defense = Mathf.Max(entry.defense, 8);
                entry.magic = Mathf.Max(entry.magic, 14);
                if (entry.classColor == default) entry.classColor = new Color(1.0f, 0.84f, 0.0f); // white gold
                break;
            case SpumUnitRegistry.Role.DPS:
                entry.primaryStat = string.IsNullOrEmpty(entry.primaryStat) ? "Attack" : entry.primaryStat;
                entry.health = Mathf.Max(entry.health, 95);
                entry.mana = Mathf.Max(entry.mana, 70);
                entry.attack = Mathf.Max(entry.attack, 15);
                entry.defense = Mathf.Max(entry.defense, 7);
                entry.magic = Mathf.Max(entry.magic, 6);
                if (entry.classColor == default) entry.classColor = new Color(0.13f, 0.19f, 0.44f); // midnight-ish
                break;
            case SpumUnitRegistry.Role.None:
            default:
                break;
        }
    }

    private static void ApplyClassPreset(SpumUnitRegistry.Entry entry, ClassPreset preset)
    {
        entry.className = preset.className;
        entry.race = preset.race;
        entry.role = preset.role;
        entry.description = preset.description;
        entry.primaryStat = preset.primaryStat;
        entry.health = preset.health;
        entry.mana = preset.mana;
        entry.attack = preset.attack;
        entry.defense = preset.defense;
        entry.magic = preset.magic;
        entry.classColor = preset.color;
    }
}
