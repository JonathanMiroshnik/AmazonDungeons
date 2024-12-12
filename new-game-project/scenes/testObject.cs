using Godot;
using System;

public partial class testObject : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Character character = new Character("hoo", "waga waga uuu");
		character.JSONSerialize();
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
