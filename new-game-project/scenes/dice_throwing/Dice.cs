using System;
using Godot;

/// <summary>
/// Represents a die and allows knowing which side of the die is up
/// </summary>
public partial class Dice : RigidBody3D
{
	// Sides of the die
	[Export]
    public Node3D[] Sides { get; set; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int NUM_OF_SIDES = 6;
		// TODO: raise exception/error
		if (Sides.Length < NUM_OF_SIDES) return;
	}

	// Returns the side of the die that is up
	public int WhichSideUp() {
		if (Sides.Length == 0) return -1;

		// Each side has a name which is an int, find the highest side(by y value) and return the number by its name
		float highestSide = float.MinValue;
		Node3D highestSideNode = Sides[0];

		foreach (Node3D side in Sides) {
			if (side == null) return -1;
			if (side.GlobalPosition.Y > highestSide) {
				highestSide = side.GlobalPosition.Y;
				highestSideNode = side;
			}
		}

		return highestSideNode.Name.ToString().ToInt(); // TODO: what if name is not int?
	}

	public void PaintDice(Color color) {
		Node3D DiceMesh = GetNodeOrNull<Node3D>("%DiceMesh");
		if (DiceMesh == null) return;
		
		MeshInstance3D mesh = DiceMesh.GetNode<MeshInstance3D>("Cube");
		if (mesh == null) return;

		StandardMaterial3D material = (StandardMaterial3D)mesh.GetActiveMaterial(0);//GetSurfaceOverrideMaterial(0);
		mesh.SetSurfaceOverrideMaterial(0, (StandardMaterial3D) material.Duplicate());
		material = (StandardMaterial3D)mesh.GetActiveMaterial(0);//GetSurfaceOverrideMaterial(0);
		if (material == null) return;

		material.AlbedoColor = color;
	}
}
