using System.Runtime.Versioning;
using System.Security.Cryptography;
using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

using System.Text.Json;
using Amazon.BedrockAgentRuntime.Model.Internal.MarshallTransformations;
using Amazon.Runtime.Internal;

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

	// Represents the Dungeon Master for the game, of which there is only one per game
	private GameEntity DungeonMaster = new GameEntity(GameEntityType.DungeonMaster);

	// Time it will take for the Camera to move from one position to another(in seconds)
	private float TIME_TO_MOVE = 2f;

	public override async void _Ready()
	{
		await GameManager.Instance.IsLoaded();

		if (cameraMover == null || characterUI == null || diceThrowerMechanism == null) return; // FIXME: raise error

		// DoPrelude();
		
		characterUI.ClearResponses();
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
		for (int i = 0; i < GameManager.Instance.characters.Count; i++) {
			// Clear previous character responses
			characterUI.ClearResponses();

			// Choose the next character
			Character character = GameManager.Instance.characters[i];

			// Move to that Character
			await cameraMover.MoveCameraByIndex(i, TIME_TO_MOVE); // FIXME: maybe character number is not the same as index in the moveCamera?

			// --------------------------------------------------------------------------------------------------- 23.12.24
			JSONDMResponse result = await DM_response(character);

			if (result.score > 0) {
				bool won = await PlayDice(result.score);

				// Move back to the Character
				await cameraMover.MoveCameraByIndex(i, TIME_TO_MOVE); // FIXME: maybe character number is not the same as index in the moveCamera?
				if (won) {
					
				}
				else {

				}
			}	

			// TODO: Response from character to DM

			// TODO: Possible dice roll

			// TODO: DM Responds to dice roll
		}
	}

	public async Task<JSONDMResponse> DM_response(Character character) {
		// // make a list of the ShortenedDescriptions of the other characters // Amazon Q
		// string otherCharactersStr = "";
		// foreach (Character otherCharacter in GameManager.Instance.characters) {
		// 	if (otherCharacter == character) continue;
		// 	otherCharactersStr += " The name: " + otherCharacter.Name + " The description: " + otherCharacter.ShortenedDescription + ", ";
		// }
		
		// Construct the input for the LLM
		string input = ConstructLLMInput(true, true, false, character);
		// string other = LLMLibrary.DM_PREFIX + LLMLibrary.DM_JOB_PREFIX + 
		// 									LLMLibrary.LOCATION_PREFIX + GameManager.Instance.location +
		// 									LLMLibrary.ALL_PLAYER_PREFIX + otherCharactersStr + 
		// 									LLMLibrary.CURRENT_CHARACTER_PREFIX + character.GetDescription() + 
		// 									LLMLibrary.CHARACTER_DM_HISTORY_PREFIX + "" +
		// 									LLMLibrary.JSON_DM_RESPONSE_TYPE + 
		// 									"\nWrite a short response of up to 100 words.";
		// GD.Print("Input to LLM: \n" + input);
		// GD.Print("Input to LLM 2: \n" + other);

		string retStr = await GameManager.AskLlama(input);
		retStr = GlobalStringLibrary.JSONStringBrackets(retStr);
		
		// Accessing JSON output
		JSONDMResponse result = JsonConvert.DeserializeObject<JSONDMResponse>(retStr);

		GD.Print("DM Response: " + result.text + " Dice number: " + result.score);

		await characterUI.AddResponse(result.text, character);

		return result;
	}

	public async Task<bool> PlayDice(int diceToWin) {
		// TODO: change number of dice here accordingly
		int curDiceToThrow = 5;

		// TODO: this DicePos thing feels magical numbery
		await cameraMover.MoveCameraByString("DicePos", TIME_TO_MOVE);

		diceThrowerMechanism.numDice = curDiceToThrow;
		int final = await diceThrowerMechanism.ThrowDice();
		
		GD.Print("The number of dice you got: " + final + " the number of dice you needed: " + diceToWin);
		
		if (final >= diceToWin) {
			GD.Print("You won!");
			return true;
		}
		else {
			GD.Print("You lost!");
			return false;
		}
	}


	public string ConstructLLMInput(bool DM, bool DM_JOB, bool CHARACTER, Character character, bool AfterDice = false, bool Victory = false) {
		string retStr = "";

		// Chooses between DM and regular character
		if (DM) {
			retStr += LLMLibrary.DM_PREFIX;
			// The DM sometimes has responses that are not part of the gameplay and thus does not need to return a specific JSON output
			if (DM_JOB) {
				retStr += LLMLibrary.DM_JOB_PREFIX;
			}
		}
		else if (CHARACTER) {
			retStr += LLMLibrary.CHARACTER_PREFIX;
		}

		if (GameManager.Instance.characters == null) return retStr; // TODO: maybe instead we write "there are no characters" ?
		if (GameManager.Instance.characters.Count == 0) return retStr; // TODO: maybe raise error?

		// Adds the location
		retStr += LLMLibrary.LOCATION_PREFIX + GameManager.Instance.location;

		// If there is only one character, there is no need to add the other characters' descriptions
		if (GameManager.Instance.characters.Count > 1) {
			// make a list of the ShortenedDescriptions of the other characters // Amazon Q
			string otherCharactersStr = "";
			foreach (Character otherCharacter in GameManager.Instance.characters) {
				if (otherCharacter == character) continue;
				otherCharactersStr += " The name: " + otherCharacter.Name + " The description: " + otherCharacter.ShortenedDescription + ", ";
			}

			// Adding the other characters' descriptions
			retStr += LLMLibrary.ALL_PLAYER_PREFIX + otherCharactersStr;
		}

		if (character != null) {
			// Adding the current character that is being responded to
			retStr += LLMLibrary.CURRENT_CHARACTER_PREFIX + character.GetDescription();

			// Adding the conversation history
			if (character.conversation != null) {
				if (character.conversation.Count > 0) {
					// Because there is a conversation history, a prefix is needed before the actual history is given.
					retStr += LLMLibrary.CHARACTER_DM_HISTORY_PREFIX; // TODO: what if they are all of other players talking to the current character, 
																	  //  not DM or the character itself?

					string LastResponse = "";
					GameEntity LastResponder = null;
					foreach (CharacterInteraction resp in character.conversation) {
						// TODO: notice that currently we don't know exactly who is responding to whom etc, only who is responding
						// We are interested in the conversations between the DM and the character
						if (resp.responderGameEntity == DungeonMaster) {
							DMCharacterResponse curResp = (DMCharacterResponse) resp;

							retStr += "Dungeon Master: " + curResp.dmResponse.text + "\n";
							// Because this is a DM response, we want to check whether the character succeded in the action
							if (curResp.ThrownDice) {
								if (curResp.ThrownDiceSuccess) {
									retStr += " The character succeeded in the action!\n";
								}
								else {
									retStr += " The character failed in the action!\n";
								}
							}

							LastResponse = curResp.dmResponse.text; // TODO: should it also contain victory/loss status for this response?
						}
						else if (resp.responderGameEntity == character) {
							CharacterResponse curResp = (CharacterResponse) resp;
							retStr += character.Name + ": " + curResp.text + "\n";
							LastResponse = curResp.text;
						}
					}

					if (LastResponder != null) {
						retStr += "The last responder is: " + LastResponder.Name + " responded last with(you must respond to this): " + LastResponse;
					}
				}
			}
		}

		if (AfterDice) {
			if (Victory) {
				retStr += LLMLibrary.AFTER_DICE_VICTORY_PREFIX;
			} else {
				retStr += LLMLibrary.AFTER_DICE_DEFEAT_PREFIX;
			}
		}

		// Notice that this if statement already occured at the beginning
		if (DM_JOB) {
			// TODO: maybe short/moderated length responses should be regulated a different way?
			retStr += LLMLibrary.JSON_DM_RESPONSE_TYPE + "\nWrite a short response of up to 100 words.";
		}


		return retStr;
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
