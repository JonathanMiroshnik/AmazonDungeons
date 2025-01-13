using Godot;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

public partial class EndUI : Node
{
    public RichTextLabel RTC;

    public override async void _Ready() {
        RTC = GetNodeOrNull<RichTextLabel>("RichTextLabel");
        if (RTC == null) throw new Exception("RichTextLabel not found");

        await GameManager.Instance.IsLoaded();

        var g = await LLMLibrary.GameSummaryJSON();

        var JSON = GlobalStringLibrary.JSONStringBrackets(g);
        GD.Print(g);
		GD.Print(JsonConvert.DeserializeObject<JSONSong>(JSON));

        GD.Print("wew2");
        string result = await LLMLibrary.GameSeparatedSummary();

        GD.Print("wew3");
        GD.Print("HD \n" + result);
        RTC.Text = result;
    }
}
