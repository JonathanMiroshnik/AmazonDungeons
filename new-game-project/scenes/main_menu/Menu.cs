using System.Threading.Tasks;
using Godot;

public partial class Menu : Control
{
	[Export]
	public GlobalAudioLibrary globalAudioLibrary;

	private Button startGameButton;
	private SpinBox roundsBox;

	public override async void _Ready() {
		startGameButton = GetNode<Button>("%StartGameButton");
		if (startGameButton == null) {
			GD.PrintErr("StartGameButton is null");
			return;
		}

		roundsBox = GetNode<SpinBox>("%RoundsBox");
		if (roundsBox == null) {
			GD.PrintErr("RoundsBox is null");
			return;
		}

		if (globalAudioLibrary == null) throw new System.Exception("globalAudioLibrary is null in Menu");

		startGameButton.Text = "Loading...";
		startGameButton.Disabled = true;

		await GameManager.Instance.IsLoaded();

		startGameButton.Text = "Play Game";
		startGameButton.Disabled = false;
	}

	public async void _on_start_game_button_pressed() {
		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);
		GameManager.Instance.TotalRounds = (int) roundsBox.Value;
		await Task.Delay(200);

		try {
			GetTree().ChangeSceneToFile("res://scenes/character_creation/character_creation.tscn");	
		}
		catch (System.Exception e) {
			GD.PrintErr(e);
		}
	}

	public async void _on_exit_game_button_pressed() {
		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);
		await Task.Delay(200);

		GetTree().Quit();
	}
}
