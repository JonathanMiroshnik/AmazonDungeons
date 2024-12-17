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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		responseRichLabel = GetNode<RichTextLabel>("%ResponseRichLabel");
		if (responseRichLabel == null) return; // FIXME: raise error
		characterNameLabel = GetNode<Label>("%CharacterNameLabel");
		if (characterNameLabel == null) return; // FIXME: raise error
		userLabelContainer = GetNode<MarginContainer>("%UserLabelContainer");
		if (userLabelContainer == null) return; // FIXME: raise error
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

		await GlobalStringLibrary.TypeWriteOverDuration(response, responseRichLabel, TIME_TO_SHOW);
	}
}
