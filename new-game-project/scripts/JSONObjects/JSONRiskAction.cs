using Amazon.BedrockAgentRuntime.Model;
using Godot;
using System;

public partial class JSONRiskAction : GodotObject
{
    public string risk { get; set; } 
    public string dice { get; set; }
    public string strengh { get; set; }
    public string reflex { get; set; }
    public string intelligence { get; set; }
    
    public bool isRisk () {
        Boolean myBool = false;

        Boolean.TryParse(risk , out myBool);

        return myBool;
    }

    public Tuple<bool, bool, bool> getSkills () {
        Boolean strengthBool = false;
        Boolean.TryParse(strengh , out strengthBool);

        Boolean reflexBool = false;
        Boolean.TryParse(reflex , out reflexBool);

        Boolean intelligenceBool = false;
        Boolean.TryParse(intelligence , out intelligenceBool);

        return Tuple.Create<bool,bool,bool>(strengthBool, reflexBool, intelligenceBool);
    }

    public int getDice () {
        int myInt = -1;
        Int32.TryParse(dice, out myInt);

        return myInt;
    }
}
