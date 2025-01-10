using Godot;
using System;
using System.Threading.Tasks;

public partial class CharacterUI : Control
{
	[Export]
	public PackedScene fullResponseContainer;

	// State Machine that controls the pace of the dialogue, it is connected both ways with the CharacterUI
	public DialogueStateMachine dialogueStateMachine;

	// Current conversation between DM and character that is being shown in the UI // TODO: generalize it beyong DM-character interactions
	public DMCharacterResponse curDMResponse;

	private VBoxContainer vContainer;
	private MarginContainer diceContainer;
	public MarginContainer nextContainer;
	public MarginContainer diceDescContainer;
	public RichTextLabel diceDescLabel;

	// Reply to responses part
	public MarginContainer replyContainer;
	public TextEdit replyEdit;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (fullResponseContainer == null)	throw new ArgumentException("Add FullResponseContainer to the CharacterUI");
		vContainer = GetNodeOrNull<VBoxContainer>("%VResponsesContainer");
		if (vContainer == null) throw new ArgumentException("Add VResponsesContainer to the CharacterUI");
		diceContainer = GetNodeOrNull<MarginContainer>("%DiceContainer");
		if (diceContainer == null) throw new ArgumentException("Add DiceContainer to the CharacterUI");
		nextContainer = GetNodeOrNull<MarginContainer>("%NextContainer");
		if (nextContainer == null) throw new ArgumentException("Add NextContainer to the CharacterUI");

		// Reply to responses part
		replyContainer = GetNode<MarginContainer>("%ReplyContainer");
		if (replyContainer == null) throw new ArgumentException("Add ReplyContainer to the CharacterUI");
		replyEdit = GetNode<TextEdit>("%ReplyEdit");
		if (replyEdit == null) throw new ArgumentException("Add ReplyEdit to the CharacterUI");

		// Dice description holder
		diceDescContainer = GetNode<MarginContainer>("%DiceDescContainer");
		if (diceDescContainer == null) throw new ArgumentException("Add DiceDescContainer to the CharacterUI");
		diceDescLabel = GetNode<RichTextLabel>("%DiceRichLabel");
		if (diceDescLabel == null) throw new ArgumentException("Add DiceRichLabel to the CharacterUI");
	}

	public async Task AddResponse(string response)
	{
		await AddResponse(response, null, false);
	}

	public async Task AddResponse(string response, GameEntity gameEntity, bool RespondingOption = true)
	{
		FullResponseContainer container = fullResponseContainer.Instantiate<FullResponseContainer>();
		vContainer.AddChild(container);
		container.gameEntity = gameEntity;

		await container.ShowResponse(response);
		await Task.Delay(1000);

		if (gameEntity == null) return; // TODO: have the same if statmenet above, combine these

		if (!RespondingOption) {
			replyContainer.Visible = false;
			replyEdit.Editable = false;
			replyEdit.Text = "";

			return;
		}

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
		JSONDMResponse result = await LLMLibrary.DM_response((Character) dmCharResponse.respondeeGameEntity); // FIXME: bad downcasting thing
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
			dialogueStateMachine.ChangeState(new DiceThrowing(curDMResponse));
		}
		else {
			dialogueStateMachine.ChangeState(new ResponseDialogue(dmCharResponse));
		}
	}

	public async void _on_dice_button_pressed() {
		// TODO: null checks?
		// TODO: how do we make sure that the curDMResponse is really the current one and not a previous one?

		diceContainer.Visible = false;
		Visible = false;

		await dialogueStateMachine.CurrentState.Action(dialogueStateMachine);
	}

	// The "next" button should activate the Action of the dialogue state machine to further the dialogue along
	public async void _on_next_button_pressed() {
		if (curDMResponse == null) return;
		
		if (curDMResponse.dmResponse.score > 0) {
			if (!curDMResponse.ThrownDice) return;
		}

		replyContainer.Visible = false;
		nextContainer.Visible = false;

		await dialogueStateMachine.CurrentState.Action(dialogueStateMachine);
	}

	// When the replyEdit text input has text in it, the next button should show itself
	public void _on_reply_edit_text_changed() {
		if (replyEdit.Text.Length > 0) {
			nextContainer.Visible = true;
		}
		else {
			nextContainer.Visible = false;
		}
	}

	public void ReplyToggle(bool allowReplying) {
		replyEdit.Editable = allowReplying;
		replyContainer.Visible = allowReplying;

		if (!allowReplying) {
			replyEdit.Text = "";
		}
	}
}
