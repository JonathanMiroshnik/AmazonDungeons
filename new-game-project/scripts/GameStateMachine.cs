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
		gameStateMachine.ChangeState(new DMDialogue());
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
	public DialogueStateMachine dialogueStateMachine = null;

	public void Enter(GameStateMachine gameStateMachine) {
		GD.Print("Enter DMDialogue");

		if (dialogueStateMachine == null) {
			gameStateMachine.NextCharacter();
		}

		dialogueStateMachine = new DialogueStateMachine();
		Action(gameStateMachine);
	}

	public void Exit(GameStateMachine gameStateMachine) {
		GD.Print("Exit DMDialogue");

		Character character = gameStateMachine.NextCharacter();
		if (character == null) {
			gameStateMachine.ChangeState(new EndGame());
			return;
		}

		gameStateMachine.ChangeState(new DMDialogue());
	}

	public async void Action(GameStateMachine gameStateMachine) {
		GD.Print("Action DMDialogue");
		if (dialogueStateMachine == null) {
			gameStateMachine.ChangeState(new EndGame());
			return;
		}

		await gameStateMachine.cameraMover.MoveCameraByNode3D(gameStateMachine.character.worldSpacePosition, GameStateMachine.TIME_TO_MOVE); // FIXME: magic number
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

	public Character character { get; set; }

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

	public Character NextCharacter() {
		int TotalGameEntities = GameManager.Instance.gameEntities.Count;
		if (TotalGameEntities <= 0) {
			return null;
		}

		if (character == null) {
			character = (Character)GameManager.Instance.gameEntities[0];
			return character;
		}

		for (int i = 0; i < TotalGameEntities; i++) {
			GameEntity curGE = GameManager.Instance.gameEntities[i];
			if (curGE is not Character) continue;

			Character curChar = (Character) curGE; // FIXME: downcasting bad
			if (curChar != character) continue;

			if (i+1 < TotalGameEntities) {
				character = (Character)GameManager.Instance.gameEntities[i+1];
				return character;

			} else {
				return null;
			}
		}

		return null;
	}
}
