using System;

[Serializable]
public class HubCharacterData
{
	public string id;
	public string name;
	public string race;
	public string className;
	public bool isMale;
	public int level;

	public static HubCharacterData Create(string name, string race, string className, bool isMale, int level = 1)
	{
		return new HubCharacterData
		{
			id = Guid.NewGuid().ToString("N"),
			name = name,
			race = race,
			className = className,
			isMale = isMale,
			level = level
		};
	}
}


