using Godot;
using System;

public partial class Dice : Node
{
	[Export]
    public Node3D[] Sides { get; set; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int NUM_OF_SIDES = 6;
		// TODO: raise exception/error
		if (Sides.Length < NUM_OF_SIDES) return;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public int WhichSideUp() {
		// Each side has a name which is an int, find the highest side(by y value) and return the number by its name
		int highestSide = -1;
		Node3D highestSideNode = null;
		foreach (Node3D side in Sides) {
			if (side.Position.Y > highestSide) {
				highestSide = (int)side.Position.Y;
				highestSideNode = side;
			}
		}

		return highestSideNode.Name.ToString().ToInt(); // TODO: what if name is not int?
	}
}
