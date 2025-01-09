using System.Text.Json;
using System.Collections.Generic;
using Godot;

public partial class Character : GameEntity
{
	// Core Skills
	// TODO: should be up to 2, NOTICE that this is the STARTING core skill level total
	public static int MAX_CORE_SKILL_LEVEL = 2;

	public Dictionary<CoreSkill, int> CoreSkills { get; set; }

	// Character Lore
	public string Personality { get; set; } = "";  // Contains personality/morality/history/goals
	public string ShortenedDescription { get; set; } = "";

	// Items
	public List<string> Items { get; set; }

	// Character Base Aspects
	public int HealthPoints { get; set; } = 8;
	public int BaseDiceNumber { get; set; } = 3;

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

	public string GetDescription()
	{
		string retStr = "";

		retStr += $"Name: {Name}\n Personality: {Personality}\nCore Skills: {GetCoreSkillsString()}\n";
		if (Items != null) {
			if (Items.Count > 0) {
				retStr += $"Items: {GetItemsString()}\n";
			}
		}
		
		retStr += "Health Points: {HealthPoints}\nBase Dice Number: {BaseDiceNumber}\n";
		
		return retStr;
	}

	private string GetCoreSkillsString() // Amazon Q figured it out instantly for me without me needing to even ask it
	{
		string coreSkillsString = "";
		foreach (var coreSkill in CoreSkills)
		{
			coreSkillsString += $"{coreSkill.Key}: {coreSkill.Value}\n";
		}
		return coreSkillsString;
	}

	// get items string
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

	public string GetConversationHistory() {
		if (conversation == null) return "";
		if (conversation.Count == 0) return "";

		string retStr = "";

		foreach (CharacterInteraction interaction in conversation) {
			retStr += $"{interaction.responderGameEntity} reponds to {interaction.respondeeGameEntity}: " + $"{interaction}\n";
		}

		return retStr;
	}
}
