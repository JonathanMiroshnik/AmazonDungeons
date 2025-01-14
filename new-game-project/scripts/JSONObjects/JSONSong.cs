using Amazon.BedrockAgent.Model;
using Godot;
using System;

public partial class JSONSong : Node
{
    public string text { get; set; }
    public string name { get; set; }

    public JSONSong() {
		text = "";
		name = "";
	}
}
