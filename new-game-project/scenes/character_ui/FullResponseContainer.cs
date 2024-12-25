using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class FullResponseContainer : MarginContainer
{
	public GameEntity gameEntity;

	private Dictionary<GameEntityType, Color> gameEntityTypeColor = new Dictionary<GameEntityType, Color>()
	{
		{ GameEntityType.AI, Colors.Red },
		{ GameEntityType.Player, Colors.Blue },
		{ GameEntityType.DungeonMaster, Colors.Green }
	};

	private Label gameEntityNameLabel;
	private RichTextLabel responseRichLabel;
	private MarginContainer userLabelContainer;

	// Reply to responses part
	private MarginContainer replyContainer;
	public TextEdit replyEdit;
	// private Button replyButton;

	[Signal]
	public delegate void ReplyToResponseEventHandler(string reply);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		responseRichLabel = GetNode<RichTextLabel>("%ResponseRichLabel");
		if (responseRichLabel == null) return; // FIXME: raise error
		gameEntityNameLabel = GetNode<Label>("%GameEntityNameLabel");
		if (gameEntityNameLabel == null) return; // FIXME: raise error
		userLabelContainer = GetNode<MarginContainer>("%UserLabelContainer");
		if (userLabelContainer == null) return; // FIXME: raise error

		// Reply to responses part
		replyContainer = GetNode<MarginContainer>("%ReplyContainer");
		if (replyContainer == null) return; // FIXME: raise error
		replyEdit = GetNode<TextEdit>("%ReplyEdit");
		if (replyEdit == null) return; // FIXME: raise error
		// replyButton = GetNode<Button>("%ReplyButton");
		// if (replyButton == null) return; // FIXME: raise error
	}

	public async Task ShowResponse(string response)
	{
		float TIME_TO_SHOW = 3f;

		if (gameEntity == null) {
			userLabelContainer.Visible = false;
			userLabelContainer.SetProcess(false);
		}
		else {
			userLabelContainer.SetProcess(true);
			userLabelContainer.Visible = true;
			gameEntityNameLabel.Text = gameEntity.Name;
			gameEntityNameLabel.AddThemeColorOverride("font_color", gameEntityTypeColor[gameEntity.GameEntityType]);
		}

		// Writing the text
		await GlobalStringLibrary.TypeWriteOverDuration(response, responseRichLabel, TIME_TO_SHOW);

		if (gameEntity == null) return; // TODO: have the same if statmenet above, combine these

		// After the text is written, we show the reply container
		replyContainer.Visible = true;
		replyEdit.Editable = false;

		// If the gameEntity is not the player, it should not be able to allow changing of the text // TODO: or maybe it should be allowed?
		if (gameEntity.GameEntityType == GameEntityType.Player) {
			replyEdit.Editable = true;
		}
	}

	// TODO: these two buttons should not be in this container, but in the one above, characteUI
	// public void _on_reply_button_pressed() {
	// 	EmitSignal(SignalName.ReplyToResponse, replyContainer);
	// 	replyButton.Text = "Sent!";
	// 	replyButton.Disabled = true;
	// }
}
