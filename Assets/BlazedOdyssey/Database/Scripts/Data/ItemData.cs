using System;
using UnityEngine;

namespace BlazedOdyssey.Database.Data
{
	[Serializable]
	public class ItemData
	{
		public string id; // unique id string (e.g., ITEM_0001)
		public string displayName;
		[TextArea] public string description;
		public Sprite icon;
		public int maxStack = 99;
		public Rarity rarity = Rarity.Common;
		public int baseValue = 0;
	}

	public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
}


