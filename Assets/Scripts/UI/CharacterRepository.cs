using System.Collections.Generic;
using UnityEngine;

public static class CharacterRepository
{
	private const string KeyCount = "CharRepo_Count";
	private const string KeyPrefix = "CharRepo_";

	public static List<HubCharacterData> LoadAll()
	{
		int count = PlayerPrefs.GetInt(KeyCount, 0);
		var list = new List<HubCharacterData>(count);
		for (int i = 0; i < count; i++)
		{
			string json = PlayerPrefs.GetString(KeyPrefix + i, "");
			if (!string.IsNullOrEmpty(json))
				list.Add(JsonUtility.FromJson<HubCharacterData>(json));
		}
		return list;
	}

	public static void SaveAll(List<HubCharacterData> characters)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			string json = JsonUtility.ToJson(characters[i]);
			PlayerPrefs.SetString(KeyPrefix + i, json);
		}
		PlayerPrefs.SetInt(KeyCount, characters.Count);
		PlayerPrefs.Save();
	}
}


