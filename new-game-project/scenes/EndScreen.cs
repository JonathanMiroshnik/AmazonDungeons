using Godot;
using System;

using System.Text.Json.Nodes;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class EndScreen : Control
{
	private RichTextLabel RTC;
	private Label songName;

	public override async void _Ready() {
		await GameManager.Instance.IsLoaded();

		songName = GetNodeOrNull<Label>("%SongName");
        if (songName == null) throw new Exception("SongName not found");
		RTC = GetNodeOrNull<RichTextLabel>("%SongContent");
        if (RTC == null) throw new Exception("SongContent not found");

        string songJSONified = await LLMLibrary.GameSummaryJSON();
		GD.Print(songJSONified); // TODO: delete

		songJSONified = GlobalStringLibrary.JSONStringBrackets(songJSONified);
		JSONSong songJSON = JsonConvert.DeserializeObject<JSONSong>(songJSONified);

        EnterSongInfo(songJSON.name, songJSON.text);
	}

	public void EnterSongInfo(string songNameIn, string song) {
		songName.Text = "[center]" + songNameIn + "[/center]";
		RTC.Text = song;
	}

	public void _on_exit_game_button_pressed() {
		GetTree().Quit();
	}

	public void _on_new_game_button_pressed() {
		GameManager.Instance._Ready();
		GameManager.Instance.SceneChange("res://scenes/main_menu/menu.tscn");
	}
}
