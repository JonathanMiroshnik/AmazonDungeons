using Godot;
using System;
using System.Threading.Tasks;

public interface DialogueState
{
    public abstract void Enter(DialogueStateMachine dialogueStateMachine);
    public abstract void Exit(DialogueStateMachine dialogueStateMachine);
    public abstract void Action(DialogueStateMachine dialogueStateMachine);
}

public partial class StartDialogue : DialogueState {
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
    public void Action(DialogueStateMachine dialogueStateMachine) {
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

        await dialogueStateMachine.gameStateMachine.cameraMover.MoveCameraByNode3D(dialogueStateMachine.gameStateMachine.DICE_POS, GameStateMachine.TIME_TO_MOVE);
        Action(dialogueStateMachine);
    }

    public async void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit DiceThrowing");

        await dialogueStateMachine.gameStateMachine.cameraMover.MoveCameraByNode3D(dmCharacterResponse.respondeeGameEntity.worldSpacePosition, 
																		            GameManager.TIME_TO_MOVE_CAMERA_POSITIONS);
    }

    public async void Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action DiceThrowing");
        
        dmCharacterResponse.ThrownDiceSuccess = await diceThrowerMechanism.PlayDice(6, 5); // FIXME: number of dice changes
        dmCharacterResponse.ThrownDice = true;
    }
}

public partial class EndDialogue : DialogueState {
    public async void Enter(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Enter EndDialogue");
    }

    public void Exit(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Exit EndDialogue");
    }
    public void Action(DialogueStateMachine dialogueStateMachine) {
        GD.Print("Action EndDialogue");
    }
}

public partial class DialogueStateMachine : GodotObject
{
    public DialogueState CurrentState { get; private set; }
    public GameStateMachine gameStateMachine { get; private set; }

    public void StartDialogue(GameStateMachine curGameStateMachine) {
        if (curGameStateMachine == null) {
            GD.Print("Error: GameStateMachine is null");
            return;
        }

        gameStateMachine = curGameStateMachine;
        gameStateMachine.characterUI.ClearResponses();
        gameStateMachine.characterUI.dialogueStateMachine = this;

        ChangeState(new StartDialogue());
    }

    public void ChangeState(DialogueState newState) {
        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
    }
}
