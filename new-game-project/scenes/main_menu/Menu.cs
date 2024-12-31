using Godot;

public partial class Menu : Control
{
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
