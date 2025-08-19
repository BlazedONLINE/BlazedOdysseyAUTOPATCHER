using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace BlazedOdyssey.Tools.Editor
{
	public static class SuperTiled2UnityFix
	{
		[MenuItem("BlazedOdyssey/Tiles/Fix SuperTiled2Unity PPU (32) & Reimport")]
		public static void FixAndReimport()
		{
			try
			{
				// Find ST2USettings type by scanning all assemblies
				var st2uType = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
					.FirstOrDefault(t => t.Name.Equals("ST2USettings", StringComparison.Ordinal));

				if (st2uType == null)
				{
					EditorUtility.DisplayDialog("ST2U Fix", "Could not find ST2USettings type.", "OK");
					return;
				}

				// Resolve singleton instance (property could be 'instance' or 'Instance')
				var instProp = st2uType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
					.FirstOrDefault(p => p.Name.Equals("instance", StringComparison.OrdinalIgnoreCase));
				object settings = instProp != null ? instProp.GetValue(null) : null;
				if (settings == null)
				{
					EditorUtility.DisplayDialog("ST2U Fix", "Could not access ST2USettings.instance.", "OK");
					return;
				}

				// Find PixelsPerUnit property/field by name contains (case-insensitive)
				bool set = false;
				var pProp = st2uType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.FirstOrDefault(p => p.Name.IndexOf("PixelsPerUnit", StringComparison.OrdinalIgnoreCase) >= 0 && p.CanWrite);
				if (pProp != null)
				{
					pProp.SetValue(settings, 32);
					set = true;
				}
				else
				{
					var pField = st2uType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
						.FirstOrDefault(f => f.Name.IndexOf("PixelsPerUnit", StringComparison.OrdinalIgnoreCase) >= 0);
					if (pField != null) { pField.SetValue(settings, 32); set = true; }
				}

				// Save if available
				var save = st2uType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.FirstOrDefault(m => m.Name.Equals("Save", StringComparison.Ordinal));
				if (save != null) save.Invoke(settings, null);

				if (!set)
				{
					EditorUtility.DisplayDialog("ST2U Fix", "Unable to set Pixels Per Unit. Please set it manually in Project Settings → Super Tiled2Unity.", "OK");
					return;
				}

				// Reimport all Tiled assets
				var guids = AssetDatabase.FindAssets("t:TextAsset");
				int count = 0, total = guids.Length;
				for (int i = 0; i < guids.Length; i++)
				{
					var path = AssetDatabase.GUIDToAssetPath(guids[i]);
					if (path.EndsWith(".tmx", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase))
					{
						EditorUtility.DisplayProgressBar("Reimporting Tiled Assets", path, (float)i / Math.Max(1, total));
						AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
						count++;
					}
				}
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("ST2U Fix", $"Set Pixels Per Unit to 32 and reimported {count} Tiled assets.", "OK");
			}
			catch (Exception ex)
			{
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("ST2U Fix", "Error: " + ex.Message, "OK");
			}
		}
	}
}
