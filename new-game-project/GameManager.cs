using Godot;
using System;
using System.Threading.Tasks;
using System.IO;

using Amazon;
using System.Text.Json.Nodes;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Collections.Generic;
using Newtonsoft.Json;


public enum CoreSkill
{
	Strength,
	Reflex,
	Intelligence
}

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public bool Loaded = false;

	// The game that is currently running
	public Game game;

	public List<GameEntity> gameEntities;

	// Represents the Dungeon Master for the game, of which there is only one per game
	public GameEntity DungeonMaster = new GameEntity("Dungeon Master", GameEntityType.DungeonMaster);

	// Description of the DnD-like world
	public JSONWorld worldDesc;

	// Total numbers of characters in the game
	public const int NUMBER_OF_AI_CHARACTERS = 2;
	public const int NUMBER_OF_DMS = 1;
	public const int NUMBER_OF_PLAYERS = 1;
	
	public const int NUMBER_OF_CHARACTERS = NUMBER_OF_AI_CHARACTERS + NUMBER_OF_PLAYERS;
	public const int NUMBER_OF_GAME_ENTITIES = NUMBER_OF_AI_CHARACTERS + NUMBER_OF_DMS + NUMBER_OF_PLAYERS;

	// Other constants
	public const float TIME_TO_MOVE_CAMERA_POSITIONS = 2f;

	[Signal]
	public delegate void NextActionEventHandler();


	public override async void _Ready()
	{
		Instance = this;

		// TODO: DELETE THIS CODE, TEST CODE
		gameEntities = new List<GameEntity>();

		// Creating the world
		worldDesc = await CreateWorldDesc();

		// Creating Main Player character
		gameEntities.Add(new Character("Player", "write personality here", "write shortened description here", 0, 0, 0, GameEntityType.Player));

		// Creating the AI characters
		// for (int i = 0; i < NUMBER_OF_AI_CHARACTERS; i++) { // FIXME:
		// 	await CreateGameCharacter();
		// }

		await CreateGameCharacter();

		// Create new dungeon master
		DungeonMaster = new GameEntity("Dungeon Master", GameEntityType.DungeonMaster);
		gameEntities.Add(DungeonMaster);

		await CreateGameCharacter();

		GD.Print("EW " + gameEntities.Count);
		
		// IMPORTANT, must be kept at the end of this Ready function 
		//  because other parts of the game rely on it through the IsLoaded function
		Loaded = true;
	}

	public async Task<JSONWorld> CreateWorldDesc()
	{
		// Creating a fictionary DnD-like world description
		string worldSerializedJSON = await AskLlama("Write a description Dungeons and Dragons world, " + 
										  "with different locations and small bits of lore\n " + 
										  "Write it as a JSON with only two categories. " +
										  "The first category is world, which contains the description of the world and "+
										  "the second is location, which contains a very short description(or just name) of the place the characters are placed in the world.\n" +
										  "respond only with the JSON and with nothing else." +
										  "The input in the categories are only string, not lists, not anything else.\n" + // FIXME: problems with this depth thing
										  "Don't write something long but make sure that the JSON is valid and closed properly.",
										  0.5f, 1000);
		worldSerializedJSON = GlobalStringLibrary.JSONStringBrackets(worldSerializedJSON);
		GD.Print(worldSerializedJSON);

		return JsonConvert.DeserializeObject<JSONWorld>(worldSerializedJSON);
	}

	public async Task<Character> CreateGameCharacter() {
		// create new character
		string personalityTest = await AskLlama("Write a Dungeons and Dragons character description.\n " + 
												LLMLibrary.JSON_CHARACTER_CREATION_TYPE +
												"The input in the categories are only string, not lists, not anything else.\n" +
												"Don't write something long but make sure that the JSON is valid and closed properly.",
												0.5f, 1000);
		personalityTest = GlobalStringLibrary.JSONStringBrackets(personalityTest);
		GD.Print(personalityTest);

		var resultChar = JsonConvert.DeserializeObject<JSONCharacter>(personalityTest);

		var charName = resultChar.name;
		var personality = resultChar.personality;
		var shortDesc = resultChar.shortdesc;
		
		Character character = new Character(charName, personality, shortDesc, 2);
		gameEntities.Add(character);

		return character;
	}
	
	// Used by parts of the game that need the GameManager to be loaded before they begin their activation
	public async Task<bool> IsLoaded() {
		while (!Loaded) {
			// GD.Print("Waiting for GameManager to load...");
			await Task.Delay(1000);
		}

		return true;
	}

	// Makes the given game the current game that is effected by the actions of the GameManager
	public void RegisterGame(Game game) {
		this.game = game;
		this.NextAction += game.NextAction;
	}

	// Called by certain buttons and hotkeys in the game to indicate to the Game to continue on to the next action in line
	public void PerformNextAction() {
		EmitSignal(SignalName.NextAction);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest) {
			GD.Print();
			GD.Print();
			GD.Print("Total number of input LLM tokens used for this game: " + LLMLibrary.TotalInputTokens.ToString());
			GD.Print("Total number of output LLM tokens used for this game: " + LLMLibrary.TotalOutputTokens.ToString());
			GD.Print("Total number of LLM tokens(input and output string words) used for this game: " + 
			(LLMLibrary.TotalInputTokens + LLMLibrary.TotalOutputTokens).ToString());
			GD.Print("Total number of requests to the LLM: " + LLMLibrary.TotalNumberOfRequests.ToString());
			GD.Print("Average number of tokens per request: " + 
						((float)(LLMLibrary.TotalInputTokens + LLMLibrary.TotalOutputTokens / LLMLibrary.TotalNumberOfRequests)).ToString());
			GD.Print();
			GD.Print();
		}
	}

	public static async Task<string> AskLlama(string prompt, float temperature = 0.5f, int max_gen_len = 500)
	{
		// return "";

		// GD.Print("Ask Llama began");

		//-------------------
		// 1. Configuration
		//-------------------

		// Specify an AWS region
		// Note: Make sure Bedrock is supported in your chosen region
		var region = RegionEndpoint.USEast1;

		// Choose your model ID. Supported models can be found at:
		// https://docs.aws.amazon.com/bedrock/latest/userguide/conversation-inference-supported-models-features.html
		const string modelId = "meta.llama3-8b-instruct-v1:0";

		//-------------------
		// 2. Client Setup
		//-------------------

		// Initialize the Bedrock Runtime Client
		// The client will use your configured AWS credentials automatically
		using var client = new AmazonBedrockRuntimeClient(region);

		//-------------------
		// 3. Request Setup
		//-------------------
		
		// Embed the prompt in Llama 3's instruction format
		var formattedPrompt = $"""
							<|begin_of_text|><|start_header_id|>user<|end_header_id|>
							{prompt}
							<|eot_id|>
							<|start_header_id|>assistant<|end_header_id|>
							""";
		
		// Format the request using the model's native payload structure
		var nativeRequest = System.Text.Json.JsonSerializer.Serialize(new
		{
			// Add the formatted prompt
			prompt = formattedPrompt,
			
			// Optional: Configure inference parameters
			temperature = temperature,
			max_gen_len = max_gen_len
		});

		// Configure the invoke model request
		var request = new InvokeModelRequest()
		{
			ModelId = modelId,
			Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(nativeRequest)),
			ContentType = "application/json"
		};

		//----------------------
		// 4. Send the Request
		//----------------------

		try
		{
			// Send the request and wait for the response
			var response = await client.InvokeModelAsync(request);
			
			// Decode the model's native response payload
			var modelResponse = await JsonNode.ParseAsync(response.Body);

			// Extract and print the response text
			string responseText = (string) modelResponse["generation"] ?? "";
			// GD.Print(responseText);

			// Adding to the total number of tokens(words) in this game
			LLMLibrary.TotalInputTokens += GlobalStringLibrary.NumberOfWords(prompt);
			LLMLibrary.TotalOutputTokens += GlobalStringLibrary.NumberOfWords(responseText);
			LLMLibrary.TotalNumberOfRequests++;

			// GD.Print("Finished Llama request: " + responseText);
			return responseText;
		}
		catch (Exception ex)
		{
			// In production, you should handle specific exceptions:
			// - AccessDeniedException: Missing access permissions
			// - ValidationException: Invalid request parameters
			// - etc.

			GD.Print($"\nError: {ex.Message}");

			return null;
		}
	}
}
