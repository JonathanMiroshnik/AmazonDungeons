using Godot;

// Importing everything from other file test.cs
using System.Text.Json; 
using System.IO;


public partial class Player : Node
{
    // public async override void _Ready()
    // {
    //     // Calling AskLlama in testName
    //     string response = await test.AskLlama("Return a descriptiong of a warrior in JSON format. Do not write anything other than the JSON.");

    //     // get response from async operation AskLlama
    //     // GD.Print(response);

    //     // Writing JSON file of response
    //     File.WriteAllText("test.json", response);

    //     // Opening and finding name of character
    //     string jsonFilePath = "test.json"; // Path to your JSON file
    //     string categoryNameToAccess = "Name";

    //     // Read the JSON file
    //     string jsonContent = File.ReadAllText(jsonFilePath);

    //     // Parse the JSON content
    //     using JsonDocument document = JsonDocument.Parse(jsonContent);
    //     JsonElement root = document.RootElement;

    //     // Access a category by its name
    //     if (root.TryGetProperty(categoryNameToAccess, out JsonElement category))
    //     {
    //         GD.Print($"Name '{category}' found.");
    //     }
    //     else
    //     {
    //         GD.Print($"Name '{categoryNameToAccess}' not found.");
    //     }
    // }
}
