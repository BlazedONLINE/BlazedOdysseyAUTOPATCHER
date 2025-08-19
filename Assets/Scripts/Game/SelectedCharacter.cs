using UnityEngine;

public static class SelectedCharacter
{
	public static string Race { get; set; }
	public static string ClassName { get; set; }
	public static bool IsMale { get; set; }
	public static string CharacterName { get; set; }
	public static string SpumPrefabName { get; set; } // Resources/Units/<name>

	public static string GetIdleResourcePath()
	{
		// New path: use Heroes pack structure under Resources/Characters/<ClassName>/<ClassName>_idle (if exists)
		return $"Characters/{ClassName}/{ClassName}_idle";
	}

	public static string GetWalkFramesFolder()
	{
		// New path: Resources/Characters/<ClassName>/<ClassName>_walk (sheet to be sliced at runtime)
		return $"Characters/{ClassName}/{ClassName}_walk";
	}
}


