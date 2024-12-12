using Godot;
using System;

public partial class AICharacter : Character
{
    public AICharacter(string name, string personality, int strength, int reflex, int intelligence)
        : base(name, personality, strength, reflex, intelligence)
    {
    }
    
    // public string Personality { get; set; }

    // public AICharacter(string name, int strength, int reflex, int intelligence, string personality)
    //     : base(name, personality, strength, reflex, intelligence)
    // {
    //     Personality = personality;
    // }

    // public void TakeTurn(DungeonMaster dm)
    // {
    //     string task = "Perform a daring leap";
    //     int challengeLevel = dm.AssignChallengeLevel(task);

    //     GD.Print($"{Name} ({Personality}) attempts to {task}. Challenge Level: {challengeLevel}");
    //     int diceRolls = RollDice();
    //     GD.Print($"{Name} rolls: {diceRolls} wins");
    //     dm.InterpretResult(this, diceRolls, challengeLevel);
    // }
}
