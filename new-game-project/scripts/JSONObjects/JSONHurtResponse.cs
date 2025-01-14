using Godot;
using System;

public partial class JSONHurtResponse : Node
{
    public string text { get; set; }
    public string hurt { get; set; }
    public string damage { get; set; }

    public JSONHurtResponse() {
		text = "";
		hurt = "";
        damage = "";
	}

    public bool getHurt () {
        Boolean ret = false;
        if (!Boolean.TryParse(hurt , out ret)) {
            ret = false;
        }

        return ret;
    }

    public int getDamage () {
        int myInt = -1;
        if (!Int32.TryParse(damage, out myInt)) {
            myInt = -1;
        }

        return myInt;
    }
}
