using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BlazedOdyssey.Tools.Editor
{
	public static class PostProcessingEmergencySwitch
	{
		[MenuItem("BlazedOdyssey/Visual/Disable ALL Post-Processing (Emergency)")]
		public static void DisableAll()
		{
			int volumes = ToggleVolumes(false);
			int cams = ToggleCameraPP(false);
			EditorSceneManager.MarkAllScenesDirty();
			EditorUtility.DisplayDialog("Post-Processing Disabled",
				$"Disabled {volumes} Volume/PostProcessVolume components and turned off post-processing on {cams} cameras.",
				"OK");
		}

		[MenuItem("BlazedOdyssey/Visual/Enable ALL Post-Processing (Emergency)")]
		public static void EnableAll()
		{
			int volumes = ToggleVolumes(true);
			int cams = ToggleCameraPP(true);
			EditorSceneManager.MarkAllScenesDirty();
			EditorUtility.DisplayDialog("Post-Processing Enabled",
				$"Enabled {volumes} Volume/PostProcessVolume components and toggled post-processing on {cams} cameras.",
				"OK");
		}

		private static int ToggleVolumes(bool enable)
		{
			int count = 0;
			// Try URP/HDRP Volume
			var volumeType = FindTypeAnyAssembly("UnityEngine.Rendering.Volume");
			if (volumeType != null)
			{
				var vols = UnityEngine.Object.FindObjectsByType(volumeType, FindObjectsInactive.Include, FindObjectsSortMode.None);
				foreach (var v in vols)
				{
					var beh = v as Behaviour;
					if (beh != null)
					{
						beh.enabled = enable;
						count++;
					}
				}
			}
			// Try legacy PostProcessVolume
			var ppVolType = FindTypeAnyAssembly("UnityEngine.Rendering.PostProcessing.PostProcessVolume");
			if (ppVolType != null)
			{
				var vols = UnityEngine.Object.FindObjectsByType(ppVolType, FindObjectsInactive.Include, FindObjectsSortMode.None);
				foreach (var v in vols)
				{
					var beh = v as Behaviour;
					if (beh != null)
					{
						beh.enabled = enable;
						count++;
					}
				}
			}
			return count;
		}

		private static int ToggleCameraPP(bool enable)
		{
			int count = 0;
			var cams = Camera.allCameras;
			// URP: UniversalAdditionalCameraData.renderPostProcessing
			var urpType = FindTypeAnyAssembly("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
			PropertyInfo renderPPProp = null;
			if (urpType != null)
				renderPPProp = urpType.GetProperty("renderPostProcessing", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (var cam in cams)
			{
				bool toggled = false;
				if (urpType != null && renderPPProp != null)
				{
					var data = cam.GetComponent(urpType);
					if (data != null)
					{
						renderPPProp.SetValue(data, enable);
						toggled = true;
					}
				}
				// Legacy PostProcessLayer on camera
				var legacyLayerType = FindTypeAnyAssembly("UnityEngine.Rendering.PostProcessing.PostProcessLayer");
				if (legacyLayerType != null)
				{
					var layer = cam.GetComponent(legacyLayerType) as Behaviour;
					if (layer != null)
					{
						layer.enabled = enable;
						toggled = true;
					}
				}
				if (toggled) count++;
			}
			return count;
		}

		private static Type FindTypeAnyAssembly(string fullName)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
				.FirstOrDefault(t => t.FullName == fullName);
		}
	}
}
