using UnityEditor;
using UnityEngine;
using BlazedOdyssey.Database.Data;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(PlayerCharacterDB))]
    public class PlayerCharacterDBEditor : Editor
    {
        private SerializedProperty idProp;
        private SerializedProperty displayNameProp;
        private SerializedProperty classNameProp;
        private SerializedProperty portraitProp;
        private SerializedProperty spumUnitIdProp;
        private SerializedProperty spumPrefabProp;
        private SerializedProperty spumScaleProp;
        private SerializedProperty previewOffsetProp;
        private SerializedProperty previewZoomProp;

        private void OnEnable()
        {
            idProp = serializedObject.FindProperty("id");
            displayNameProp = serializedObject.FindProperty("displayName");
            classNameProp = serializedObject.FindProperty("className");
            portraitProp = serializedObject.FindProperty("portrait");
            spumUnitIdProp = serializedObject.FindProperty("spumUnitId");
            spumPrefabProp = serializedObject.FindProperty("spumPrefab");
            spumScaleProp = serializedObject.FindProperty("spumScale");
            previewOffsetProp = serializedObject.FindProperty("previewOffset");
            previewZoomProp = serializedObject.FindProperty("previewZoom");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(idProp);
            EditorGUILayout.PropertyField(displayNameProp);
            EditorGUILayout.PropertyField(classNameProp);
            EditorGUILayout.PropertyField(portraitProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SPUM Setup", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spumUnitIdProp);
            EditorGUILayout.PropertyField(spumPrefabProp);
            EditorGUILayout.PropertyField(spumScaleProp);
            EditorGUILayout.PropertyField(previewOffsetProp);
            EditorGUILayout.PropertyField(previewZoomProp);

            serializedObject.ApplyModifiedProperties();

            // Preview
            var pc = (PlayerCharacterDB)target;
            Texture2D preview = null;
            if (pc.portrait != null) preview = pc.portrait.texture;
            if (preview == null && pc.spumPrefab != null) preview = AssetPreview.GetAssetPreview(pc.spumPrefab);
            if (preview != null)
            {
                GUILayout.Space(10);
                float size = 128f * pc.previewZoom;
                Rect r = GUILayoutUtility.GetRect(size, size);
                r.position += pc.previewOffset;
                GUI.DrawTexture(r, preview, ScaleMode.ScaleToFit);
            }
        }
    }
}


