using Godot;
using System;

public partial class JSONEndSummary : GodotObject
{
    public string text { get; set; }
    public string damage { get; set; }

    public int getDamage() {
        int ret = 0;
        Int32.TryParse(damage, out ret);

        return ret;
    }
}
