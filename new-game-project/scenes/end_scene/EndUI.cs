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
		GD.Print(JsonConvert.DeserializeObject<JSONSong>(JSON)); // TODO: delete
        string result = await LLMLibrary.GameSeparatedSummary();
        RTC.Text = result;
    }
}
