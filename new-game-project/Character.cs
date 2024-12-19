using System.Text.Json;
using System.Collections.Generic;

public partial class Character : GameEntity
{
    // Core Skills
    // TODO: should be up to 2, NOTICE that this is the STARTING core skill level total
    public static int MAX_CORE_SKILL_LEVEL = 2;

    public Dictionary<CoreSkill, int> CoreSkills { get; set; }

    // Character Lore
    public string Name { get; set; }
    public string Personality { get; set; } = "";  // Contains personality/morality/history/goals
    public string ShortenedDescription { get; set; } = "";

    // Items
    public List<string> Items { get; set; }

    // Character Base Aspects
    public int HealthPoints { get; set; } = 8;
    public int BaseDiceNumber { get; set; } = 3;

    // constructor that takes into consideration the constructor of GameEntity
    public Character(string name, string personality = "", string shortenedDescription = "", int strength = 0, 
                    int reflex = 0, int intelligence = 0, GameEntityType gameEntityType = GameEntityType.AI) : base(gameEntityType)
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
        return $"Name: {Name}\nPersonality: {Personality}\nCore Skills: {CoreSkills}\nItems: {Items}\nHealth Points: {HealthPoints}\nBase Dice Number: {BaseDiceNumber}";
    }

    // TODO: notice that some of the variables should not be serialized in a JSON that is given to the LLM, or maybe not use JSON like this anyways?
    public string JSONSerialize()
    {
        // returns a string of a JSON representing the Character object
        return JsonSerializer.Serialize(this);
    }
}
