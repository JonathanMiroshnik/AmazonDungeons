using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Allows the smooth movement of a camera throughout the game world between designated Node3Ds
/// </summary>
public partial class CameraMover : Camera3D
{
	// Positions that the Camera can move to
	[Export] public Node3D[] Positions { get; set; }

	private Transform3D trueTransform;
	private float CURSOR_MOVEMENT_MAX = 0.1f;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		trueTransform = Transform;

		// Examples of movement over the indeces of positions in the list
		// await MoveCameraByIndex(0, 2);
		// await MoveCameraByIndex(1, 2);
		// await MoveCameraByIndex(2, 2);

		// Example of movement with the name of the position in the lsit
		// await MoveCameraByString("OriginalCameraPos", 4);
	}

	public override void _Process(double delta)
	{
		// We add some camera movement by the movement of the cursor on the screen
		Vector2 ViewportSize = GetViewport().GetVisibleRect().Size;
		Vector2 MousePos = GetViewport().GetMousePosition();
		float yToMove = MousePos.X / ViewportSize.X - 0.5f;
		float xToMove = MousePos.Y / ViewportSize.Y - 0.5f;

		Transform3D slightTransform = trueTransform;
		slightTransform.Basis *= new Basis(new Vector3(1, 0, 0), -xToMove * CURSOR_MOVEMENT_MAX) * //FIXME: might not work when moving between transforms!
								 new Basis(new Vector3(0, 1, 0), -yToMove * CURSOR_MOVEMENT_MAX);
		Transform = slightTransform;
	}

	// SOURCE: https://easings.net/#easeInOutQuad // Amazon Q
	private float easeInOutQuad(float x)
	{
		return x < 0.5 ? 2 * x * x : 1 - (float)Math.Pow(-2 * x + 2, 2) / 2;
	}

	// Move the Camera with an index in the Position 
	public async Task MoveCameraByIndex(int index, float duration)
	{
		if (Positions == null) return;
		if (Positions.Length <= index) return;

		await MoveCamera(this, Positions[index], duration);
	}

	// Move the Camera with the name of the Position in the Positions list
	public async Task MoveCameraByString(string name, float duration)
	{
		if (Positions == null) return;

		// Go over positions and find the node that is of the name // Amazon Q
		var node = Array.Find(Positions, x => x.Name == name);
		if (node == null) return;
		await MoveCamera(this, node, duration);
	}

	// Move the Camera with the name of the Position if that node exists in world space with that Unique name
	public async Task MoveCameraByWorldString(string name, float duration)
	{
		Node3D relevantNode = GetNodeOrNull<Node3D>("%" + name);
		if (relevantNode == null) {
			GD.Print("Node not found in world space: " + name);
			return;
		}

		await MoveCamera(this, relevantNode, duration);
	}

	// Move the Camera to the node in duration seconds
	public async Task MoveCameraByNode3D(Node3D node, float duration)
	{
		if (node == null) return;
		await MoveCamera(this, node, duration);
	}

	// Async function that takes Camera3D and Node3D and moves the Camera from its current position to the Node3D position with the easeInOutQuad function instead of lerp // Amazon Q
	private async Task<bool> MoveCamera(Camera3D camera, Node3D node, float duration = 1f)
	{
		var start = camera.GlobalPosition;
		var end = node.GlobalPosition;
		if (start == end) return false;

		// NOTICE: Using Rotation or GlobalRotation does not work in this instance, the Slerp function inevitably breaks down with an issue of normalization, Quaternions do work though.
		var startRot = camera.Quaternion;
		var endRot = node.Quaternion;

		trueTransform = node.Transform;

		var time = 0f;
		while (time < duration)
		{
			time += (float)GetProcessDeltaTime();
			var t = time / duration;
			var ease = easeInOutQuad(t);

			camera.GlobalPosition = start.Lerp(end, ease);
			camera.Quaternion = startRot.Slerp(endRot, ease);

			await ToSignal(GetTree(), "process_frame");
		}

		return true;
	}
}
