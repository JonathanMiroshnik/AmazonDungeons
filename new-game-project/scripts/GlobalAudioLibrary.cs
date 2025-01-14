using Amazon.BedrockAgentRuntime;
using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class GlobalAudioLibrary : AudioStreamPlayer
{
    public const string DICE_PATH = "res://Sounds/Dice/";
    public const string WOOSH_PATH = "res://Sounds/Wooshes/";
    public const string BUTTON_PATH = "res://Sounds/Buttons/";

    private Random random = new Random();

    public override void _Ready() {
        random = new Random();
    }

    public bool PlayRandomSound(string path)
    {
        var files = DirAccess.GetFilesAt(path);
        var randomFile = files[random.Next(0, DirContents(path))];
        Stream = AudioStreamOggVorbis.LoadFromFile(path + randomFile);
        Play();

        return true;
    }

    // source: https://docs.godotengine.org/en/4.0/classes/class_diraccess.html#diraccess
    public int DirContents(string path)
    {
        int length = 0;

        using var dir = DirAccess.Open(path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (dir.CurrentIsDir())
                {                
                    // GD.Print($"Found directory: {fileName}");
                }
                else
                {
                    length++;
                    // GD.Print($"Found file: {fileName}");
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.Print("An error occurred when trying to access the path.");
        }

        return length;
    }
}
