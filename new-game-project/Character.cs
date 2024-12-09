using Godot;
using System;

// TODO: fix magic numbers

public partial class Character : Node
{
    public int Strength { get; set; }
    public int Reflex { get; set; }
    public int Intelligence { get; set; }
    public int BaseDice { get; set; } = 3;

    public Character(string name, int strength, int reflex, int intelligence)
    {
        Name = name;
        Strength = strength;
        Reflex = reflex;
        Intelligence = intelligence;
    }

    public int RollDice(int bonusDice = 0)
    {
        Random rand = new Random();
        int totalDice = BaseDice + Strength + Reflex + Intelligence + bonusDice;
        int wins = 0;

        for (int i = 0; i < totalDice; i++)
        {
            int roll = rand.Next(1, 7); // Simulates a D6 roll
            if (roll >= 4)
                wins++;
        }

        return wins;
    }
}
