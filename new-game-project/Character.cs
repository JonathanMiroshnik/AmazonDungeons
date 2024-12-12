using Godot;
using GodotPlugins.Game;
using System.Text.Json;

public partial class Character
{
    // Core Skills
    // TODO: should be up to 2
    public static int MAX_CORE_SKILL_LEVEL = 2;
    public int Strength { get; set; } = 0;
    public int Reflex { get; set; } = 0;
    public int Intelligence { get; set; } = 0;

    // Character Lore
    public string Name { get; set; }
    public string Personality { get; set; } = "";  // Contains personality/morality/history/goals

    // Character Base Aspects
    public int HealthPoints { get; set; } = 8;
    public int BaseDiceNumber { get; set; } = 3;

    public Character(string name, string personality = "", int strength = 0, int reflex = 0, int intelligence = 0)
    {
        Name = name;
        Personality = personality;

        Strength = strength;
        Reflex = reflex;
        Intelligence = intelligence;
    }

    public string JSONSerialize()
    {
        // returns a string of a JSON representing the Character object
        return JsonSerializer.Serialize(this);
    }
}
