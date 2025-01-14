using Godot;
using System;

using System.Text.Json.Nodes;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;

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

		songJSONified = GlobalStringLibrary.JSONStringBrackets(songJSONified);
		JSONSong songJSON = JsonConvert.DeserializeObject<JSONSong>(songJSONified);

        EnterSongInfo(songJSON.name, songJSON.text);
	}

	public void EnterSongInfo(string songNameIn, string song) {
		songName.Text = "[center]" + songNameIn + "[/center]";
		RTC.Text = song;
	}

	public async void _on_exit_game_button_pressed() {
		GetNode<GlobalAudioLibrary>("AudioStreamPlayer")?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);
		await Task.Delay(200);
		
		GetTree().Quit();
	}

	public async void _on_new_game_button_pressed() {
		GetNode<GlobalAudioLibrary>("AudioStreamPlayer")?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);
		await Task.Delay(200);

		GameManager.Instance._Ready();
		GameManager.Instance.SceneChange("res://scenes/main_menu/menu.tscn");
	}
}
