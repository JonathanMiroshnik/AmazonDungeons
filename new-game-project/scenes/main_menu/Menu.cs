using Godot;

public partial class Menu : Control
{
	private Button startGameButton;

	public override async void _Ready() {
		startGameButton = GetNode<Button>("%StartGameButton");
		if (startGameButton == null) {
			GD.PrintErr("StartGameButton is null");
			return;
		}

		startGameButton.Text = "Loading...";
		startGameButton.Disabled = true;

		await GameManager.Instance.IsLoaded();

		startGameButton.Text = "Play Game";
		startGameButton.Disabled = false;
	}

	public void _on_start_game_button_pressed() {
		var nextScene = ResourceLoader.Load<PackedScene>("res://scenes/character_creation/character_creation.tscn").Instantiate();
		var prevScene = GetTree().CurrentScene;

		GetTree().Root.AddChild(nextScene);
		prevScene.QueueFree();
	}

	public void _on_exit_game_button_pressed() {
		GetTree().Quit();
	}
}
