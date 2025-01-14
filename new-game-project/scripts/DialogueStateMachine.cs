using Godot;
using System;
using System.Threading.Tasks;

public interface DialogueState
{
	public abstract void Enter(DialogueStateMachine dialogueStateMachine);
	public abstract void Exit(DialogueStateMachine dialogueStateMachine);
	public abstract Task Action(DialogueStateMachine dialogueStateMachine);
}

public partial class StartDialogue : DialogueState {
	public StartDialogue(GameStateMachine curGameStateMachine, DialogueStateMachine dialogueStateMachine) {
		if (curGameStateMachine == null) {
			GD.Print("Error: GameStateMachine is null");
			return;
		}

		dialogueStateMachine.gameStateMachine = curGameStateMachine;
		dialogueStateMachine.gameStateMachine.characterUI.ClearResponses();
		dialogueStateMachine.gameStateMachine.characterUI.Visible = true;

		// Change HP in accordance with result in End dialogue response
		dialogueStateMachine.gameStateMachine.characterUI.SetHP(dialogueStateMachine.character.HealthPoints);

		dialogueStateMachine.gameStateMachine.characterUI.dialogueStateMachine = dialogueStateMachine;
	}

	public async void Enter(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Enter StartDialogue");

		// Creating the conversation between the DM and the character
		DMCharacterResponse resp = new DMCharacterResponse(); // TODO: very bad code and structure
		resp.responderGameEntity = GameManager.Instance.DungeonMaster;
		resp.respondeeGameEntity = dialogueStateMachine.character;

		dialogueStateMachine.gameStateMachine.characterUI.curDMResponse = resp;

		string text = await LLMLibrary.DM_response_summary((Character) resp.respondeeGameEntity);
		await dialogueStateMachine.gameStateMachine.characterUI.AddResponse(text, resp.responderGameEntity, false);
		dialogueStateMachine.gameStateMachine.characterUI.ReplyToggle(true);

		dialogueStateMachine.ChangeState(new ResponseDialogue(resp));
	}

	public void Exit(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Exit StartDialogue");
	}
	public async Task Action(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Action StartDialogue");
	}
}

public partial class DiceThrowing : DialogueState {
	public Character character = null;
	public DMCharacterResponse dmCharacterResponse;
	public int numOfAttemptDice = 1;
	public int numOfWinningDice = 1;

	public DiceThrowing(DMCharacterResponse curDMCharacterResponse, int num_of_attempt_dice = 1, int num_of_victor_dice = 1) { // TODO: add number of dice to throw/win
		if (curDMCharacterResponse == null) throw new ArgumentException("Given DM Character Response for Dice Throwing is null");
		numOfWinningDice = num_of_victor_dice;
		numOfAttemptDice = num_of_attempt_dice;
		dmCharacterResponse = curDMCharacterResponse;
	}

	public void Enter(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Enter DiceThrowing");
	}

	public void Exit(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Exit DiceThrowing");
	}

	public async Task Action(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Action DiceThrowing");

		 // Moving over to the dice
		await dialogueStateMachine.gameStateMachine.cameraMover.MoveCameraByNode3D(dialogueStateMachine.gameStateMachine.DICE_POS, GameStateMachine.TIME_TO_MOVE);

		// Indicating the start of the dice rolling
		// dialogueStateMachine.gameStateMachine.characterUI.diceDescContainer.Visible = true; // TODO: delete?

		// Throwing the dice
		dmCharacterResponse.ThrownDiceSuccess = await dialogueStateMachine.gameStateMachine.diceThrowerMechanism.PlayDice(numOfAttemptDice, numOfWinningDice); // FIXME: number of dice changes
		dmCharacterResponse.ThrownDice = true;

		// Indicating the victory status of the completed dice roll
		if (dmCharacterResponse.ThrownDiceSuccess) {
			dialogueStateMachine.gameStateMachine.characterUI.curFullResponseContainer.AddDiceDescription("You've won this dice roll!");
		}
		else {
			dialogueStateMachine.gameStateMachine.characterUI.curFullResponseContainer.AddDiceDescription("You've lost this dice roll!");
		}

		// Returning to the character that threw the dice
		await dialogueStateMachine.gameStateMachine.cameraMover.MoveCameraByNode3D(dmCharacterResponse.respondeeGameEntity.worldSpacePosition, 
																					GameManager.TIME_TO_MOVE_CAMERA_POSITIONS);

		// TODO: change to public function in CharacterUI, do this for all such long lines that depend on it.
		// Move back to the Character
		dialogueStateMachine.gameStateMachine.characterUI.Visible = true;
		dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = true;

		dialogueStateMachine.ChangeState(new EndDialogue(dmCharacterResponse.responderGameEntity, 
														 dmCharacterResponse.respondeeGameEntity)); // FIXME: should this be here??? not only outside doing such things, changing states?
	}
}

public partial class ResponseDialogue : DialogueState {
	public DMCharacterResponse dmCharacterResponse;
	public ResponseDialogue(DMCharacterResponse curDMCharacterResponse) {
		if (curDMCharacterResponse == null) throw new ArgumentException("Given DM Character Response for Response Dialogue is null");
		dmCharacterResponse = curDMCharacterResponse;
	}

	public async void Enter(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Enter ResponseDialogue");

		// If the gameEntity is an AI, the responses should show themselves automatically until the end of the dialogue
		if (dmCharacterResponse.respondeeGameEntity.GameEntityType is GameEntityType.AI) {
			string text = await LLMLibrary.AI_character_response(
				(Character) dmCharacterResponse.respondeeGameEntity, dmCharacterResponse.ThrownDice, dmCharacterResponse.ThrownDiceSuccess);
			
			dialogueStateMachine.gameStateMachine.characterUI.replyEdit.Text = text;
			await Action(dialogueStateMachine);
		}
	}

	public void Exit(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Exit ResponseDialogue");
	}

	public async Task Action(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Action ResponseDialogue");

		// We take the reply text input
		string text = dialogueStateMachine.gameStateMachine.characterUI.replyEdit.Text;

		dialogueStateMachine.gameStateMachine.characterUI.replyContainer.Visible = false;
		dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = false;
		await dialogueStateMachine.gameStateMachine.characterUI.AddResponse(text, dmCharacterResponse.respondeeGameEntity, false);

		// FIXME: downcasting bad
		if (dmCharacterResponse.respondeeGameEntity is Character) {	
			Character character = (Character) dmCharacterResponse.respondeeGameEntity;

			CharacterInteraction characterInteraction = new CharacterInteraction();
			characterInteraction.responderGameEntity = dmCharacterResponse.respondeeGameEntity;
			characterInteraction.respondeeGameEntity = dmCharacterResponse.responderGameEntity;
			characterInteraction.text = text;

			character.conversation.Add(characterInteraction);

			JSONRiskAction riskAction = await LLMLibrary.ActionCategorization(character, text); // TODO: delete!
			if (riskAction != null) {
				bool isRisk = riskAction.isRisk();
				int neededDice = riskAction.getDice(); 

				var skillsBools = riskAction.getSkills();
				bool strength = skillsBools.Item1;
				bool reflex = skillsBools.Item2;
				bool intelligence = skillsBools.Item3;

				GD.Print("Risk action: " + isRisk + " Dice: " + neededDice);

				if (isRisk && neededDice > 0) {
					dialogueStateMachine.gameStateMachine.characterUI.diceContainer.Visible = true;

					int diceToThrow = neededDice + character.getBonus(strength, reflex, intelligence);
					dialogueStateMachine.gameStateMachine.characterUI.curFullResponseContainer.AddDiceDescription(
													"Throwing " + diceToThrow + " dice, " + neededDice + " of which must get a score of four or above");
					dialogueStateMachine.ChangeState(new DiceThrowing(dmCharacterResponse, diceToThrow, neededDice));
					return;
				}
			}
		}

		dialogueStateMachine.ChangeState(new EndDialogue(dmCharacterResponse.responderGameEntity, dmCharacterResponse.respondeeGameEntity));
	}
}

public partial class EndDialogue : DialogueState {
	public GameEntity responder;
	public Character respondee;
	public EndDialogue(GameEntity gameEntity, GameEntity character) {
		if (gameEntity == null) throw new ArgumentException("Given GameEntity for EndDialogue is null");
		if (character == null) throw new ArgumentException("Given Character for EndDialogue is null");
		if (character is not Character) throw new ArgumentException("Given Character for EndDialogue is not a Character");
		responder = gameEntity;
		respondee = (Character) character; // FIXME: downcasting bad
	}

	public async void Enter(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Enter EndDialogue");

		dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = false;

		// string text = await LLMLibrary.DM_response_summary(respondee);
		JSONHurtResponse hurtResp = await LLMLibrary.DMHurtReponse(respondee);
		string text = hurtResp.text;
		await dialogueStateMachine.gameStateMachine.characterUI.AddResponse(text, responder, false);

		// Change HP in accordance with result in End dialogue response
		if (hurtResp.getHurt()) {
			respondee.HealthPoints -= hurtResp.getDamage();
		}
		dialogueStateMachine.gameStateMachine.characterUI.SetHP(respondee.HealthPoints);

		if (respondee.GameEntityType is GameEntityType.Player && respondee.HealthPoints <= 0) {
			dialogueStateMachine.gameStateMachine.ChangeState(new EndGame());
			return;
		}

		dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = true;
	}

	public void Exit(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Exit EndDialogue");
	}

	public async Task Action(DialogueStateMachine dialogueStateMachine) {
		// GD.Print("Action EndDialogue");

		dialogueStateMachine.gameStateMachine.characterUI.Visible = false; // FIXME: too long!!
		dialogueStateMachine.gameStateMachine.CurrentState.Action(dialogueStateMachine.gameStateMachine);
	}
}

public partial class DialogueStateMachine : GodotObject
{
	public DialogueState CurrentState { get; private set; }
	public GameStateMachine gameStateMachine { get; set; }
	public Character character { get; set; }

	public DialogueStateMachine(GameStateMachine curGameStateMachine, Character curCharacter) {
		gameStateMachine = curGameStateMachine;
		character = curCharacter;
	}

	public void ChangeState(DialogueState newState) {
		CurrentState?.Exit(this);
		CurrentState = newState;
		CurrentState.Enter(this);
	}
}
