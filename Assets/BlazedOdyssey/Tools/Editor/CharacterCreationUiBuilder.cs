using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using BlazedOdyssey.UI;
using BlazedOdyssey.UI.Common;

namespace BlazedOdyssey.Tools.Editor
{
	public static class CharacterCreationUiBuilder
	{
		private const string PrefabFolder = "Assets/BlazedOdyssey/Prefabs/UI";
		public const string CharacterCreationPrefabPath = PrefabFolder + "/CharacterCreationCanvas.prefab";

		[MenuItem("BlazedOdyssey/UI/Build Character Creation (Scalable)")]
		public static GameObject BuildCharacterCreation()
		{
			// Ensure folders
			if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey")) AssetDatabase.CreateFolder("Assets", "BlazedOdyssey");
			if (!AssetDatabase.IsValidFolder("Assets/BlazedOdyssey/Prefabs")) AssetDatabase.CreateFolder("Assets/BlazedOdyssey", "Prefabs");
			if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder("Assets/BlazedOdyssey/Prefabs", "UI");

			// Remove existing prefab if present (avoid corruption)
			var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterCreationPrefabPath);
			if (existing != null)
			{
				AssetDatabase.DeleteAsset(CharacterCreationPrefabPath);
			}

			// Create Canvas
			var canvasGO = new GameObject("CharacterCreationCanvas", typeof(Canvas), typeof(GraphicRaycaster));
			var canvas = canvasGO.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			var scaler = canvasGO.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			scaler.matchWidthOrHeight = 0.5f;

			// Ensure EventSystem exists
			if (!Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>())
			{
				new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
			}

			// Background (try to use Blazed Odyssey login background if found)
			var bg = new GameObject("Background", typeof(RectTransform), typeof(RawImage));
			var bgRT = bg.GetComponent<RectTransform>(); bgRT.SetParent(canvasGO.transform, false);
			bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
			var bgRaw = bg.GetComponent<RawImage>(); bgRaw.color = Color.white;
			// Try exact path then GUID search
			Texture2D bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BlazedOdyssey/UI/Art/login_bg_blazed_odyssey_1920x1080.png");
			if (bgTex == null)
			{
				var guids = AssetDatabase.FindAssets("t:Texture2D login_bg_blazed_odyssey_1920x1080");
				if (guids.Length > 0)
				{
					bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
				}
			}
			bgRaw.texture = bgTex; // may be null; controller UI still works without

			// Root for controller
			var root = new GameObject("CharacterCreation", typeof(RectTransform));
			root.transform.SetParent(canvasGO.transform, false);
			root.AddComponent<HudAdjustable>().WidgetId = "CharacterCreationPanel";

			// Attach controller (builds UI at runtime)
			// root.AddComponent<CharacterCreationController>(); // TODO: Create this controller class

			// Save prefab and keep instance
			PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, CharacterCreationPrefabPath, InteractionMode.UserAction);
			Selection.activeObject = canvasGO;
			EditorGUIUtility.PingObject(canvasGO);
			return canvasGO;
		}
	}
}


