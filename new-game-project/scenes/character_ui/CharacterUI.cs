using Godot;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

public partial class CharacterUI : Control
{
	[Export]
	public PackedScene fullResponseContainer;

	private VBoxContainer vContainer;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		if (fullResponseContainer == null) return; // FIXME: raise error
		vContainer = GetNode<VBoxContainer>("%VResponsesContainer");
		if (vContainer == null) return; // FIXME: raise error

		// TODO: test below, remove
		// await AddResponse("j-1024812-048i-9df9sjiofj23uiomdovu4ojgorjgojog4jo9g j4tj409jt39guov o4jto94jto34jgo99jeog4o9iji", GameManager.Instance.characters[0]);
		// await AddResponse("j-1024812-048i-9df9sjiofj23uiomdovu4ojgorjgojog4jo9g j4tj409jt39guov o4jto94jto34jgo99jeog4o9iji", GameManager.Instance.characters[0]);
	}

	public async Task AddResponse(string response)
	{
		await AddResponse(response, null);
	}

	public async Task AddResponse(string response, Character character)
	{
		FullResponseContainer container = fullResponseContainer.Instantiate<FullResponseContainer>();
		vContainer.AddChild(container);
		container.character = character;
		await container.ShowResponse(response);
	}

	public void ClearResponses()
	{
		if (vContainer == null) return;
		if (vContainer.GetChildren() == null) return;
		if (vContainer.GetChildren().Count == 0) return;

		foreach (Node child in vContainer.GetChildren())
		{
			child.QueueFree();
		}
	}
}
