using Godot;
using System;
using System.Threading.Tasks;

public interface DialogueState
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Action();
}

public partial class StartDialogue : DialogueState {
    public async void Enter() {
        GD.Print("Enter StartDialogue");
    }

    public void Exit() {
        GD.Print("Exit StartDialogue");
    }
    public void Action() {
        GD.Print("Action StartDialogue");
    }
}

public partial class DiceThrowing : DialogueState {
    public Character character = null;
    public DiceThrowerMechanism diceThrowerMechanism = null;

    public async void Enter() {
        GD.Print("Enter DiceThrowing");
    }

    public void Exit() {
        GD.Print("Exit DiceThrowing");
    }

    public async void Action() {
        GD.Print("Action DiceThrowing");
        int numSuccesses = await diceThrowerMechanism.ThrowDice();
        GD.Print("Dice succeeded " + numSuccesses);
    }
}

public partial class EndDialogue : DialogueState {
    public async void Enter() {
        GD.Print("Enter EndDialogue");
    }

    public void Exit() {
        GD.Print("Exit EndDialogue");
    }
    public void Action() {
        GD.Print("Action EndDialogue");
    }
}

public partial class DialogueStateMachine : GodotObject
{
    public DialogueState CurrentState { get; private set; }
    public CharacterUI characterUI { get; private set; } // TODO: maybe get the GameStateMachine instead of UI?

    public void StartDialogue(CharacterUI curCharacterUI) {
        if (curCharacterUI == null) {
            GD.Print("Error: CharacterUI is null");
            return;
        }

        characterUI = curCharacterUI;
        characterUI.ClearResponses();

        ChangeState(new StartDialogue());
    }

    public void ChangeState(DialogueState newState) {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
