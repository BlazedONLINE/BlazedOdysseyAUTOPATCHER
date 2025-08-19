#if DATABASE_UI_PRO_STUBS
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
	[CustomEditor(typeof(PlayerCharacter))]
	public class PlayerCharacterEditor : Editor
	{
		SerializedProperty idProp;
		SerializedProperty categoryProp;
		SerializedProperty iconProp;
		SerializedProperty displayNameProp;
		SerializedProperty spumUnitIdProp;
		SerializedProperty portraitProp;
		SerializedProperty spumPrefabProp;
		SerializedProperty spumScaleProp;
		SerializedProperty previewOffsetProp;
		SerializedProperty previewZoomProp;

		private void OnEnable()
		{
			idProp = serializedObject.FindProperty("Id");
			categoryProp = serializedObject.FindProperty("Category");
			iconProp = serializedObject.FindProperty("Icon");
			displayNameProp = serializedObject.FindProperty("DisplayName");
			spumUnitIdProp = serializedObject.FindProperty("SpumUnitId");
			portraitProp = serializedObject.FindProperty("Portrait");
			spumPrefabProp = serializedObject.FindProperty("SpumPrefab");
			spumScaleProp = serializedObject.FindProperty("SpumScale");
			previewOffsetProp = serializedObject.FindProperty("PreviewOffset");
			previewZoomProp = serializedObject.FindProperty("PreviewZoom");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(idProp, new GUIContent("Id"));
			EditorGUILayout.PropertyField(displayNameProp, new GUIContent("Display Name"));
			EditorGUILayout.PropertyField(categoryProp, new GUIContent("Category"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("SPUM Setup", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(spumUnitIdProp, new GUIContent("SPUM Unit Id"));
			EditorGUILayout.PropertyField(spumPrefabProp, new GUIContent("SPUM Prefab"));
			EditorGUILayout.PropertyField(spumScaleProp, new GUIContent("World Scale"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Portrait / Preview", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(portraitProp, new GUIContent("Portrait Sprite"));
			EditorGUILayout.PropertyField(iconProp, new GUIContent("List Icon"));
			EditorGUILayout.PropertyField(previewOffsetProp, new GUIContent("Preview Offset"));
			EditorGUILayout.PropertyField(previewZoomProp, new GUIContent("Preview Zoom"));

			serializedObject.ApplyModifiedProperties();

			// Simple preview
			var pc = (PlayerCharacter)target;
			Texture2D preview = null;
			if (pc.Portrait != null) preview = pc.Portrait.texture;
			if (preview == null && pc.SpumPrefab != null) preview = AssetPreview.GetAssetPreview(pc.SpumPrefab);
			if (preview != null)
			{
				GUILayout.Space(10);
				GUILayout.Label(preview, GUILayout.Height(128));
			}
			EditorGUILayout.HelpBox("Assign your SPUM prefab and unit id. Portrait is optional; if omitted, a prefab preview is shown.", MessageType.Info);
		}
	}
}


 #endif