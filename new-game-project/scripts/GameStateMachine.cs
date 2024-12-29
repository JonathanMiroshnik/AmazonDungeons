using Godot;
using System;
using System.Threading.Tasks;

public interface GameState
{
	public abstract void Enter(GameStateMachine gameStateMachine);
	public abstract void Exit(GameStateMachine gameStateMachine);
	public abstract void Action(GameStateMachine gameStateMachine);
}

public partial class StartGame : GameState {
	public void Enter(GameStateMachine gameStateMachine) {
		GD.Print("Enter StartGame");

		// TODO: perform game prelude here
		DoPrelude();

		gameStateMachine.NextCharacter();
		gameStateMachine.ChangeState(new DMDialogue());
	}

	public void Exit(GameStateMachine gameStateMachine) {
		GD.Print("Exit StartGame");
	}
	public void Action(GameStateMachine gameStateMachine) {
		GD.Print("Action StartGame");
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
	}

	public void Exit(GameStateMachine gameStateMachine) {
		GD.Print("Exit EndGame");
	}
	public void Action(GameStateMachine gameStateMachine) {
		GD.Print("Action EndGame");
	}
}


public partial class DMDialogue : GameState {
	public DialogueStateMachine dialogueStateMachine = null;

	public void Enter(GameStateMachine gameStateMachine) {
		GD.Print("Enter DMDialogue");

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

		await gameStateMachine.cameraMover.MoveCameraByNode3D(gameStateMachine.character.worldSpacePosition, 2); // FIXME: magic number

		dialogueStateMachine.StartDialogue(gameStateMachine.characterUI);
	}
	
}

public partial class GameStateMachine : Node
{
	public GameState CurrentState { get; private set; }

	[Export]
	public CameraMover cameraMover; // TODO: maybe there should be a CharacterCameraMover and a function MoveToNextCharacter?
	[Export]
	public CharacterUI characterUI;

	public Character character { get; set; }

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

			Character curChar = (Character) curGE;
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
