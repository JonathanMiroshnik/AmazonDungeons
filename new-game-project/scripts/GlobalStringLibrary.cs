using Godot;
using System.Threading.Tasks;
using System;

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

    public static int NumberOfWords(string inputStr) {
        char[] delimiters = new char[] {' ', '\r', '\n' };
        return inputStr.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
