using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Character data for Blazed Odyssey (No SPUM dependency)
/// Following the user's guidelines exactly
/// </summary>
[System.Serializable]
public class BlazedCharacterClass
{
	public string className;
	public string race;
	public string description;
	public string visualCues;
	public string primaryStat;
	public int health;
	public int mana;
	public int attack;
	public int defense;
	public int magic;
	public Color classColor;
}

/// <summary>
/// Static database of all character classes
/// </summary>
public static class BlazedCharacterDatabase
{
	public static List<BlazedCharacterClass> GetAllClasses()
	{
		return new List<BlazedCharacterClass>
		{
			// SPUM Character Classes - Updated for modern integration
			new BlazedCharacterClass
			{
				className = "Warrior",
				race = "Hero",
				description = "A mighty warrior skilled in melee combat and defense.",
				visualCues = "heavy armor; battle stance; sword and shield; warrior's pride; battle scars",
				primaryStat = "Attack",
				health = 120,
				mana = 40,
				attack = 16,
				defense = 12,
				magic = 4,
				classColor = new Color(0.80f, 0.20f, 0.20f, 1f)
			},
			new BlazedCharacterClass
			{
				className = "Mage",
				race = "Hero",
				description = "A powerful spellcaster with devastating magical abilities.",
				visualCues = "flowing robes; arcane staff; magical aura; focused concentration; spell effects",
				primaryStat = "Magic",
				health = 80,
				mana = 140,
				attack = 6,
				defense = 5,
				magic = 18,
				classColor = new Color(0.60f, 0.30f, 0.80f, 1f)
			},
			new BlazedCharacterClass
			{
				className = "Rogue",
				race = "Hero",
				description = "A stealthy assassin with deadly precision and agility.",
				visualCues = "dark leathers; dual daggers; stealth stance; shadowy presence; deadly focus",
				primaryStat = "Defense",
				health = 90,
				mana = 60,
				attack = 14,
				defense = 10,
				magic = 8,
				classColor = new Color(0.20f, 0.60f, 0.20f, 1f)
			}
		};
	}
	
	public static List<BlazedCharacterClass> GetClassesByRace(string race)
	{
		var allClasses = GetAllClasses();
		return allClasses.FindAll(c => c.race.Equals(race, System.StringComparison.OrdinalIgnoreCase));
	}
	
	public static List<string> GetAllRaces()
	{
		return new List<string> { "Hero" };
	}
}
