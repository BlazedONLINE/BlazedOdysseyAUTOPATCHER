using UnityEngine;
using System.Collections.Generic;
using BlazedOdyssey.Database.Data;

namespace BlazedOdyssey.Database
{
	[CreateAssetMenu(menuName = "BlazedOdyssey/Database", fileName = "GameDatabase")]
	public class DatabaseAsset : ScriptableObject
	{
		// Core categories
		public List<ItemData> items = new List<ItemData>();
		public List<PlayerCharacterDB> characters = new List<PlayerCharacterDB>();
		public List<AttributeDB> attributes = new List<AttributeDB>();
		public List<CurrencyDB> currencies = new List<CurrencyDB>();
		public List<DamageElementDB> damageElements = new List<DamageElementDB>();
		public List<ItemCraftDB> itemCraftFormulas = new List<ItemCraftDB>();
		public List<ArmorTypeDB> armorTypes = new List<ArmorTypeDB>();
		public List<WeaponTypeDB> weaponTypes = new List<WeaponTypeDB>();
		public List<AmmoTypeDB> ammoTypes = new List<AmmoTypeDB>();
		public List<StatusEffectDB> statusEffects = new List<StatusEffectDB>();
		public List<SkillDB> skills = new List<SkillDB>();
		public List<GuildSkillDB> guildSkills = new List<GuildSkillDB>();
		public List<GuildIconDB> guildIcons = new List<GuildIconDB>();
		public List<MonsterCharacterDB> monsterCharacters = new List<MonsterCharacterDB>();
		public List<HarvestableDB> harvestables = new List<HarvestableDB>();
		public List<MapInfoDB> mapInfos = new List<MapInfoDB>();
		public List<QuestDB> quests = new List<QuestDB>();
		public List<FactionDB> factions = new List<FactionDB>();

		public ItemData GetItemById(string id) => items.Find(it => it.id == id);
		public PlayerCharacterDB GetCharacterById(string id) => characters.Find(c => c.id == id);
	}
}


