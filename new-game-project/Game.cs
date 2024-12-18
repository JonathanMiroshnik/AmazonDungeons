using System.Runtime.Versioning;
using System.Security.Cryptography;
using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.Text.Json;

public partial class Game : Node
{
	[Export]
	public CameraMover cameraMover; // TODO: maybe there should be a CharacterCameraMover and a function MoveToNextCharacter?
	[Export]
	public CharacterUI characterUI;

	[Export]
	public int NumberOfRounds = 1;

	public string worldDescription = "";

	public override async void _Ready()
	{
		await GameManager.Instance.IsLoaded();

		if (cameraMover == null || characterUI == null) return; // FIXME: raise error

		// DoPrelude();

		for (int i = 0; i < NumberOfRounds; i++) {
			characterUI.ClearResponses();
			await DoRound();
		}
	}

	private void SeparatorPrint(string inner = "") {
		GD.Print();
		GD.Print("-----------------------------------------------------------------------------------------------");
		if (inner != "") {
			GD.Print(inner);
			GD.Print("-----------------------------------------------------------------------------------------------");
		}
		GD.Print();
	}

	public async Task DoRound() {
		float TIME_TO_MOVE = 2f;

		string retStr = "";
		for (int i = 0; i < GameManager.Instance.characters.Count; i++) {
			// Clear previous character responses
			characterUI.ClearResponses();

			// Choose the next character
			Character character = GameManager.Instance.characters[i];
			// make a list of the ShortenedDescriptions of the other characters // Amazon Q
			string otherCharactersStr = "";
			foreach (Character otherCharacter in GameManager.Instance.characters) {
				if (otherCharacter == character) continue;
				otherCharactersStr += " The name: " + otherCharacter.Name + " The description: " + otherCharacter.ShortenedDescription + ", ";
			}

			// Move to that Character
			await cameraMover.MoveCameraByIndex(i, TIME_TO_MOVE); // FIXME: maybe character number is not the same as index in the moveCamera?

			// First response from DM to character
			retStr = await GameManager.AskLlama(LLMLibrary.DM_PREFIX + LLMLibrary.DM_JOB_PREFIX + 
												LLMLibrary.LOCATION_PREFIX + character.Location +
												LLMLibrary.ALL_PLAYER_PREFIX + otherCharactersStr + 
												LLMLibrary.CURRENT_CHARACTER_PREFIX + character.GetDescription() + 
												LLMLibrary.JSON_DM_RESPONSE_TYPE + 
												"\nWrite a short response of up to 100 words.");
			
			// Accessing JSON output
			var jsonObject = JsonSerializer.Deserialize<JsonElement>(retStr);
			var retText = jsonObject.GetProperty("text").GetString();
			int retScore = jsonObject.GetProperty("score").GetInt32();
			GD.Print(retScore);

			await characterUI.AddResponse(retText, character);

			// TODO: Response from character to DM

			// TODO: Possible dice roll

			// TODO: DM Responds to dice roll
		}
	}

	public async void DoPrelude() {
		var worldStr = await GameManager.AskLlama(LLMLibrary.WORLD_CREATION + LLMLibrary.PRESENT_AS_POEM);
        GD.Print(worldStr);

		SeparatorPrint("Rewriting world description...");

		worldDescription = await GameManager.AskLlama("A description of a world in the form of an epic poem is thus:\n" 
														+ worldStr + LLMLibrary.REWRITE_WORLD_SUM + 
														"Write the description in 300 words or fewer");
        GD.Print(worldDescription);

		SeparatorPrint("Describing the characters....");

		string curCharacterStr = "";
		foreach (Character character in GameManager.Instance.characters) {
			curCharacterStr = await GameManager.AskLlama(LLMLibrary.DM_PREFIX + LLMLibrary.DESCRIBE_FOLLOWING_CHARACTER 
														+ character.Personality);
        	GD.Print(curCharacterStr);
		}
	}
	
	// TODO:
	// public async void FinalRound() {
	// 	return;
	// }
}
