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
	private MarginContainer nextContainer;

	// Reply to responses part
	private MarginContainer replyContainer;
	public TextEdit replyEdit;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		if (fullResponseContainer == null) return; // FIXME: raise error
		vContainer = GetNodeOrNull<VBoxContainer>("%VResponsesContainer");
		if (vContainer == null) return; // FIXME: raise error
		diceContainer = GetNodeOrNull<MarginContainer>("%DiceContainer");
		if (diceContainer == null) return; // FIXME: raise error
		nextContainer = GetNodeOrNull<MarginContainer>("%NextContainer");
		if (nextContainer == null) return; // FIXME: raise error

		// Reply to responses part
		replyContainer = GetNode<MarginContainer>("%ReplyContainer");
		if (replyContainer == null) return; // FIXME: raise error
		replyEdit = GetNode<TextEdit>("%ReplyEdit");
		if (replyEdit == null) return; // FIXME: raise error
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

		// container.ReplyToResponse += ActivatedReply; // FIXME: delete?

		await container.ShowResponse(response);
		await Task.Delay(1000);

		if (gameEntity == null) return; // TODO: have the same if statmenet above, combine these

		// After the text is written, we show the reply container
		replyContainer.Visible = true;
		replyEdit.Editable = false;

		// If the gameEntity is not the player, it should not be able to allow changing of the text // TODO: or maybe it should be allowed?
		if (curDMResponse.respondeeGameEntity.GameEntityType == GameEntityType.Player) {
			replyEdit.Editable = true;
		}
		else {
			replyEdit.Editable = false;
		}
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

		curDMResponse = dmCharResponse;

		// Clear previous character responses
		ClearResponses();

		// Asking the DM for a first response
		JSONDMResponse result = await GameManager.Instance.game.DM_response((Character) dmCharResponse.respondeeGameEntity); // FIXME: bad downcasting thing
		dmCharResponse.dmResponse = result;

		await AddResponse(dmCharResponse.dmResponse.text, dmCharResponse.responderGameEntity); // FIXME: downcasting

		// TODO: maybe conversations should be attached to GameEntity?
		if (dmCharResponse.respondeeGameEntity is Character) {
			Character character = (Character) dmCharResponse.respondeeGameEntity;
			dmCharResponse.text = result.text; // FIXME: this is a distinct problem with the heirarchy of JSON and text stuff
			character.conversation.Add(dmCharResponse);
		}

		// TODO: show dice score needed to win in separate label?

		// If the DM response requires a roll of the dice, it is performed
		if (dmCharResponse.dmResponse.score > 0) {
			diceContainer.Visible = true;
		}
		else {
			nextContainer.Visible = true;
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

		nextContainer.Visible = true;
	}

	public async void _on_next_button_pressed() {
		if (curDMResponse == null) return;
		
		if (curDMResponse.dmResponse.score > 0) {
			if (!curDMResponse.ThrownDice) return;
		}

		nextContainer.Visible = false;

		string inputText = replyEdit.Text;
		replyEdit.Text = "";
		replyContainer.Visible = false;

		if (inputText.Length > 0) {
			FullResponseContainer container = fullResponseContainer.Instantiate<FullResponseContainer>();
			vContainer.AddChild(container);
			container.gameEntity = curDMResponse.respondeeGameEntity;

			await container.ShowResponse(inputText);

			// FIXME: downcasting bad
			if (curDMResponse.respondeeGameEntity is Character) {	
				Character character = (Character) curDMResponse.respondeeGameEntity;

				CharacterInteraction characterInteraction = new CharacterInteraction();
				characterInteraction.responderGameEntity = curDMResponse.respondeeGameEntity;
				characterInteraction.respondeeGameEntity = curDMResponse.responderGameEntity;
				characterInteraction.text = inputText;

				character.conversation.Add(characterInteraction);

				// FIXME: this part should always happen outside of these ifs
				// TODO: create LLM response here that is only explanation and summary instead of further DM JOB tag
				string summary = await GameManager.Instance.game.DM_response_summary(character);
				CharacterInteraction characterInteractionSum = new CharacterInteraction(curDMResponse.responderGameEntity, curDMResponse.respondeeGameEntity, summary);
				character.conversation.Add(characterInteractionSum);

				FullResponseContainer sumContainer = fullResponseContainer.Instantiate<FullResponseContainer>();
				vContainer.AddChild(sumContainer);
				sumContainer.gameEntity = characterInteractionSum.responderGameEntity;
				await sumContainer.ShowResponse(characterInteractionSum.text);

				GD.Print("Interactions: ");
				foreach (CharacterInteraction c in character.conversation) {
					GD.Print("From: " + c.responderGameEntity.Name + " To: " + c.respondeeGameEntity.Name + " Text: " + c.text);
				}
			}
		}

		// ClearResponses();
		// GameManager.Instance.game.MoveToNextCharacter();
	}
}
