using System.Runtime.Versioning;
using System.Security.Cryptography;
using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

using System.Text.Json;
using Amazon.BedrockAgentRuntime.Model.Internal.MarshallTransformations;

public partial class Game : Node
{
	[Export]
	public CameraMover cameraMover; // TODO: maybe there should be a CharacterCameraMover and a function MoveToNextCharacter?
	[Export]
	public CharacterUI characterUI;
	[Export]
	public DiceThrowerMechanism diceThrowerMechanism;

	[Export]
	public int NumberOfRounds = 1;

	public string worldDescription = "";

	private List<string> responses;

	public override async void _Ready()
	{
		await GameManager.Instance.IsLoaded();

		if (cameraMover == null || characterUI == null || diceThrowerMechanism == null) return; // FIXME: raise error

		DoPrelude();

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

		for (int i = 0; i < GameManager.Instance.characters.Count; i++) {
			// Clear previous character responses
			characterUI.ClearResponses();

			// Choose the next character
			Character character = GameManager.Instance.characters[i];

			// Move to that Character
			await cameraMover.MoveCameraByIndex(i, TIME_TO_MOVE); // FIXME: maybe character number is not the same as index in the moveCamera?

			JSONDMResponse result = await DM_response(character);

			// TODO: Response from character to DM

			// TODO: Possible dice roll

			// TODO: DM Responds to dice roll
		}
	}

	public async Task<JSONDMResponse> DM_response(Character character) {
		string retStr = "";

		// make a list of the ShortenedDescriptions of the other characters // Amazon Q
		string otherCharactersStr = "";
		foreach (Character otherCharacter in GameManager.Instance.characters) {
			if (otherCharacter == character) continue;
			otherCharactersStr += " The name: " + otherCharacter.Name + " The description: " + otherCharacter.ShortenedDescription + ", ";
		}

		// First response from DM to character
		retStr = await GameManager.AskLlama(LLMLibrary.DM_PREFIX + LLMLibrary.DM_JOB_PREFIX + 
											LLMLibrary.LOCATION_PREFIX + GameManager.Instance.location +
											LLMLibrary.ALL_PLAYER_PREFIX + otherCharactersStr + 
											LLMLibrary.CURRENT_CHARACTER_PREFIX + character.GetDescription() + 
											LLMLibrary.JSON_DM_RESPONSE_TYPE + 
											"\nWrite a short response of up to 100 words.");
		retStr = GlobalStringLibrary.JSONStringBrackets(retStr);
		
		// Accessing JSON output
		JSONDMResponse result = JsonConvert.DeserializeObject<JSONDMResponse>(retStr);

		await characterUI.AddResponse(result.text, character);
		responses.Add(result.text);

		return result;
	}
	public async void DoPrelude() {
		GD.Print("Describing the world....\n\n");
		GD.Print(GameManager.Instance.worldDescription + "\n\n");

		SeparatorPrint("Describing the characters....\n\n");
		foreach (Character character in GameManager.Instance.characters) {
			GD.Print(character.ShortenedDescription + "\n");
		}
	}
	
	// TODO:
	// public async void FinalRound() {
	// 	return;
	// }
}
