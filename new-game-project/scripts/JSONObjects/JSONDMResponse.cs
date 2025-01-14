using Godot;
using System;

public partial class JSONDMResponse : Node
{
	public string text { get; set; }
	public int score { get; set; }

	public JSONDMResponse() {
		text = "";
		score = 0;
	}
}
