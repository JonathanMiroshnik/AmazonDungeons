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

	// Indicates the current character that is being interacted with
	private int indexCurrentCharacter = 0;

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
		// DoDMConvo(GameManager.Instance.gameEntities[indexCurrentCharacter]); // FIXME: need to figure out what connects characters in chain
		
		GameStateMachine gameStateMachine = new GameStateMachine();
		gameStateMachine.cameraMover = cameraMover;
		gameStateMachine.StartGame();


		// TODO: stop after NumberOfRounds, or get to final round?
	}

	// TODO: change func name
	public async void DoDMConvo(GameEntity gameEntity) {
		// Move to that Character
		await cameraMover.MoveCameraByNode3D(gameEntity.worldSpacePosition, GameManager.TIME_TO_MOVE_CAMERA_POSITIONS);

		// Creating the conversation between the DM and the character
		DMCharacterResponse resp = new DMCharacterResponse(); // TODO: very bad code and structure
		resp.responderGameEntity = GameManager.Instance.DungeonMaster;
		resp.respondeeGameEntity = gameEntity;

		await characterUI.DoDMConversation(resp);
	}

	public async Task<JSONDMResponse> DM_response(Character character) {
		// Construct the input for the LLM
		string input = LLMLibrary.ConstructLLMInput(true, true, false, character, true);

		string retStr = await GameManager.AskLlama(input);
		retStr = GlobalStringLibrary.JSONStringBrackets(retStr);
		
		// Accessing JSON output
		JSONDMResponse result = JsonConvert.DeserializeObject<JSONDMResponse>(retStr);

		GD.Print("DM Response: " + result.text + " Dice number: " + result.score);

		return result;
	}

	public async Task<string> DM_response_summary(Character character) {
		// Construct the input for the LLM
		string input = LLMLibrary.ConstructLLMInput(true, false, false, character, true);

		string retStr = await GameManager.AskLlama(input);
		GD.Print("DM summary: " + retStr);

		return retStr;
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
}
