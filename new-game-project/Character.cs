using System.Text.Json;
using System.Collections.Generic;

public partial class Character : GameEntity
{
    // Core Skills
    // TODO: should be up to 2
    public static int MAX_CORE_SKILL_LEVEL = 2;

    public Dictionary<CoreSkill, int> CoreSkills { get; set; }

    // Character Lore
    public string Name { get; set; }
    public string Personality { get; set; } = "";  // Contains personality/morality/history/goals

    // Items
    public List<string> Items { get; set; }

    // Character Base Aspects
    public int HealthPoints { get; set; } = 8;
    public int BaseDiceNumber { get; set; } = 3;

    // constructor that takes into consideration the constructor of GameEntity
    public Character(string name, string personality = "", int strength = 0, 
                    int reflex = 0, int intelligence = 0, GameEntityType gameEntityType = GameEntityType.AI) : base(gameEntityType)
    {
        Name = name;
        Personality = personality;

        CoreSkills = new Dictionary<CoreSkill, int>
        {
            { CoreSkill.Strength, strength },
            { CoreSkill.Reflex, reflex },
            { CoreSkill.Intelligence, intelligence }
        };
    }

    // TODO: notice that some of the variables should not be serialized in a JSON that is given to the LLM, or maybe not use JSON like this anyways?
    public string JSONSerialize()
    {
        // returns a string of a JSON representing the Character object
        return JsonSerializer.Serialize(this);
    }
}
