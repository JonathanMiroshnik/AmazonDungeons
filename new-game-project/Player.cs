using Godot;
using System;

// Importing everything from other file test.cs
using testName;

//import async
using System.Threading.Tasks;


public partial class Player : Node
{
    // do ready
    public async override void _Ready()
    {
        // Calling AskLlama in testName
        await test.AskLlama("hello");
    }
}
