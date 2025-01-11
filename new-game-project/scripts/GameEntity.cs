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
	public string CreationPrompt { get; set; }
	public string Personality { get; set; }

	// Camera position pointed at this GameEntity in the game
	public Node3D worldSpacePosition;

	public GameEntity(string name, GameEntityType gameEntityType, string creationPrompt)
	{
		Name = name;
		GameEntityType = gameEntityType;
		CreationPrompt = creationPrompt;
	}
}
