using Godot;
using System;

// TODO: fix magic numbers
// TODO: name inherited from Node

public partial class DungeonMaster : Node
{
    public DungeonMaster() {
        Name = "Game Master (LLM)";
    }
    
    public int AssignChallengeLevel(string taskDescription)
    {
        // This can be expanded to assign CLs based on context.
        GD.Print($"Task: {taskDescription}");
        GD.Print("Assigning Challenge Level...");
        return new Random().Next(1, 8); // Example: Random CL between 1 and 8
    }

    public void InterpretResult(Character character, int diceRolls, int challengeLevel)
    {
        if (diceRolls >= challengeLevel)
        {
            GD.Print($"{character.Name} succeeds in the task!");
        }
        else
        {
            GD.Print($"{character.Name} fails the task. Narrative failure ensues.");
        }
    }
}

