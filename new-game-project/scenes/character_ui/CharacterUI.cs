using Godot;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

public partial class CharacterUI : Control
{
	[Export]
	public PackedScene fullResponseContainer;

	// Current conversation between DM and character that is being shown in the UI // TODO: generalize it beyong DM-character interactions
	public DMCharacterResponse curDMResponse;

	private VBoxContainer vContainer;
	private MarginContainer diceContainer;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		if (fullResponseContainer == null) return; // FIXME: raise error
		vContainer = GetNodeOrNull<VBoxContainer>("%VResponsesContainer");
		if (vContainer == null) return; // FIXME: raise error
		diceContainer = GetNodeOrNull<MarginContainer>("%DiceContainer");
		if (diceContainer == null) return; // FIXME: raise error
	}

	public async Task AddResponse(string response)
	{
		await AddResponse(response, null);
	}

	public async Task AddResponse(string response, GameEntity gameEntity)
	{
		FullResponseContainer container = fullResponseContainer.Instantiate<FullResponseContainer>();
		vContainer.AddChild(container);
		container.gameEntity = gameEntity;

		container.ReplyToResponse += ActivatedReply;

		await container.ShowResponse(response);
		await Task.Delay(1000);
		
	}

	// TODO: delete this? tried to put the conversation in a fullresponsecontainer but is a part of it
	// public async Task EngageDMConversation(DMCharacterResponse dmReponse)
	// {
	// 	FullResponseContainer container = fullResponseContainer.Instantiate<FullResponseContainer>();
	// 	vContainer.AddChild(container);
	// 	container.gameEntity = dmReponse.respondeeGameEntity;

	// 	container.ReplyToResponse += ActivatedReply;

	// 	await container.ShowResponse(response);
	// 	await Task.Delay(1000);
		
	// }

	public void ActivatedReply(string response)
	{
		GD.Print("ReplyToResponse " + response);
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

	public async Task DoDMConversation(DMCharacterResponse dmCharResponse) {
		if (dmCharResponse == null) {
			GD.Print("Error: a DM-character response is null");
			return;
		}

		// Clear previous character responses
		ClearResponses();

		// Asking the DM for a first response
		JSONDMResponse result = await GameManager.Instance.game.DM_response((Character) dmCharResponse.respondeeGameEntity); // FIXME: bad downcasting thing
		dmCharResponse.dmResponse = result;

		await AddResponse(dmCharResponse.dmResponse.text, dmCharResponse.respondeeGameEntity); // FIXME: downcasting

		// TODO: show dice score needed to win in separate label?

		// If the DM response requires a roll of the dice, it is performed
		if (dmCharResponse.dmResponse.score > 0) {
			diceContainer.Visible = true;
		}

		// TODO: Response from character to DM
		// TODO: Possible dice roll
		// TODO: DM Responds to dice roll
	}

	public async void _on_dice_button_pressed() {
		// TODO: null checks?
		// TODO: how do we make sure that the curDMResponse is really the current one and not a previous one?

		diceContainer.Visible = false;
		Visible = false;
		curDMResponse.ThrownDiceSuccess = await GameManager.Instance.game.PlayDice(curDMResponse.dmResponse.score);
		curDMResponse.ThrownDice = true;
		
		// Move back to the Character
		await Task.Delay(1000);
		await GameManager.Instance.game.cameraMover.MoveCameraByNode3D(curDMResponse.respondeeGameEntity.worldSpacePosition, 
																		GameManager.TIME_TO_MOVE_CAMERA_POSITIONS);
		Visible = true;
	}

	public void _on_next_button_pressed() {
		if (curDMResponse == null) return;
		
		if (curDMResponse.dmResponse.score > 0) {
			if (!curDMResponse.ThrownDice) return;
		}

		GameManager.Instance.game.NextAction();
	}
}
