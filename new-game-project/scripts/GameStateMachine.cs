using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface GameState
{
	public abstract void Enter(GameStateMachine gameStateMachine);
	public abstract void Exit(GameStateMachine gameStateMachine);
	public abstract void Action(GameStateMachine gameStateMachine);
}

public partial class StartGame : GameState {
	public async void Enter(GameStateMachine gameStateMachine) {
		// GD.Print("Enter StartGame");

		gameStateMachine.characterUI.HealthContainer.Visible = false;
		gameStateMachine.characterUI.Visible = true;

		GD.Print("wew2");
		string curWorldDesc = await LLMLibrary.WorldPrelude();

		await gameStateMachine.characterUI.AddResponse(curWorldDesc);
		await Task.Delay(3000);

		gameStateMachine.ChangeState(new DMDialogue(gameStateMachine.NextCharacter()));
	}

	public void Exit(GameStateMachine gameStateMachine) {
		// GD.Print("Exit StartGame");
	}
	
	public void Action(GameStateMachine gameStateMachine) {
		// GD.Print("Exit ActionGame");
	}
}
 
 
 public partial class EndGame : GameState {
	public async void Enter(GameStateMachine gameStateMachine) {
		// GD.Print("Enter EndGame");
		await gameStateMachine.cameraMover.MoveCameraByNode3D(GameManager.Instance.characters[0].worldSpacePosition, 
																GameStateMachine.TIME_TO_MOVE);

		GameManager.Instance.SceneChange("res://scenes/EndScreen.tscn");
	}

	public void Exit(GameStateMachine gameStateMachine) {
		// GD.Print("Exit EndGame");
	}
	public void Action(GameStateMachine gameStateMachine) {
		return;
	}
}


public partial class DMDialogue : GameState {
	public Character character { get; set; }
	public DMDialogue(Character curCharacter) {
		character = curCharacter;
	}

	public async void Enter(GameStateMachine gameStateMachine) {
		// GD.Print("Enter DMDialogue");
		if (character == null) {
			Action(gameStateMachine);
			return;
		}

		gameStateMachine.characterUI.HealthContainer.Visible = true;
		gameStateMachine.characterUI.SetHP(character.HealthPoints);

		DialogueStateMachine dialogueStateMachine = new DialogueStateMachine(gameStateMachine, character);

		await gameStateMachine.cameraMover.MoveCameraByNode3D(character.worldSpacePosition, GameStateMachine.TIME_TO_MOVE);
		dialogueStateMachine.ChangeState(new StartDialogue(gameStateMachine, dialogueStateMachine));
	}

	public void Exit(GameStateMachine gameStateMachine) {
		// GD.Print("Exit DMDialogue");
	}

	public void Action(GameStateMachine gameStateMachine) {
		// GD.Print("Action DMDialogue");

		// If the character is null, that means we finished the round and are now moving onto the next round, or the end of the game.
		if (character == null) {
			if (gameStateMachine.CurrentRound+1 >= gameStateMachine.NumberOfRounds) {
				gameStateMachine.ChangeState(new EndGame());
			} else {
				gameStateMachine.CurrentRound++;
				gameStateMachine.ChangeState(new DMDialogue(gameStateMachine.NextCharacter()));
			}
		}
		else {
			gameStateMachine.ChangeState(new DMDialogue(gameStateMachine.NextCharacter()));
		}
	}
	
}

// TODO: should the State machine move itself or should something else create it, activate it?

public partial class GameStateMachine : Node
{
	public GameState CurrentState { get; private set; }

	// TODO: maybe there should be a CharacterCameraMover and a function MoveToNextCharacter?
	[Export]
	public CameraMover cameraMover;
	// Time it will take for the Camera to move from one position to another(in seconds)
	public const float TIME_TO_MOVE = 2f;

	[Export]
	public CharacterUI characterUI;
	[Export]
	public DiceThrowerMechanism diceThrowerMechanism;

	// Total number of rounds in the game
	public int NumberOfRounds = 1;
	public int CurrentRound = 0;

	// Current character index that is in dialogue with the DM
	public int character_ind = 0;

	// Camera positions relating to GameEntities, Character and other relevant objects
	private List<string> GAME_ENTITIES_POS = new List<string> { "MainCharacter", "RightCharacter", "ForwardCharacterDM", "LeftCharacter" };
	// Camera position for the Dice mechanism
	public Node3D DICE_POS;

	public override async void _Ready() {
		await GameManager.Instance.IsLoaded();

		if (cameraMover == null || 
			characterUI == null || 
			diceThrowerMechanism == null) throw new System.Exception("Error: a GameStateMachine component is null");

		// TODO: weird check that isn't appropriate in GameManager but also weird here as it combines both of these....

		// Connecting between the Game Entities and the Camera positions
		if (GameManager.Instance.gameEntities == null || 
			GAME_ENTITIES_POS == null) throw new System.Exception("Error: a GameStateMachine component is null");
		if (GameManager.Instance.gameEntities.Count != GameManager.NUMBER_OF_GAME_ENTITIES) 
			throw new System.Exception("Error: the number of game entities in the game is not correct");
		if (GAME_ENTITIES_POS.Count != GameManager.NUMBER_OF_GAME_ENTITIES)
			throw new System.Exception("Error: the number of game entity positions in the game is not correct");

		for (int i = 0; i < GameManager.Instance.gameEntities.Count; i++) {
			if (GameManager.Instance.gameEntities[i] == null) throw new System.Exception("Error: a character in the game is null");

			GameManager.Instance.gameEntities[i].worldSpacePosition = GetNodeOrNull<Node3D>("%" + GAME_ENTITIES_POS[i]);
			if (GameManager.Instance.gameEntities[i].worldSpacePosition == null)
				throw new System.Exception("Error: a corresponding Node3D position has not been found for " + GAME_ENTITIES_POS[i]);
		}

		// Finding the Dice camera position
		DICE_POS = GetNodeOrNull<Node3D>("%DicePos");
		if (DICE_POS == null) throw new System.Exception("Error: a corresponding Node3D position has not been found for DicePos");

		StartGame();
	}

	public async void StartGame() {
		// Getting the true Total number of rounds in the game from the GameManager
		NumberOfRounds = GameManager.Instance.TotalRounds;

		// Reformatting the personalities of the characters into the shortened form for token conservation
		foreach (Character character in GameManager.Instance.characters) {
			character.ShortenedDescription = await LLMLibrary.PersonalitySummary(character);
		}

		// Starting the game loop
		ChangeState(new StartGame());
	}

	public void ChangeState(GameState newState) {
		CurrentState?.Exit(this);
		CurrentState = newState;
		CurrentState.Enter(this);
	}

	// Returns the next character in the cycle, null if we came to the end of the round
	public Character NextCharacter() {
		int TotalCharacters = GameManager.Instance.characters.Count;
		if (TotalCharacters <= 0) return null;

		if (character_ind >= TotalCharacters) {
			character_ind = 0;
			return null;
		}

		return GameManager.Instance.characters[character_ind++];
	}
}
