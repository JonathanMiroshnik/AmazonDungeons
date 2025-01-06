using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface GameState
{
	public abstract void Enter(GameStateMachine gameStateMachine);
	public abstract void Exit(GameStateMachine gameStateMachine);
	public abstract void Action(GameStateMachine gameStateMachine);
}

// FIXME: ChangeState should nto be used by states, only outside of them

public partial class StartGame : GameState {
	public void Enter(GameStateMachine gameStateMachine) {
		GD.Print("Enter StartGame");

		Action(gameStateMachine);
		gameStateMachine.ChangeState(new DMDialogue(gameStateMachine.NextCharacter()));
	}

	public void Exit(GameStateMachine gameStateMachine) {
		GD.Print("Exit StartGame");
	}
	public void Action(GameStateMachine gameStateMachine) {
		GD.Print("Action StartGame");

		DoPrelude();
	}

	public void DoPrelude() {
		GD.Print("Describing the world....\n\n");
		GD.Print(GameManager.Instance.worldDesc.world + "\n\n");

		GD.Print("Describing the characters....\n\n");
		foreach (GameEntity curGameEntity in GameManager.Instance.gameEntities) {
			if (curGameEntity is not Character) continue;
			Character curCharDown = (Character) curGameEntity;

			GD.Print(curCharDown.ShortenedDescription + "\n");
		}
	}
}
 
 
 public partial class EndGame : GameState {
	public void Enter(GameStateMachine gameStateMachine) {
		GD.Print("Enter EndGame");

		Action(gameStateMachine);
	}

	public void Exit(GameStateMachine gameStateMachine) {
		GD.Print("Exit EndGame");
	}
	public void Action(GameStateMachine gameStateMachine) {
		GD.Print("Action EndGame");

		// TODO: make epic poem from game, display it
	}
}


public partial class DMDialogue : GameState {
	public Character character { get; set; }
	public DialogueStateMachine dialogueStateMachine = null;

	public DMDialogue(Character curCharacter) {
		if (curCharacter == null) {
			GD.Print("problem"); // FIXME: raise exepction
		}
		character = curCharacter;
	}

	public void Enter(GameStateMachine gameStateMachine) {
		GD.Print("Enter DMDialogue");

		dialogueStateMachine = new DialogueStateMachine(gameStateMachine, character);
		Action(gameStateMachine);
	}

	public void Exit(GameStateMachine gameStateMachine) {
		GD.Print("Exit DMDialogue");

		Character character = gameStateMachine.NextCharacter();
		if (character == null) {
			if (gameStateMachine.CurrentRound+1 >= gameStateMachine.NumberOfRounds) {
				gameStateMachine.ChangeState(new EndGame());
				return;
			} else {
				gameStateMachine.CurrentRound++;
				gameStateMachine.ChangeState(new DMDialogue(character));
				return;
			}
		}
	}

	public async void Action(GameStateMachine gameStateMachine) {
		GD.Print("Action DMDialogue");
		if (dialogueStateMachine == null) {
			gameStateMachine.ChangeState(new EndGame());
			return;
		}
		if (character == null) return;

		await gameStateMachine.cameraMover.MoveCameraByNode3D(character.worldSpacePosition, GameStateMachine.TIME_TO_MOVE); // FIXME: magic number
		dialogueStateMachine.ChangeState(new StartDialogue(gameStateMachine, dialogueStateMachine));
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
	[Export]
	public int NumberOfRounds = 1;
	public int CurrentRound = 0;

	// public Character character { get; set; }

	// Camera positions relating to GameEntities, Character and other relevant objects
	private List<string> GAME_ENTITIES_POS = new List<string> { "MainCharacter", "RightCharacter", "ForwardCharacterDM", "LeftCharacter" };
	public Node3D DICE_POS;

	public override async void _Ready() {
		await GameManager.Instance.IsLoaded();
		// GameManager.Instance.RegisterGame(this);

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

		StartGame();
	}

	public void StartGame() {
		ChangeState(new StartGame());
	}

	public void ChangeState(GameState newState) {
		CurrentState?.Exit(this);
		CurrentState = newState;
		CurrentState.Enter(this);
	}

	public Character NextCharacter(Character curCharacter = null) {
		int TotalCharacters = GameManager.Instance.characters.Count;
		if (TotalCharacters <= 0) return null;
		if (curCharacter == null) return GameManager.Instance.characters[0];

		Character character = curCharacter;

		for (int i = 0; i < TotalCharacters; i++) {
			Character checkChar = GameManager.Instance.characters[i];
			if (checkChar != character) continue;

			if (i+1 < TotalCharacters) {
				character = GameManager.Instance.characters[i+1];
				break;
			} else {
				character = null;
				break;
			}
		}

		return character;
	}
}
