using Godot;

public partial class Menu : Control
{
	public void _on_start_game_button_pressed() {
	}

	public void _on_exit_game_button_pressed() {
		GetTree().Quit();
	}
}
