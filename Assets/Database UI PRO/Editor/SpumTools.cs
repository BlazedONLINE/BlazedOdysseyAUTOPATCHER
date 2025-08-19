using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class SpumTools
    {
        [MenuItem("BlazedOdyssey/SPUM/Tag All Prefabs As Player", false, 10)]
        private static void TagAllSpumPrefabsAsPlayer()
        {
            string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            int changed = 0;
            foreach (string guid in allPrefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Replace('\\','/').Contains("/SPUM/"))
                    continue;

                // Load an editable prefab instance, change tag, then save
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                if (prefabRoot == null)
                    continue;
                try
                {
                    if (prefabRoot.tag != "Player")
                    {
                        prefabRoot.tag = "Player";
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                        changed++;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("SPUM Tools", $"Tagged {changed} SPUM prefabs as 'Player'", "OK");
        }
    }
}


