using System;
using UnityEngine;

namespace BlazedOdyssey.Database.Data
{
    public class AttributeDB : ScriptableObject { public string id; public string displayName; public Sprite icon; }
    public class CurrencyDB : ScriptableObject { public string id; public string displayName; public Sprite icon; public int baseValue; }
    public class DamageElementDB : ScriptableObject { public string id; public string displayName; public Color color; }
    public class ItemCraftDB : ScriptableObject { public string id; public string resultItemId; public int resultAmount = 1; }
    public class ArmorTypeDB : ScriptableObject { public string id; public string displayName; }
    public class WeaponTypeDB : ScriptableObject { public string id; public string displayName; }
    public class AmmoTypeDB : ScriptableObject { public string id; public string displayName; }
    public class StatusEffectDB : ScriptableObject { public string id; public string displayName; [TextArea] public string description; }
    public class SkillDB : ScriptableObject { public string id; public string displayName; [TextArea] public string description; }
    public class GuildSkillDB : ScriptableObject { public string id; public string displayName; }
    public class GuildIconDB : ScriptableObject { public string id; public Sprite icon; }
    public class MonsterCharacterDB : ScriptableObject { public string id; public string displayName; public Sprite portrait; }
    public class HarvestableDB : ScriptableObject { public string id; public string displayName; }
    public class MapInfoDB : ScriptableObject { public string id; public string displayName; }
    public class QuestDB : ScriptableObject { public string id; public string displayName; }
    public class FactionDB : ScriptableObject { public string id; public string displayName; }
}


