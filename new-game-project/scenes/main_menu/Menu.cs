using System.Threading.Tasks;
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

	public async void _on_start_game_button_pressed() {
		GD.Print("here");

		GetNode<GlobalAudioLibrary>("AudioStreamPlayer").PlayRandomSound(GlobalAudioLibrary.TEST);
		await Task.Delay(500);

		try {
			GetTree().ChangeSceneToFile("res://scenes/character_creation/character_creation.tscn");	
		}
		catch (System.Exception e) {
			GD.PrintErr(e);
		}
	}

	public void _on_exit_game_button_pressed() {
		GetNode<GlobalAudioLibrary>("AudioStreamPlayer").PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);

		GetTree().Quit();
	}
}
