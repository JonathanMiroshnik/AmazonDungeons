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
	[Export]
	public GlobalAudioLibrary globalAudioLibrary;

	private RichTextLabel RTC;
	private Label songName;

	public override async void _Ready() {
		await GameManager.Instance.IsLoaded();

		if (globalAudioLibrary == null) throw new Exception("globalAudioLibrary is null in EndScreen");

		songName = GetNodeOrNull<Label>("%SongName");
        if (songName == null) throw new Exception("SongName not found");
		RTC = GetNodeOrNull<RichTextLabel>("%SongContent");
        if (RTC == null) throw new Exception("SongContent not found");

        string songJSONified = await LLMLibrary.GameSummaryJSON();

		songJSONified = GlobalStringLibrary.JSONStringBrackets(songJSONified);

		JSONSong result = new JSONSong();
		try {
			result = JsonConvert.DeserializeObject<JSONSong>(songJSONified);
		}
		catch (Exception e) {
			GD.Print(e.Message);
		}

        EnterSongInfo(result.name, result.text);
	}

	public void EnterSongInfo(string songNameIn, string song) {
		songName.Text = "[center]" + songNameIn + "[/center]";
		RTC.Text = song;
	}

	public async void _on_exit_game_button_pressed() {
		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);
		await Task.Delay(200);
		
		GetTree().Quit();
	}

	public async void _on_new_game_button_pressed() {
		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);
		await Task.Delay(200);

		GameManager.Instance._Ready();
		GameManager.Instance.SceneChange("res://scenes/main_menu/menu.tscn");
	}
}
