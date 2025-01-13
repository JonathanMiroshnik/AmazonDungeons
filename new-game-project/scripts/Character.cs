using System.Text.Json;
using System.Collections.Generic;

public partial class Character : GameEntity
{
	// Core Skills
	// TODO: should be up to 2, NOTICE that this is the STARTING core skill level total
	public static int MAX_CORE_SKILL_LEVEL = 2;

	public Dictionary<CoreSkill, int> CoreSkills { get; set; }

	// Character Lore
	public string ShortenedDescription { get; set; } = "";

	// Items
	public List<string> Items { get; set; }

	// Character Base Aspects
	public int HealthPoints { get; set; } = 8;

	// Includes all the conversations between the character and the others(specifically the DM) in chronological order
	public List<CharacterInteraction> conversation = new List<CharacterInteraction>();

	// constructor that takes into consideration the constructor of GameEntity
	public Character(string name, string personality = "", string shortenedDescription = "", string creationPrompt = "", int strength = 0, 
					int reflex = 0, int intelligence = 0, GameEntityType gameEntityType = GameEntityType.AI) : base(name, gameEntityType, creationPrompt)
	{
		Name = name;
		Personality = personality;
		ShortenedDescription = shortenedDescription;

		CoreSkills = new Dictionary<CoreSkill, int>
		{
			{ CoreSkill.Strength, strength },
			{ CoreSkill.Reflex, reflex },
			{ CoreSkill.Intelligence, intelligence }
		};
	}

	public Dictionary<CoreSkill, int> SetCoreSkills(int strength = 0, int reflex = 0, int intelligence = 0)
	{
		CoreSkills = new Dictionary<CoreSkill, int>
		{
			{ CoreSkill.Strength, strength },
			{ CoreSkill.Reflex, reflex },
			{ CoreSkill.Intelligence, intelligence }
		};

		return CoreSkills;
	}

	// Gets a full description of the Character
	public string GetDescription()
	{
		string retStr = "";

		retStr += $"Name: {Name}\n Personality: {Personality}\nCore Skills: {GetCoreSkillsString()}\n";
		if (Items != null) {
			if (Items.Count > 0) {
				retStr += $"Items: {GetItemsString()}\n";
			}
		}
		
		retStr += $"Health Points: {HealthPoints}\n";
		
		return retStr;
	}

	// Gets a description of the Core Skills of the Character
	private string GetCoreSkillsString() // Amazon Q figured it out instantly for me without me needing to even ask it
	{
		string coreSkillsString = "";
		foreach (var coreSkill in CoreSkills)
		{
			coreSkillsString += $"{coreSkill.Key}: {coreSkill.Value}\n";
		}
		return coreSkillsString;
	}

	// Gets the Character's items
	private string GetItemsString()
	{
		string itemsString = "";
		foreach (string item in Items)
		{
			itemsString += $"{item}\n";
		}
		return itemsString;
	}

	// TODO: notice that some of the variables should not be serialized in a JSON that is given to the LLM, or maybe not use JSON like this anyways?
	public string JSONSerialize()
	{
		// returns a string of a JSON representing the Character object
		return JsonSerializer.Serialize(this);
	}

	// Gets the conversation history between the character and the others
	public string GetConversationHistory() {
		if (conversation == null) return "";
		if (conversation.Count == 0) return "";

		string retStr = "";

		foreach (CharacterInteraction interaction in conversation) {
			retStr += $"{interaction.responderGameEntity} reponds to {interaction.respondeeGameEntity}: " + $"{interaction}\n";
		}

		return retStr;
	}

	// Gets the dice bonus for the core skills of the Character
	public int getBonus(bool strength, bool reflex, bool intelligence) {
		int ret = 0;

		if (strength) ret += CoreSkills[CoreSkill.Strength];
		if (reflex) ret += CoreSkills[CoreSkill.Reflex];
		if (intelligence) ret += CoreSkills[CoreSkill.Intelligence];

		return ret;
	}
}
