using Godot;
using System;

public partial class JSONRiskAction : GodotObject
{
    public string risk { get; set; } 
    public string dice { get; set; }
    
    public bool isRisk () {
        Boolean myBool = false;

        Boolean.TryParse(risk , out myBool);

        return myBool;
    }

    public int getDice () {
        int myInt = -1;
        Int32.TryParse(dice, out myInt);

        return myInt;
    }
}
