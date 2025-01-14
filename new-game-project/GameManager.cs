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
using Amazon.Runtime;


public enum CoreSkill
{
	Strength,
	Reflex,
	Intelligence
}

// TODO: EXTRA FEATURE, add small movement of camera in accordance to posiiton of mouse in relative position of the screen
public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public bool Loaded = false;

	// The game that is currently running
	public Game game;

	// These two lists will point to the same entities but each will have a different purpose as
	//  GameEntities and Characters serve slightly different purposes.
	public List<GameEntity> gameEntities;
	public List<Character> characters;


	// Represents the Dungeon Master for the game, of which there is only one per game
	public GameEntity DungeonMaster;

	// Description of the DnD-like world
	public JSONWorld worldDesc;

	public int TotalRounds = 1;

	// Total numbers of characters in the game
	public const int NUMBER_OF_AI_CHARACTERS = 2;
	public const int NUMBER_OF_DMS = 1;
	public const int NUMBER_OF_PLAYERS = 1;
	
	public const int NUMBER_OF_CHARACTERS = NUMBER_OF_AI_CHARACTERS + NUMBER_OF_PLAYERS;
	public const int NUMBER_OF_GAME_ENTITIES = NUMBER_OF_AI_CHARACTERS + NUMBER_OF_DMS + NUMBER_OF_PLAYERS;

	// Other constants
	public const float TIME_TO_MOVE_CAMERA_POSITIONS = 2f;
	
	// Used wherever a random value is needed in the project
	public Random random;

	[Signal]
	public delegate void NextActionEventHandler();


	public override async void _Ready()
	{
		Instance = this;
		Loaded = false;

		random = new Random();
		gameEntities = new List<GameEntity>();
		characters = new List<Character>();
		DungeonMaster = new GameEntity("Dungeon Master", GameEntityType.DungeonMaster, "prompter");


		// TODO: delete this part and replace with above in real-game
		worldDesc = new JSONWorld();
		worldDesc.world = LLMLibrary.EXAMPLE_WORLD;
		worldDesc.location = LLMLibrary.EXAMPLE_LOCATION;

		// Creating Main Player character
		Character curChar = new Character("Player", "write personality here", "write shortened description here", "prompt1", 0, 0, 0, GameEntityType.Player);
		gameEntities.Add(curChar);
		characters.Add(curChar);

		// Creating the AI characters
		// for (int i = 0; i < NUMBER_OF_AI_CHARACTERS; i++) { // FIXME:
		// 	await CreateGameCharacter();
		// }

		AddCharacterToLists(await CreateGameCharacter()); // TODO: delete this part and replace with above in real-game
		// CreateExampleCharacter(LLMLibrary.EXAMPLE_CHARACTER_1[0], LLMLibrary.EXAMPLE_CHARACTER_1[1], LLMLibrary.EXAMPLE_CHARACTER_1[2]);

		// Create new dungeon master
		DungeonMaster = new GameEntity("Dungeon Master", GameEntityType.DungeonMaster, "prompt2");
		gameEntities.Add(DungeonMaster);

		AddCharacterToLists(await CreateGameCharacter()); // TODO: delete this part and replace with above in real-game
		// CreateExampleCharacter(LLMLibrary.EXAMPLE_CHARACTER_2[0], LLMLibrary.EXAMPLE_CHARACTER_2[1], LLMLibrary.EXAMPLE_CHARACTER_2[2]);

		// IMPORTANT, must be kept at the end of this Ready function 
		//  because other parts of the game rely on it through the IsLoaded function
		Loaded = true;
	}

	public async Task<Character> CreateGameCharacter() {
		string curCreationPrompt = "Write a Dungeons and Dragons character description.\n ";

		// Adding the previous characters into the creation consideration:
		curCreationPrompt += "\nDo not create two character that are too similar. Characters that have been created so far:";
		for (int i = 0; i < characters.Count; i++) {
			curCreationPrompt += "\n" + characters[i].GetDescription();
		}

		// Adding JSON limits
		curCreationPrompt += LLMLibrary.JSON_CHARACTER_CREATION_TYPE +
								"The input in the categories are only string, not lists, not anything else.\n" +
								"Don't write something long but make sure that the JSON is valid and closed properly.";

		// create new character
		string personalityTest = await LLMLibrary.AskLlama(curCreationPrompt, 0.5f, 1000);
		personalityTest = GlobalStringLibrary.JSONStringBrackets(personalityTest);

		JSONCharacter resultChar = new JSONCharacter();
		try {
			resultChar = JsonConvert.DeserializeObject<JSONCharacter>(personalityTest);
		}
		catch (Exception e) {
			GD.Print(e.Message);
		}

		string charName = resultChar.name;
		string personality = resultChar.personality;
		string shortDesc = resultChar.shortdesc;
		
		// Creating random stats for the character
		int totalPoints = 2;
		int strength = random.Next(0, 2);
		totalPoints -= strength;

		int reflex = random.Next(0, totalPoints);
		totalPoints -= reflex;
		
		int intelligence = totalPoints;

		// Creating the character and adding it to the total
		Character character = new Character(charName, personality, shortDesc, curCreationPrompt, strength, reflex, intelligence);

		return character;
	}

	public void AddCharacterToLists(Character c) {
		if (characters == null || gameEntities == null) {
			GD.PrintErr("Error: characters or gameEntities is null");
			return;
		}

		gameEntities.Add(c);
		characters.Add(c);
	}

	public Character CreateExampleCharacter(string charName, string personality, string shortDesc) {
		Character character = new Character(charName, personality, shortDesc, "prompt4", 2);
		gameEntities.Add(character);
		characters.Add(character);

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

	public void ExchangeGameEntity(int gameEntityInd, GameEntity gameEntity) {
		if (gameEntities == null || characters == null) {
			GD.PrintErr("Error: gameEntities or characters is null");
			return;
		}
		if (gameEntities.Count <= gameEntityInd) {
			GD.PrintErr("Error: gameEntityInd is out of range");
			return;
		}

		GameEntity curGE = gameEntities[gameEntityInd];
		if (curGE == null) {
			GD.PrintErr("Error: curGE is null");
			return;
		}

		GameEntityType curGEType = curGE.GameEntityType;

		if (curGE is Character && gameEntity is Character) {
			Character curChar = (Character) curGE;

			int charInd = 0;
			foreach (Character character in characters) {
				if (character == curChar) {
					break;
				}

				charInd++;
			}

			characters.RemoveAt(charInd);
			characters.Insert(charInd, (Character) gameEntity);
		}

		gameEntities.RemoveAt(gameEntityInd);
		gameEntities.Insert(gameEntityInd, gameEntity);

		gameEntities[gameEntityInd].GameEntityType = curGEType;
	}

	// Makes the given game the current game that is effected by the actions of the GameManager
	public void RegisterGame(Game game) {
		this.game = game;
		// TODO: should be a universal state machine at some point
		// this.NextAction += game.MoveToNextCharacter;
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
						((float) (LLMLibrary.TotalInputTokens + LLMLibrary.TotalOutputTokens) / LLMLibrary.TotalNumberOfRequests).ToString());
			GD.Print();
			GD.Print();
		}
	}

	public void SceneChange(string path) {
		GetTree().ChangeSceneToFile(path);
	}
}
