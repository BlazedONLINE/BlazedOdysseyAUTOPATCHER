#if DATABASE_UI_PRO_STUBS
using UnityEngine;

// Runtime types for the compatibility layer must live outside an Editor folder
// so Unity can create assets from them.

namespace MultiplayerARPG
{
	public class BaseGameData : ScriptableObject
	{
		public string Id;
		public string Category;
		public UnityEngine.Object Icon; // assign a Sprite for list icons
	}

	// Placeholder data classes (all inherit BaseGameData)
	public class Attribute : BaseGameData { }
	public class Currency : BaseGameData { }
	public class DamageElement : BaseGameData { }
	public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

	public class BaseItem : BaseGameData
	{
		public string DisplayName;
		[TextArea] public string Description;
		public Rarity ItemRarity = Rarity.Common;
		public int MaxStack = 99;
		public int BaseValue = 0;
	}
	public class ItemCraftFormula : BaseGameData { }
	public class ArmorType : BaseGameData { }
	public class WeaponType : BaseGameData { }
	public class AmmoType : BaseGameData { }
	public class StatusEffect : BaseGameData { }
	public class BaseSkill : BaseGameData { }
	public class GuildSkill : BaseGameData { }
	public class GuildIcon : BaseGameData { }
	public class PlayerCharacter : BaseGameData
	{
		public string DisplayName;
		public string SpumUnitId;
		public Sprite Portrait; // you can also set Icon with a Sprite for list icon
	}
	public class MonsterCharacter : BaseGameData { }
	public class Harvestable : BaseGameData { }
	public class BaseMapInfo : BaseGameData { }
	public class Quest : BaseGameData { }
	public class Faction : BaseGameData { }
}


#endif
