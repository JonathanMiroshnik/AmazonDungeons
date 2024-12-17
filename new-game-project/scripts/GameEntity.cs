using Godot;
using System;

public enum GameEntityType
{
    Player,
    AI,
    DungeonMaster
}

public partial class GameEntity : Node
{
    public GameEntityType GameEntityType;

    public GameEntity(GameEntityType gameEntityType)
    {
        GameEntityType =  gameEntityType;
    }
}
