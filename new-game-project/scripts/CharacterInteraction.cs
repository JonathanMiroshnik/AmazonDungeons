using Godot;
using System;

public partial class CharacterInteraction : GodotObject
{
    // The GameEntity(Character, Player or DM) that said the response.
    public GameEntity responderGameEntity;

    // The GameEntity(Character, Player or DM) that the response is directed at.
    public GameEntity respondeeGameEntity;

    // The response text.
    public string text;

    public CharacterInteraction(GameEntity responderGameEntity = null, GameEntity respondeeGameEntity = null, string text = "")
    {
        this.responderGameEntity = responderGameEntity;
        this.respondeeGameEntity = respondeeGameEntity;
        this.text = text;
    }
}
