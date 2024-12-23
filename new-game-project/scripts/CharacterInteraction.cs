using Godot;
using System;

public partial class CharacterInteraction : GodotObject
{
    // The GameEntity(Character, Player or DM) that said the response.
    public GameEntity responderGameEntity;

    // The GameEntity(Character, Player or DM) that the response is directed at.
    public GameEntity respondeeGameEntity;
}
