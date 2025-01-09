using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Presents a response from a GameEntity in the UI, showing their name and the text
/// </summary>
public partial class FullResponseContainer : MarginContainer
{
	// GameEntity that is responding
	public GameEntity gameEntity;

	// Different name colors indicate different types of GameEntities
	private Dictionary<GameEntityType, Color> gameEntityTypeColor = new Dictionary<GameEntityType, Color>()
	{
		{ GameEntityType.AI, Colors.Red },
		{ GameEntityType.Player, Colors.Blue },
		{ GameEntityType.DungeonMaster, Colors.Green }
	};

	private Label gameEntityNameLabel;
	private RichTextLabel responseRichLabel;
	private MarginContainer userLabelContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		responseRichLabel = GetNode<RichTextLabel>("%ResponseRichLabel");
		if (responseRichLabel == null) throw new Exception("ResponseRichLabel not found in FullResponseContainer");
		gameEntityNameLabel = GetNode<Label>("%GameEntityNameLabel");
		if (gameEntityNameLabel == null) throw new Exception("GameEntityNameLabel not found in FullResponseContainer");
		userLabelContainer = GetNode<MarginContainer>("%UserLabelContainer");
		if (userLabelContainer == null) throw new Exception("UserLabelContainer not found in FullResponseContainer");
	}

	/// <summary>
	/// Shows the response text over a designated time.
	/// </summary>
	public async Task ShowResponse(string response)
	{
		// Time it takes the text to fully show, we seek to not make the Player wait too long
		float TIME_TO_SHOW = Math.Min(3f, response.Length * 0.05f);

		// If there is no GameEntity, the response is shown as-is without a name
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
	}
}
