using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class FullResponseContainer : MarginContainer
{
	public Character character;

	private Dictionary<GameEntityType, Color> gameEntityTypeColor = new Dictionary<GameEntityType, Color>()
	{
		{ GameEntityType.AI, Colors.Red },
		{ GameEntityType.Player, Colors.Blue },
		{ GameEntityType.DungeonMaster, Colors.Green }
	};

	private Label characterNameLabel;
	private RichTextLabel responseRichLabel;
	private MarginContainer userLabelContainer;

	// Reply to responses part
	private MarginContainer replyContainer;
	private TextEdit replyEdit;
	private Button replyButton;

	[Signal]
	public delegate void ReplyToResponseEventHandler(string reply);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		responseRichLabel = GetNode<RichTextLabel>("%ResponseRichLabel");
		if (responseRichLabel == null) return; // FIXME: raise error
		characterNameLabel = GetNode<Label>("%CharacterNameLabel");
		if (characterNameLabel == null) return; // FIXME: raise error
		userLabelContainer = GetNode<MarginContainer>("%UserLabelContainer");
		if (userLabelContainer == null) return; // FIXME: raise error

		// Reply to responses part
		replyContainer = GetNode<MarginContainer>("%ReplyContainer");
		if (replyContainer == null) return; // FIXME: raise error
		replyEdit = GetNode<TextEdit>("%ReplyEdit");
		if (replyEdit == null) return; // FIXME: raise error
		replyButton = GetNode<Button>("%ReplyButton");
		if (replyButton == null) return; // FIXME: raise error
	}

	public async Task ShowResponse(string response)
	{
		float TIME_TO_SHOW = 3f;

		if (character == null) {
			userLabelContainer.Visible = false;
			userLabelContainer.SetProcess(false);
		}
		else {
			userLabelContainer.SetProcess(true);
			userLabelContainer.Visible = true;
			characterNameLabel.Text = character.Name;
			characterNameLabel.AddThemeColorOverride("font_color", gameEntityTypeColor[character.GameEntityType]);
		}

		// Writing the text
		await GlobalStringLibrary.TypeWriteOverDuration(response, responseRichLabel, TIME_TO_SHOW);

		// After the text is written, we show the reply container
		replyContainer.Visible = true;
	}

	public void _on_reply_button_pressed() {
		EmitSignal(SignalName.ReplyToResponse, replyContainer);
		replyButton.Text = "Sent!";
		replyButton.Disabled = true;
	}
}
