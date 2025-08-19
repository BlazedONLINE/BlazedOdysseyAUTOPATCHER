using UnityEngine;

[CreateAssetMenu(fileName = "SpumUnitRegistry", menuName = "Blazed Odyssey/SPUM Unit Registry")]
public class SpumUnitRegistry : ScriptableObject
{
	[System.Serializable]
	public class Entry
	{
		[Header("Identity")]
		public string className;
		public string race; // optional; leave empty for wildcard
		public Gender gender = Gender.Any; // optional

		[Header("Prefab Reference")]
		[Tooltip("SPUM prefab file name under Assets/SPUM/Resources/Units (without .prefab)")]
		public string prefabName;

		[Header("Presentation")]
		[TextArea(2,4)] public string description;
		public string primaryStat;
		public Color classColor = Color.white;

		[Header("Stats")]
		public int health;
		public int mana;
		public int attack;
		public int defense;
		public int magic;

		[Header("Role Preset")]
		public Role role = Role.None;

		[Header("Protection")]
		[Tooltip("When enabled, Scan won't overwrite or modify this entry")] public bool locked = false;
	}

	public enum Gender { Any, Male, Female }
	public enum Role { None, Tank, Healer, DPS }

	public Entry[] entries;

	public bool TryGetPrefab(string className, out GameObject prefab)
	{
		prefab = null;
		return TryGetPrefabByClassRaceGender(className, null, null, out prefab);
	}

	public bool TryGetPrefabByClassRaceGender(string className, string race, bool? isMale, out GameObject prefab)
	{
		prefab = null;
		if (string.IsNullOrWhiteSpace(className) || entries == null) return false;

		string normClass = className.Trim();
		string normRace = string.IsNullOrWhiteSpace(race) ? null : race.Trim();
		Gender wantGender = Gender.Any;
		if (isMale.HasValue) wantGender = isMale.Value ? Gender.Male : Gender.Female;
		
		// Debug logging for gender-specific lookups
		Debug.Log($"🔍 SPUM Registry Lookup: Class={normClass}, Race={normRace}, Gender={wantGender}");

		Entry best = null; int bestScore = -1;
		for (int i = 0; i < entries.Length; i++)
		{
			var e = entries[i];
			if (e == null || string.IsNullOrWhiteSpace(e.className) || string.IsNullOrWhiteSpace(e.prefabName)) continue;
			if (!string.Equals(e.className, normClass, System.StringComparison.OrdinalIgnoreCase)) continue;

			int score = 0;
			// race match
			bool raceSpecified = !string.IsNullOrWhiteSpace(e.race);
			if (raceSpecified)
			{
				if (normRace != null && string.Equals(e.race, normRace, System.StringComparison.OrdinalIgnoreCase)) score += 2;
				else continue; // specified race but doesn't match desired
			}
			// gender match
			bool genderSpecified = e.gender != Gender.Any;
			if (genderSpecified)
			{
				if (wantGender != Gender.Any && e.gender == wantGender) score += 1;
				else continue; // specified gender but doesn't match desired
			}

			if (score > bestScore)
			{
				best = e; bestScore = score;
			}
		}

		if (best == null) 
		{
			Debug.LogWarning($"⚠️ No SPUM registry entry found for Class={normClass}, Race={normRace}, Gender={wantGender}");
			return false;
		}
		
		Debug.Log($"✅ Found SPUM registry entry: {best.prefabName} (Score: {bestScore})");
		prefab = Resources.Load<GameObject>("Units/" + best.prefabName);
		
		if (prefab == null)
			Debug.LogError($"❌ SPUM prefab not found at Resources/Units/{best.prefabName}");
		else
			Debug.Log($"✅ Successfully loaded SPUM prefab: {best.prefabName}");
			
		return prefab != null;
	}
}


