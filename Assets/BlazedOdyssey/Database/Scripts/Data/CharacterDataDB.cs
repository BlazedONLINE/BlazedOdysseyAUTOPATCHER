using System;
using UnityEngine;

namespace BlazedOdyssey.Database.Data
{
	[Serializable]
	public class CharacterDataDB
	{
		public string id; // CHAR_0001
		public string displayName;
		public string className;
		public Sprite portrait;
		public string spumUnitId; // link to SPUM registry id for previews
	}
}


