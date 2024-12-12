using Godot;
using System;

// TODO: fix magic numbers
// TODO: Inherit from Game?

public partial class GameWithAI : Node
{
    static void Main(string[] args)
    {
        // Character player = new Character("Hero", 2, 1, 2);
        // AICharacter ai1 = new AICharacter("AI Rogue", 1, 2, 1, "Sneaky");
        // AICharacter ai2 = new AICharacter("AI Warrior", 2, 1, 0, "Brave");
        // DungeonMaster dm = new DungeonMaster();

        // GD.Print($"Welcome, {player.Name}!");

        // for (int round = 1; round <= 3; round++)
        // {
        //     GD.Print($"\n--- Round {round} ---");

        //     GD.Print($"{player.Name}'s turn:");
        //     string task = "Solve a riddle";
        //     int challengeLevel = dm.AssignChallengeLevel(task);
        //     int diceRolls = player.RollDice();
        //     GD.Print($"Dice Rolls: {diceRolls} wins");
        //     dm.InterpretResult(player, diceRolls, challengeLevel);

        //     GD.Print("\nAI turns:");
        //     ai1.TakeTurn(dm);
        //     ai2.TakeTurn(dm);
        // }
    }
}
