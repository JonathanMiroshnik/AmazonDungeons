using Godot;
using System;

public enum GameEntityType
{
    Player,
    AI,
    DungeonMaster
}

public partial class GameEntity : GodotObject
{
    public GameEntityType GameEntityType { get; set; }
    public string Name { get; set; }

    // Camera position pointed at this GameEntity in the game
    public Node3D worldSpacePosition;

    public GameEntity(string name, GameEntityType gameEntityType)
    {
        Name = name;
        GameEntityType = gameEntityType;
    }
}
