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
        dialogueStateMachine.gameStateMachine.characterUI.dialogueStateMachine = dialogueStateMachine;
    }

    public async void Enter(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Enter StartDialogue");

        // Creating the conversation between the DM and the character
		DMCharacterResponse resp = new DMCharacterResponse(); // TODO: very bad code and structure
		resp.responderGameEntity = GameManager.Instance.DungeonMaster;
		resp.respondeeGameEntity = dialogueStateMachine.character;

		await dialogueStateMachine.gameStateMachine.characterUI.DoDMConversation(resp);
    }

    public void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit StartDialogue");
    }
    public async Task Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action StartDialogue");
    }
}

public partial class DiceThrowing : DialogueState {
    public Character character = null;
    public DiceThrowerMechanism diceThrowerMechanism = null;
    public DMCharacterResponse dmCharacterResponse;

    public DiceThrowing(DMCharacterResponse curDMCharacterResponse) { // TODO: add number of dice to throw/win
        if (curDMCharacterResponse == null) throw new ArgumentException("Given DM Character Response for Dice Throwing is null");
        dmCharacterResponse = curDMCharacterResponse;
    }

    public void Enter(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Enter DiceThrowing");
    }

    public void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit DiceThrowing");
    }

    public async Task Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action DiceThrowing");
        
         // Moving over to the dice
        await dialogueStateMachine.gameStateMachine.cameraMover.MoveCameraByNode3D(dialogueStateMachine.gameStateMachine.DICE_POS, GameStateMachine.TIME_TO_MOVE);

        // Throwing the dice
        dmCharacterResponse.ThrownDiceSuccess = await diceThrowerMechanism.PlayDice(6, 5); // FIXME: number of dice changes
        dmCharacterResponse.ThrownDice = true;

        // Returning to the character that threw the dice
        await dialogueStateMachine.gameStateMachine.cameraMover.MoveCameraByNode3D(dmCharacterResponse.respondeeGameEntity.worldSpacePosition, 
																		            GameManager.TIME_TO_MOVE_CAMERA_POSITIONS);

        // TODO: change to public function in CharacterUI, do this for all such long lines that depend on it.
        // Move back to the Character
		dialogueStateMachine.gameStateMachine.characterUI.Visible = true;
		dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = true;

        dialogueStateMachine.ChangeState(new ResponseDialogue(dmCharacterResponse)); // FIXME: should this be here??? not only outside doing such things, changing states?
    }
}

public partial class ResponseDialogue : DialogueState {
    public DMCharacterResponse dmCharacterResponse;
    public ResponseDialogue(DMCharacterResponse curDMCharacterResponse) {
        if (curDMCharacterResponse == null) throw new ArgumentException("Given DM Character Response for Response Dialogue is null");
        dmCharacterResponse = curDMCharacterResponse;
    }

    public async void Enter(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Enter ResponseDialogue");

        if (dmCharacterResponse.respondeeGameEntity.GameEntityType is GameEntityType.AI) {
            string text = await LLMLibrary.AI_character_response(
                (Character) dmCharacterResponse.respondeeGameEntity, dmCharacterResponse.ThrownDice, dmCharacterResponse.ThrownDiceSuccess);
            
            dialogueStateMachine.gameStateMachine.characterUI.replyEdit.Text = text;
            dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = true;
        }
    }

    public void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit ResponseDialogue");
    }

    public async Task Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action ResponseDialogue");

        // We take the reply text input
        string text = dialogueStateMachine.gameStateMachine.characterUI.replyEdit.Text;
        if (text.Length <= 0) {
            DefaultChangeState(dialogueStateMachine);
            return;
        }

        await dialogueStateMachine.gameStateMachine.characterUI.AddResponse(text, dmCharacterResponse.respondeeGameEntity, false);

        // FIXME: downcasting bad
		if (dmCharacterResponse.respondeeGameEntity is Character) {	
			Character character = (Character) dmCharacterResponse.respondeeGameEntity;

			CharacterInteraction characterInteraction = new CharacterInteraction();
			characterInteraction.responderGameEntity = dmCharacterResponse.respondeeGameEntity;
			characterInteraction.respondeeGameEntity = dmCharacterResponse.responderGameEntity;
			characterInteraction.text = text;

			character.conversation.Add(characterInteraction);
		}

        // After showing the summary response, we switch to the final dialogue
        DefaultChangeState(dialogueStateMachine);

        dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = true;
    }

    private void DefaultChangeState(DialogueStateMachine dialogueStateMachine) {
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
        GD.Print("Enter EndDialogue");

        string text = await LLMLibrary.DM_response_summary(respondee);
        await dialogueStateMachine.gameStateMachine.characterUI.AddResponse(text, responder, false);
    }

    public void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit EndDialogue");
    }

    public async Task Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action EndDialogue");

        dialogueStateMachine.gameStateMachine.characterUI.Visible = false; // FIXME: too long!!
        dialogueStateMachine.gameStateMachine.ChangeState(new DMDialogue(dialogueStateMachine.gameStateMachine.NextCharacter(dialogueStateMachine.character)));
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
