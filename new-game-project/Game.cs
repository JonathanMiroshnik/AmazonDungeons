using Godot;
using System;

// TODO: fix magic numbers

public partial class Game : Node
{
    static void Main(string[] args)
    {
        Character player = new Character("Hero", 2, 1, 2);
        DungeonMaster dm = new DungeonMaster();

        GD.Print($"Welcome, {player.Name}!");
        
        // Example Task
        string task = "Climb a slippery wall";
        int challengeLevel = dm.AssignChallengeLevel(task);

        GD.Print($"{player.Name} attempts to {task}. Challenge Level: {challengeLevel}");
        int diceRolls = player.RollDice();
        GD.Print($"Dice Rolls: {diceRolls} wins");
        
        dm.InterpretResult(player, diceRolls, challengeLevel);
    }
}

