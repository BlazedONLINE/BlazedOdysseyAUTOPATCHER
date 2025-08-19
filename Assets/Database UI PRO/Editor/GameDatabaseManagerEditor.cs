using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameDatabaseManagerEditor : EditorWindow
    {
        private List<GameDataListSection> sections;
        private Vector2 menuScrollPosition;
        private int selectedMenuIndex;
        private ScriptableObject selectedDatabase;
        private static ScriptableObject s_localWorkingDatabase;
        private Texture2D defaultIcon;

        [MenuItem(EditorMenuConsts.GAME_DATABASE_MENU, false, EditorMenuConsts.GAME_DATABASE_ORDER)]
        public static void CreateNewEditor()
        {
            GetWindow<GameDatabaseManagerEditor>();
        }

        private void OnEnable()
        {
            selectedDatabase = null;
            defaultIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Database UI PRO/Icons/default.png");
            BuildSections(GetWorkingDatabase());
        }

        private void OnDisable()
        {
            SetWorkingDatabase(null);
        }

        protected virtual void OnGUI()
        {
            titleContent = new GUIContent("Game Database", null, "Game Database");
            if (GetWorkingDatabase() == null)
            {
                Vector2 wndRect = new Vector2(500, 100);
                minSize = wndRect;

                GUILayout.BeginVertical("Game Database", "window");
                {
                    GUILayout.BeginVertical("box");
                    {
                        if (GetWorkingDatabase() == null)
                            EditorGUILayout.HelpBox("Select the game database that you want to edit", MessageType.Info);
                        var picked = EditorGUILayout.ObjectField("Game database", GetWorkingDatabase(), typeof(ScriptableObject), true, GUILayout.ExpandWidth(true)) as ScriptableObject;
                        if (picked != null)
                        {
                            Type pickedType = picked.GetType();
                            if (pickedType.GetMethod("LoadReferredData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                            {
                                SetWorkingDatabase(picked);
                            }
                        }
                        GUILayout.Space(6);
                        if (GUILayout.Button("Auto-select GameDatabase", GUILayout.Height(24)))
                        {
                            TryAutoSelectDatabase();
                        }
                        GUILayout.Space(6);
                        GUILayout.Label("Found GameDatabase assets:", EditorStyles.boldLabel);
                        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject GameDatabase");
                        if (guids != null && guids.Length > 0)
                        {
                            foreach (var guid in guids)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(guid);
                                var db = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                                if (db == null) continue;
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.ObjectField(db, typeof(ScriptableObject), false);
                                GUILayout.Label(path, GUILayout.ExpandWidth(true));
                                if (GUILayout.Button("Use", GUILayout.Width(80)))
                                {
                                    SetWorkingDatabase(db);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
                return;
            }
            else
            {
                Vector2 wndRect = new Vector2(800, 600);
                minSize = wndRect;
            }

            var workingDatabase = GetWorkingDatabase();
            if (workingDatabase != selectedDatabase)
            {
                selectedDatabase = workingDatabase;
                InvokeLoadReferredData(selectedDatabase);
                BuildSections(selectedDatabase);
            }

            // Prepare GUI styles
            GUIStyle leftMenuButton = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 40,
                margin = new RectOffset(5, 5, 5, 5)
            };

            GUIStyle selectedLeftMenuButton = new GUIStyle(leftMenuButton)
            {
                normal = { background = MakeTex(600, 1, new Color(0.24f, 0.49f, 0.90f)), textColor = Color.white }
            };

            GUILayout.BeginHorizontal();
            {
                // Left menu
                GUILayout.BeginVertical("box", GUILayout.Width(200));
                {
                    menuScrollPosition = GUILayout.BeginScrollView(menuScrollPosition);
                    {
                        for (int i = 0; i < sections.Count; ++i)
                        {
                            GameDataListSection section = sections[i];
                            Texture2D icon = section.icon ?? defaultIcon;

                            GUIContent content = new GUIContent($"  {section.MenuTitle}", icon);

                            if (GUILayout.Button(content, i == selectedMenuIndex ? selectedLeftMenuButton : leftMenuButton))
                            {
                                selectedMenuIndex = i;
                                GUI.FocusControl(null);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                // Content
                GUILayout.BeginVertical();
                {
                    sections[selectedMenuIndex].OnGUI(position.width - 200, position.height);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void BuildSections(ScriptableObject db)
        {
            sections = new List<GameDataListSection>();
            sections.Add(new DashboardSection());
            string typeName = db != null ? db.GetType().FullName : string.Empty;
            if (!string.IsNullOrEmpty(typeName) && typeName.Contains("BlazedOdyssey.Database.DatabaseAsset"))
            {
                // Use BlazedOdyssey DatabaseAsset schema (all categories)
                sections.Add(new ReflectedListSection("attributes", "Attributes", "Assets/Database UI PRO/Icons/attributes.png"));
                sections.Add(new ReflectedListSection("currencies", "Currencies", "Assets/Database UI PRO/Icons/currencies.png"));
                sections.Add(new ReflectedListSection("damageElements", "Damage Elements", "Assets/Database UI PRO/Icons/damageElements.png"));
                sections.Add(new ReflectedListSection("items", "Items", "Assets/Database UI PRO/Icons/items.png"));
                sections.Add(new ReflectedListSection("itemCraftFormulas", "Item Crafts", "Assets/Database UI PRO/Icons/itemCrafts.png"));
                sections.Add(new ReflectedListSection("armorTypes", "Armor Types", "Assets/Database UI PRO/Icons/armorTypes.png"));
                sections.Add(new ReflectedListSection("weaponTypes", "Weapon Types", "Assets/Database UI PRO/Icons/weaponTypes.png"));
                sections.Add(new ReflectedListSection("ammoTypes", "Ammo Types", "Assets/Database UI PRO/Icons/ammoTypes.png"));
                sections.Add(new ReflectedListSection("statusEffects", "Status Effects", "Assets/Database UI PRO/Icons/statusEffects.png"));
                sections.Add(new ReflectedListSection("skills", "Skills", "Assets/Database UI PRO/Icons/skills.png"));
                sections.Add(new ReflectedListSection("guildSkills", "Guild Skills", "Assets/Database UI PRO/Icons/guildSkills.png"));
                sections.Add(new ReflectedListSection("guildIcons", "Guild Icons", "Assets/Database UI PRO/Icons/guildIcons.png"));
                sections.Add(new ReflectedListSection("characters", "Player Characters", "Assets/Database UI PRO/Icons/playerCharacters.png"));
                sections.Add(new ReflectedListSection("monsterCharacters", "Monster Characters", "Assets/Database UI PRO/Icons/monsterCharacters.png"));
                sections.Add(new ReflectedListSection("harvestables", "Harvestables", "Assets/Database UI PRO/Icons/harvestables.png"));
                sections.Add(new ReflectedListSection("mapInfos", "Map Infos", "Assets/Database UI PRO/Icons/mapInfos.png"));
                sections.Add(new ReflectedListSection("quests", "Quests", "Assets/Database UI PRO/Icons/quests.png"));
                sections.Add(new ReflectedListSection("factions", "Factions", "Assets/Database UI PRO/Icons/factions.png"));
            }
            else
            {
                // Default to ARPG-like schema
                sections.Add(new ReflectedListSection("attributes", "Attributes", "Assets/Database UI PRO/Icons/attributes.png"));
                sections.Add(new ReflectedListSection("currencies", "Currencies", "Assets/Database UI PRO/Icons/currencies.png"));
                sections.Add(new ReflectedListSection("damageElements", "Damage Elements", "Assets/Database UI PRO/Icons/damageElements.png"));
                sections.Add(new ReflectedListSection("items", "Items", "Assets/Database UI PRO/Icons/items.png"));
                sections.Add(new ReflectedListSection("itemCraftFormulas", "Item Crafts", "Assets/Database UI PRO/Icons/itemCrafts.png"));
                sections.Add(new ReflectedListSection("armorTypes", "Armor Types", "Assets/Database UI PRO/Icons/armorTypes.png"));
                sections.Add(new ReflectedListSection("weaponTypes", "Weapon Types", "Assets/Database UI PRO/Icons/weaponTypes.png"));
                sections.Add(new ReflectedListSection("ammoTypes", "Ammo Types", "Assets/Database UI PRO/Icons/ammoTypes.png"));
                sections.Add(new ReflectedListSection("statusEffects", "Status Effects", "Assets/Database UI PRO/Icons/statusEffects.png"));
                sections.Add(new ReflectedListSection("skills", "Skills", "Assets/Database UI PRO/Icons/skills.png"));
                sections.Add(new ReflectedListSection("guildSkills", "Guild Skills", "Assets/Database UI PRO/Icons/guildSkills.png"));
                sections.Add(new ReflectedListSection("guildIcons", "Guild Icons", "Assets/Database UI PRO/Icons/guildIcons.png"));
                sections.Add(new ReflectedListSection("playerCharacters", "Player Characters", "Assets/Database UI PRO/Icons/playerCharacters.png"));
                sections.Add(new ReflectedListSection("monsterCharacters", "Monster Characters", "Assets/Database UI PRO/Icons/monsterCharacters.png"));
                sections.Add(new ReflectedListSection("harvestables", "Harvestables", "Assets/Database UI PRO/Icons/harvestables.png"));
                sections.Add(new ReflectedListSection("mapInfos", "Map Infos", "Assets/Database UI PRO/Icons/mapInfos.png"));
                sections.Add(new ReflectedListSection("quests", "Quests", "Assets/Database UI PRO/Icons/quests.png"));
                sections.Add(new ReflectedListSection("factions", "Factions", "Assets/Database UI PRO/Icons/factions.png"));
            }
        }

        private static void TryAutoSelectDatabase()
        {
            string[] candidates = new string[]
            {
                "Assets/UnityMultiplayerARPG/Demo/GameData/GameDatabase.asset",
                "Assets/BlazedOdyssey/Database/GameDatabase.asset",
                "Assets/GameDatabase.asset",
                "Assets/Database UI PRO/TempGameDatabase.asset"
            };
            foreach (var path in candidates)
            {
                var db = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (db != null && db.GetType().GetMethod("LoadReferredData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                {
                    SetWorkingDatabase(db);
                    return;
                }
            }
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject GameDatabase");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var db = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (db != null && db.GetType().GetMethod("LoadReferredData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                {
                    SetWorkingDatabase(db);
                    return;
                }
            }
        }

        private static ScriptableObject GetWorkingDatabase()
        {
            var type = Type.GetType("MultiplayerARPG.EditorGlobalData, Assembly-CSharp-Editor");
            if (type != null)
            {
                var prop = type.GetProperty("WorkingDatabase", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                    return prop.GetValue(null) as ScriptableObject;
            }
            return s_localWorkingDatabase;
        }

        public static ScriptableObject GetCurrentWorkingDatabase()
        {
            return GetWorkingDatabase();
        }

        private static void SetWorkingDatabase(ScriptableObject db)
        {
            var type = Type.GetType("MultiplayerARPG.EditorGlobalData, Assembly-CSharp-Editor");
            if (type != null)
            {
                var prop = type.GetProperty("WorkingDatabase", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(null, db);
                    return;
                }
            }
            s_localWorkingDatabase = db;
        }

        private static void InvokeLoadReferredData(ScriptableObject db)
        {
            if (db == null) return;
            var method = db.GetType().GetMethod("LoadReferredData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(db, null);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
