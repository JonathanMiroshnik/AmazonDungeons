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
        dialogueStateMachine.gameStateMachine.characterUI.dialogueStateMachine = dialogueStateMachine;
    }

    public async void Enter(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Enter StartDialogue");

        // Creating the conversation between the DM and the character
		DMCharacterResponse resp = new DMCharacterResponse(); // TODO: very bad code and structure
		resp.responderGameEntity = GameManager.Instance.DungeonMaster;
		resp.respondeeGameEntity = dialogueStateMachine.gameStateMachine.character;

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

    public DiceThrowing(DMCharacterResponse curDMCharacterResponse) {
        if (curDMCharacterResponse == null) throw new ArgumentException("Given DM Character Response for Dice Throwing is null");
        dmCharacterResponse = curDMCharacterResponse;
    }

    public async void Enter(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Enter DiceThrowing");

        // Moving over to the dice
        await dialogueStateMachine.gameStateMachine.cameraMover.MoveCameraByNode3D(dialogueStateMachine.gameStateMachine.DICE_POS, GameStateMachine.TIME_TO_MOVE);
        await Action(dialogueStateMachine);
    }

    public async void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit DiceThrowing");
    }

    public async Task Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action DiceThrowing");
        
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

        await dialogueStateMachine.gameStateMachine.characterUI.AddResponse(text, dmCharacterResponse.responderGameEntity, false);

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
    }

    private void DefaultChangeState(DialogueStateMachine dialogueStateMachine) {
        dialogueStateMachine.ChangeState(new EndDialogue());
    }
}

public partial class EndDialogue : DialogueState {
    public async void Enter(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Enter EndDialogue");
    }

    public void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit EndDialogue");

        dialogueStateMachine.gameStateMachine.ChangeState(new DMDialogue());
    }

    public async Task Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action EndDialogue");

        dialogueStateMachine.gameStateMachine.characterUI.nextContainer.Visible = false; // FIXME: too long!!

        Exit(dialogueStateMachine);
    }
}

public partial class DialogueStateMachine : GodotObject
{
    public DialogueState CurrentState { get; private set; }
    public GameStateMachine gameStateMachine { get; set; }

    public void ChangeState(DialogueState newState) {
        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
    }
}
