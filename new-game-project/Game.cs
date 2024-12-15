using Godot;

public partial class Game : Node
{
	public override async void _Ready()
	{
		// print hello world
		GD.Print("Hello World!");
		DoRound();
	}

	public async void DoRound() {
		foreach (Character character in GameManager.Instance.characters) {
			string retStr = await GameManager.AskLlama(character.Personality);
		}
	}

	
}
