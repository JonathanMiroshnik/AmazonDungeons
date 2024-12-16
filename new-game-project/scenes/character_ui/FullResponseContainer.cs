using Godot;
using System.Collections.Generic;

public partial class FullResponseContainer : MarginContainer
{
	public Character character;

	private Dictionary<CharacterType, Color> characterTypeColor = new Dictionary<CharacterType, Color>()
	{
		{ CharacterType.AI, Colors.Red },
		{ CharacterType.Player, Colors.Blue },
		{ CharacterType.DungeonMaster, Colors.Green }
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

	public async void ShowResponse(string response)
	{
		if (character == null) {
			userLabelContainer.Visible = false;
			userLabelContainer.SetProcess(false);
		}
		else {
			userLabelContainer.SetProcess(true);
			userLabelContainer.Visible = true;
			characterNameLabel.Text = character.Name;
			characterNameLabel.AddThemeColorOverride("font_color", characterTypeColor[character.CharacterType]);
		}

		GlobalStringLibrary.TypeWriteOverDuration(response, responseRichLabel, 3);
	}
}
