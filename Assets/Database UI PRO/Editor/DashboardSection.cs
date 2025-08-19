using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerARPG
{
    public class DashboardSection : GameDataListSection
    {
        public override string MenuTitle => "Dashboard";
        private Vector2 scrollPosition;

        // Variables for storing statistics
        private int totalAttributes;
        private int totalCurrencies;
        private int totalDamageElements;
        private int totalItems;
        private int totalSkills;
        private int totalPlayerCharacters;
        private int totalMonsterCharacters;
        private int totalQuests;
        private int totalMaps;
        private int totalFactions;

        private Dictionary<string, int> itemsByCategory;
        private Dictionary<string, int> skillsByCategory;
        private Dictionary<string, int> questsByCategory;
        private Dictionary<string, int> attributesByCategory;
        private Dictionary<string, int> currenciesByCategory;
        private Dictionary<string, int> damageElementsByCategory;
        private Dictionary<string, int> playerCharactersByCategory;
        private Dictionary<string, int> monsterCharactersByCategory;
        private Dictionary<string, int> mapsByCategory;
        private Dictionary<string, int> factionsByCategory;

        // Harmonious color palette
        private Color[] colorPalette = new Color[]
        {
            new Color(0.2f, 0.6f, 0.86f),
            new Color(0.95f, 0.77f, 0.06f),
            new Color(0.91f, 0.3f, 0.24f),
            new Color(0.17f, 0.8f, 0.5f),
            new Color(0.61f, 0.35f, 0.71f),
            new Color(0.95f, 0.61f, 0.07f),
            new Color(0.24f, 0.78f, 0.91f),
            new Color(0.9f, 0.49f, 0.13f),
            new Color(0.84f, 0.15f, 0.16f),
            new Color(0.58f, 0.65f, 0.65f),
        };

        public DashboardSection()
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Database UI PRO/Icons/dashboard.png");
        }

        public override void OnGUI(float width, float height)
        {
            var db = GetWorkingDatabase();
            if (db == null)
                return;

            // Collect data
            CollectStatistics(db);

            // Display data
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                GUILayout.Label("Game Data Dashboard", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                // Display totals on cards
                DrawTotalCards();

                EditorGUILayout.Space();

                // Create a list of charts to display
                List<KeyValuePair<string, Dictionary<string, int>>> graphs = new List<KeyValuePair<string, Dictionary<string, int>>>()
                {
                    new KeyValuePair<string, Dictionary<string, int>>("Items by Category", itemsByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Skills by Category", skillsByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Quests by Category", questsByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Attributes by Category", attributesByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Currencies by Category", currenciesByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Damage Elements by Category", damageElementsByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Player Characters by Category", playerCharactersByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Monster Characters by Category", monsterCharactersByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Maps by Category", mapsByCategory),
                    new KeyValuePair<string, Dictionary<string, int>>("Factions by Category", factionsByCategory),
                };

                // Drawing charts in a grid layout
                DrawGraphsInGrid(graphs);
            }
            GUILayout.EndScrollView();
        }

        private void CollectStatistics(ScriptableObject db)
        {
            // Initialize counts
            totalAttributes = GetArrayLength(db, "attributes");
            totalCurrencies = GetArrayLength(db, "currencies");
            totalDamageElements = GetArrayLength(db, "damageElements");
            totalItems = GetArrayLength(db, "items");
            totalSkills = GetArrayLength(db, "skills");
            totalPlayerCharacters = GetArrayLength(db, "playerCharacters");
            totalMonsterCharacters = GetArrayLength(db, "monsterCharacters");
            totalQuests = GetArrayLength(db, "quests");
            totalMaps = GetArrayLength(db, "mapInfos");
            totalFactions = GetArrayLength(db, "factions");

            // Collect data by category
            itemsByCategory = CollectDataByCategory(GetArray(db, "items"));
            skillsByCategory = CollectDataByCategory(GetArray(db, "skills"));
            questsByCategory = CollectDataByCategory(GetArray(db, "quests"));
            attributesByCategory = CollectDataByCategory(GetArray(db, "attributes"));
            currenciesByCategory = CollectDataByCategory(GetArray(db, "currencies"));
            damageElementsByCategory = CollectDataByCategory(GetArray(db, "damageElements"));
            playerCharactersByCategory = CollectDataByCategory(GetArray(db, "playerCharacters"));
            monsterCharactersByCategory = CollectDataByCategory(GetArray(db, "monsterCharacters"));
            mapsByCategory = CollectDataByCategory(GetArray(db, "mapInfos"));
            factionsByCategory = CollectDataByCategory(GetArray(db, "factions"));
        }

        private Dictionary<string, int> CollectDataByCategory(object arr)
        {
            Dictionary<string, int> dataByCategory = new Dictionary<string, int>();
            if (arr is System.Array a)
            {
                foreach (var data in a)
                {
                    string category = GetCategory(data as UnityEngine.ScriptableObject);
                    if (dataByCategory.ContainsKey(category))
                        dataByCategory[category]++;
                    else
                        dataByCategory[category] = 1;
                }
            }
            return dataByCategory;
        }

        private string GetCategory(UnityEngine.ScriptableObject data)
        {
            // Trying to get 'Category' property using reflection
            var categoryProperty = data.GetType().GetProperty("Category");
            if (categoryProperty != null)
            {
                string category = categoryProperty.GetValue(data) as string;
                if (!string.IsNullOrEmpty(category))
                    return category;
            }
            return "Undefined";
        }

        private ScriptableObject GetWorkingDatabase()
        {
            // Try main project's EditorGlobalData first
            var type = System.Type.GetType("MultiplayerARPG.EditorGlobalData, Assembly-CSharp-Editor");
            if (type != null)
            {
                var prop = type.GetProperty("WorkingDatabase", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (prop != null)
                    return prop.GetValue(null) as ScriptableObject;
            }
            // Fall back to Alpha manager helper if available
            var helperType = System.Type.GetType("MultiplayerARPG.GameDatabaseManagerEditor, Assembly-CSharp-Editor");
            if (helperType != null)
            {
                var method = helperType.GetMethod("GetCurrentWorkingDatabase", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (method != null)
                    return method.Invoke(null, null) as ScriptableObject;
            }
            return null;
        }

        private int GetArrayLength(ScriptableObject db, string fieldName)
        {
            var field = db.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field == null) return 0;
            var val = field.GetValue(db) as System.Array;
            return val != null ? val.Length : 0;
        }

        private object GetArray(ScriptableObject db, string fieldName)
        {
            var field = db.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return field != null ? field.GetValue(db) : null;
        }

        private void DrawTotalCards()
        {
            // Prepare styles
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 80,
                margin = new RectOffset(10, 10, 10, 10)
            };

            // Create a flexible grid layout
            int cardsPerRow = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / 200);
            if (cardsPerRow == 0) cardsPerRow = 1;

            int totalCards = 10;
            int rows = Mathf.CeilToInt((float)totalCards / cardsPerRow);
            int cardIndex = 0;

            for (int i = 0; i < rows; i++)
            {
                GUILayout.BeginHorizontal();
                {
                    for (int j = 0; j < cardsPerRow && cardIndex < totalCards; j++, cardIndex++)
                    {
                        string title = "";
                        int count = 0;
                        switch (cardIndex)
                        {
                            case 0:
                                title = "Attributes";
                                count = totalAttributes;
                                break;
                            case 1:
                                title = "Currencies";
                                count = totalCurrencies;
                                break;
                            case 2:
                                title = "Damage Elements";
                                count = totalDamageElements;
                                break;
                            case 3:
                                title = "Items";
                                count = totalItems;
                                break;
                            case 4:
                                title = "Skills";
                                count = totalSkills;
                                break;
                            case 5:
                                title = "Player Characters";
                                count = totalPlayerCharacters;
                                break;
                            case 6:
                                title = "Monster Characters";
                                count = totalMonsterCharacters;
                                break;
                            case 7:
                                title = "Quests";
                                count = totalQuests;
                                break;
                            case 8:
                                title = "Maps";
                                count = totalMaps;
                                break;
                            case 9:
                                title = "Factions";
                                count = totalFactions;
                                break;
                        }

                        GUILayout.BeginVertical(cardStyle, GUILayout.Width(180));
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(title, EditorStyles.largeLabel);
                            GUILayout.Label(count.ToString(), EditorStyles.boldLabel);
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndVertical();
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawGraphsInGrid(List<KeyValuePair<string, Dictionary<string, int>>> graphs)
        {
            // Set the number of graphs per row
            int graphsPerRow = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / 550);
            if (graphsPerRow == 0) graphsPerRow = 1;

            int totalGraphs = graphs.Count;
            int rows = Mathf.CeilToInt((float)totalGraphs / graphsPerRow);
            int graphIndex = 0;

            for (int i = 0; i < rows; i++)
            {
                GUILayout.BeginHorizontal();
                {
                    for (int j = 0; j < graphsPerRow && graphIndex < totalGraphs; j++, graphIndex++)
                    {
                        var graph = graphs[graphIndex];
                        DrawBarGraphSection(graph.Key, graph.Value);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawBarGraphSection(string title, Dictionary<string, int> data)
        {
            // Start card for chart
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(10, 10, 10, 10),
                fixedWidth = 550f // Sets the fixed width of the card
            };

            GUILayout.BeginVertical(cardStyle);
            {
                GUILayout.Label(title, EditorStyles.boldLabel);
                if (data == null || data.Count == 0)
                {
                    GUILayout.Label("No data available.");
                }
                else
                {
                    DrawVerticalBarGraph(data);
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawVerticalBarGraph(Dictionary<string, int> data)
        {
            float maxCount = data.Values.Max();
            int barCount = data.Count;
            float graphWidth = 550f; // Adjust according to card width
            float graphHeight = 200f; // Adjust the chart height as needed
            float barWidth = Mathf.Min(50f, (graphWidth - 20f) / barCount - 10f); // Adjust the width of the bars

            Rect graphRect = GUILayoutUtility.GetRect(graphWidth, graphHeight);

            // Draw the chart background
            EditorGUI.DrawRect(graphRect, new Color(0.2f, 0.2f, 0.2f));

            int index = 0;
            foreach (var entry in data.OrderByDescending(e => e.Value))
            {
                float barHeight = (entry.Value / maxCount) * (graphHeight - 40f); // Space for labels
                float x = graphRect.x + 10f + index * (barWidth + 10f);
                float y = graphRect.y + graphHeight - barHeight - 20f; // Adjustment for label spacing

                Rect barRect = new Rect(x, y, barWidth, barHeight);
                EditorGUI.DrawRect(barRect, colorPalette[index % colorPalette.Length]);

                // Value label above the bar
                GUIStyle valueStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.LowerCenter
                };
                Rect valueRect = new Rect(x, y - 15f, barWidth, 15f);
                EditorGUI.LabelField(valueRect, entry.Value.ToString(), valueStyle);

                // Category label below the bar
                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.UpperCenter,
                    wordWrap = true
                };
                Rect labelRect = new Rect(x - 10f, graphRect.y + graphHeight - 20f, barWidth + 20f, 40f);
                EditorGUI.LabelField(labelRect, entry.Key, labelStyle);

                index++;
            }
        }
    }
}
