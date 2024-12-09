using Godot;
using System;
using System.Threading.Tasks;

public partial class Dice : RigidBody3D
{
	[Export]
    public Node3D[] Sides { get; set; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int NUM_OF_SIDES = 6;
		// TODO: raise exception/error
		if (Sides.Length < NUM_OF_SIDES) return;

		//await testSide();
	}

	private async Task testSide() {
		await Task.Delay(3000);
		GD.Print(WhichSideUp().ToString());
	}

	public int WhichSideUp() {
		// Each side has a name which is an int, find the highest side(by y value) and return the number by its name
		float highestSide = Sides[0].GlobalPosition.Y;
		Node3D highestSideNode = Sides[0];

		foreach (Node3D side in Sides) {
			if (side.GlobalPosition.Y > highestSide) {
				highestSide = side.GlobalPosition.Y;
				highestSideNode = side;
			}
		}

		return highestSideNode.Name.ToString().ToInt(); // TODO: what if name is not int?
	}
}
