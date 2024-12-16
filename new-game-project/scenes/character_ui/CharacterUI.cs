using Godot;
using System;
using System.Net.Sockets;

public partial class CharacterUI : Control
{
	[Export]
	public PackedScene fullResponseContainer;

	private VBoxContainer vContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (fullResponseContainer == null) return; // FIXME: raise error
		vContainer = GetNode<VBoxContainer>("%VResponsesContainer");
		if (vContainer == null) return; // FIXME: raise error

		// TODO: test below, remove
		AddResponse("j-1024812-048i-9df9sjiofj23uiomdovu4ojgorjgojog4jo9g j4tj409jt39guov o4jto94jto34jgo99jeog4o9iji", GameManager.Instance.characters[0]);
		AddResponse("j-1024812-048i-9df9sjiofj23uiomdovu4ojgorjgojog4jo9g j4tj409jt39guov o4jto94jto34jgo99jeog4o9iji", GameManager.Instance.characters[0]);
	}

	public void AddResponse(string response)
	{
		AddResponse(response, null);
	}

	public void AddResponse(string response, Character character)
	{
		FullResponseContainer container = fullResponseContainer.Instantiate<FullResponseContainer>();
		vContainer.AddChild(container);
		container.character = character;
		container.ShowResponse(response);
	}
}
