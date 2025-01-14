using Godot;
using System;

public partial class JSONWorld : GodotObject
{
    public string world { get; set; } 
    public string location { get; set; }

    public JSONWorld() {
		world = "";
		location = "";
	}
}
