using System.Runtime.Versioning;
using System.Security.Cryptography;
using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

using System.Text.Json;
using Amazon.BedrockAgentRuntime.Model.Internal.MarshallTransformations;
using Amazon.Runtime.Internal;
using System.Data;

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

	// Time it will take for the Camera to move from one position to another(in seconds)
	private const float TIME_TO_MOVE = 2f;

	private List<string> GAME_ENTITIES_POS = new List<string> { "MainCharacter", "RightCharacter", "ForwardCharacterDM", "LeftCharacter" };

	private Node3D DICE_POS;

	public override async void _Ready()
	{
		await GameManager.Instance.IsLoaded();
		GameManager.Instance.RegisterGame(this);

		if (cameraMover == null || characterUI == null || diceThrowerMechanism == null) return; // FIXME: raise error

		// TODO: weird check that isn't appropriate in GameManager but also weird here as it combines both of these....

		// Connecting between the Game Entities and the Camera positions
		if (GameManager.Instance.gameEntities == null || GAME_ENTITIES_POS == null) return;
		if (GameManager.Instance.gameEntities.Count != GameManager.NUMBER_OF_GAME_ENTITIES) {
			GD.Print("Error: the number of game entities in the game is not correct");
			return;
		}
		if (GAME_ENTITIES_POS.Count != GameManager.NUMBER_OF_GAME_ENTITIES) {
			GD.Print("Error: the number of game entity positions in the game is not correct");
			return;
		}

		for (int i = 0; i < GameManager.Instance.gameEntities.Count; i++) {
			if (GameManager.Instance.gameEntities[i] == null) {
				GD.Print("Error: a character in the game is null");
				return;
			}

			GameManager.Instance.gameEntities[i].worldSpacePosition = GetNodeOrNull<Node3D>("%" + GAME_ENTITIES_POS[i]);
			if (GameManager.Instance.gameEntities[i].worldSpacePosition == null) {
				GD.Print("Error: a corresponding Node3D position has not been found for " + GAME_ENTITIES_POS[i]);
				return;
			}
		}

		// Finding the Dice camera position
		DICE_POS = GetNodeOrNull<Node3D>("%DicePos");
		if (DICE_POS == null) {
			GD.Print("Error: a corresponding Node3D position has not been found for DicePos");
			return;
		}

		// DM describes the world to the players before the start of the actual game
		// DoPrelude();
		
		// Main Game loop
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
		for (int i = 0; i < GameManager.Instance.gameEntities.Count; i++) {
			if (GameManager.Instance.gameEntities[i] == null) {
				GD.Print("Error: a character in the game is null");
				return;
			}
			if (GameManager.Instance.gameEntities[i] is not Character) continue;

			// Clear previous character responses
			characterUI.ClearResponses();

			// Choose the next character
			Character character = (Character) GameManager.Instance.gameEntities[i];

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

		if (GameManager.Instance.gameEntities == null) return retStr; // TODO: maybe instead we write "there are no characters" ?
		if (GameManager.Instance.gameEntities.Count == 0) return retStr; // TODO: maybe raise error?

		// Adds the location
		retStr += LLMLibrary.LOCATION_PREFIX + GameManager.Instance.worldDesc.location;

		// If there is only one character, there is no need to add the other characters' descriptions
		if (GameManager.Instance.gameEntities.Count > 1) {
			// make a list of the ShortenedDescriptions of the other characters // Amazon Q
			string otherCharactersStr = "";
			foreach (GameEntity otherCharacter in GameManager.Instance.gameEntities) {
				if (otherCharacter is not Character) continue;
				Character otherCharDown = (Character) otherCharacter;
				if (otherCharacter == character) continue;

				otherCharactersStr += " The name: " + otherCharDown.Name + " The description: " + otherCharDown.ShortenedDescription + ", ";
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
						if (resp.responderGameEntity == GameManager.Instance.DungeonMaster) {
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
		GD.Print(GameManager.Instance.worldDesc.world + "\n\n");

		SeparatorPrint("Describing the characters....\n\n");
		foreach (GameEntity curGameEntity in GameManager.Instance.gameEntities) {
			if (curGameEntity is not Character) continue;
			Character curCharDown = (Character) curGameEntity;

			GD.Print(curCharDown.ShortenedDescription + "\n");
		}
	}
	
	// TODO: the next action should always be ready for execution
	public void NextAction() {
		GD.Print("Next action");
	}

	// TODO:
	// public async void FinalRound() {
	// 	return;
	// }
}
