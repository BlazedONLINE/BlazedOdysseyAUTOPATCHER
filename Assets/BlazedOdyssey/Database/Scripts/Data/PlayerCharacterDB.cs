using UnityEngine;

namespace BlazedOdyssey.Database.Data
{
    public class PlayerCharacterDB : ScriptableObject
    {
        public string id;               // e.g., CHAR_0001
        public string displayName;
        public string className;
        public Sprite portrait;

        [Header("SPUM Integration")]
        public string spumUnitId;
        public GameObject spumPrefab;
        public float spumScale = 1f;
        public Vector2 previewOffset = Vector2.zero;
        public float previewZoom = 1f;
    }
}


