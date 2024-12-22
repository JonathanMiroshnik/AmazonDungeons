using Godot;
using System.Threading.Tasks;
using System;
using System.Text;

// TODO: rename library to GlobalGeneralLibrary
public partial class GlobalStringLibrary : GodotObject
{
	// async function that takes string and label or RichTextLabel and a duration and increases the visible ratio from 0 to 1 over that duration // Amazon Q
	public static async Task TypeWriteOverDuration(string text, Label label, float duration)
	{
		label.VisibleRatio = 0;
		label.Text = text;
		float time = 0;
		while (time < duration)
		{
			label.VisibleRatio = time / duration;
			time += 0.01f;
			await Task.Delay(10);
		}
		label.VisibleRatio = 1;
	}

	public static async Task TypeWriteOverDuration(string text, RichTextLabel label, float duration)
	{
		label.VisibleRatio = 0;
		label.Text = text;
		float time = 0;
		while (time < duration)
		{
			label.VisibleRatio = time / duration;
			time += 0.01f;
			await Task.Delay(10);
		}
		label.VisibleRatio = 1;
	}

	/// <summary>
	/// Returns the number of words in the Input string
	/// </summary>
	/// <param name="inputStr">String to find number of words of.</param>
	/// <returns>Number of words in input string</returns>
	public static int NumberOfWords(string inputStr) {
		char[] delimiters = new char[] {' ', '\r', '\n' };
		return inputStr.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
	}

	/// <summary>
	/// Cleans a JSON string that was given as output from an LLM
	/// </summary>
	/// <param name="inputStr">Input string</param>
	/// <returns>Cleaned up string</returns>
	public static string CleanJSONString(string inputStr) { // TODO: add check for {} brackets for valid JSON check
		byte[] bytes = Encoding.Default.GetBytes(inputStr);
		inputStr = Encoding.UTF8.GetString(bytes);
		inputStr = inputStr.Trim(new char[] { '\uFEFF', '\u200B' });

		return inputStr;
	}

	public static string JSONStringBrackets(string JSONString) {
		if (JSONString == null) return "";
		if (JSONString.Length <= 0) return "";

		// Checks if the string is missing a final closign curly bracket, if it does, it adds it and returns the final string
		// Checking the final character that isn't a space or a tab
		while (JSONString[JSONString.Length - 1] == ' ' || JSONString[JSONString.Length - 1] == '\t') {
			JSONString = JSONString.Remove(JSONString.Length - 1);
		}
		if (JSONString[JSONString.Length - 1] != '}') {
			JSONString += "}";
		}
		return JSONString;
	}
}
