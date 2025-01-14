using Amazon.BedrockAgentRuntime;
using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class GlobalAudioLibrary : AudioStreamPlayer
{
    public const string DICE_PATH = "res://Sounds/Dice/";
    public const string WOOSH_PATH = "res://Sounds/Wooshes/";
    public const string BUTTON_PATH = "res://Sounds/Buttons/";

    public const string TEST = "res://Sounds/Ambiant/Endgame/";

    private Random random = new Random();

    public override void _Ready() {
        random = new Random();
    }

    public bool PlayRandomSound(string path)
    {
        var files = DirAccess.GetFilesAt(path);
        List<string> oggFiles = new List<string>();
        foreach (var file in files) {
            if (file.EndsWith(".ogg")) {
                oggFiles.Add(file);
            }
        }

        if (oggFiles.Count <= 0) return false;

        var randomFile = oggFiles[random.Next(0, oggFiles.Count)];
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
