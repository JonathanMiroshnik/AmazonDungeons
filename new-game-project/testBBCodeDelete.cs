using Godot;
using System;

public partial class testBBCodeDelete : Node
{
	[Export] public RichTextLabel richTextLabel;

	public void _on_pressed() {
		if (GameManager.Instance.testStr == "") return;

		richTextLabel.Text = GameManager.Instance.testStr;
	}
}
