using Godot;
using System;

// NOTICE: If the DM response has a score of 0, dice do not need to be thrown and are thus irrelevant(for that condition)
public partial class DMCharacterResponse : CharacterInteraction
{
    // The DM response that it represents
    public JSONDMResponse dmResponse;

    // true if dice were thrown to check the victory/defeat status for this DM response
    public bool ThrownDice = false;

    // If the dice were thrown, true means victory, false is defeat
    public bool ThrownDiceSuccess = false;
}
