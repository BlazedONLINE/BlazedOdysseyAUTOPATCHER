#if DATABASE_UI_PRO_STUBS
using System;
using UnityEngine;

// Runtime ScriptableObject used by the Database UI window. File name matches
// class name to satisfy Unity's type resolution when assigning/creating assets.

namespace MultiplayerARPG
{
	[CreateAssetMenu(menuName = "BlazedOdyssey/TempGameDatabase", fileName = "TempGameDatabase")]
	public class GameDatabase : ScriptableObject
	{
		public Attribute[] attributes = Array.Empty<Attribute>();
		public Currency[] currencies = Array.Empty<Currency>();
		public DamageElement[] damageElements = Array.Empty<DamageElement>();
		public BaseItem[] items = Array.Empty<BaseItem>();
		public ItemCraftFormula[] itemCraftFormulas = Array.Empty<ItemCraftFormula>();
		public ArmorType[] armorTypes = Array.Empty<ArmorType>();
		public WeaponType[] weaponTypes = Array.Empty<WeaponType>();
		public AmmoType[] ammoTypes = Array.Empty<AmmoType>();
		public StatusEffect[] statusEffects = Array.Empty<StatusEffect>();
		public BaseSkill[] skills = Array.Empty<BaseSkill>();
		public GuildSkill[] guildSkills = Array.Empty<GuildSkill>();
		public GuildIcon[] guildIcons = Array.Empty<GuildIcon>();
		public PlayerCharacter[] playerCharacters = Array.Empty<PlayerCharacter>();
		public MonsterCharacter[] monsterCharacters = Array.Empty<MonsterCharacter>();
		public Harvestable[] harvestables = Array.Empty<Harvestable>();
		public BaseMapInfo[] mapInfos = Array.Empty<BaseMapInfo>();
		public Quest[] quests = Array.Empty<Quest>();
		public Faction[] factions = Array.Empty<Faction>();

		public void LoadReferredData() { }
	}

	public static class EditorGlobalData
	{
		public static GameDatabase WorkingDatabase;
	}
}


#endif
