using Godot;
using System;
using System.Threading.Tasks;
using System.IO;

using Amazon;
using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Collections.Generic;

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

	public List<Character> characters;
	// TODO: add dungeon master

    public string testStr = ""; // TODO: delete this
    public string worldDescription = "";
    public string location = "";

    public override async void _Ready()
    {
        Instance = this;

		// TODO: DELETE THIS CODE, TEST CODE
		//create new character
		characters = new List<Character>();

        string retStr = await AskLlama("Write a description Dungeons and Dragons world, " + 
                                          "with different locations and small bits of lore\n " + 
                                          "Write it as a JSON with only two categories. " +
                                          "The first category is world, which contains the description of the world and "+
                                          "the second is location, which contains a very short description(or just name) of the place the characters are placed in the world.\n" +
                                          "respond only with the JSON and with nothing else." +
                                          "Make sure the JSON file that is outputted is of depth 1 and no more.\n" + // FIXME: problems with this depth thing
                                          "Write all of this in 200 words or fewer.");
        GD.Print(retStr);
        var jsonObject = JsonSerializer.Deserialize<JsonElement>(retStr);
        worldDescription = jsonObject.GetProperty("world").GetString();
        GD.Print("WORLD " + worldDescription);
        location = jsonObject.GetProperty("location").GetString();

        string personalityTest = await AskLlama("Write a Dungeons and Dragons character description.\n " + 
                                                LLMLibrary.JSON_CHARACTER_CREATION_TYPE +
                                                "Make sure the JSON file that is outputted is of depth 1 and no more.\n" +
                                                "Write all of this in 200 words or fewer.");
        GD.Print(personalityTest);
        jsonObject = JsonSerializer.Deserialize<JsonElement>(personalityTest);
        var charName = jsonObject.GetProperty("world").GetString();
        var personality = jsonObject.GetProperty("location").GetString();
        var shortDesc = jsonObject.GetProperty("short").GetString();

        GD.Print("wew1");
		Character character = new Character(charName, personality, shortDesc, location, 2);
		characters.Add(character);
        GD.Print("wew2");


        // IMPORTANT, must be kept at the end of this Ready function 
        //  because other parts of the game rely on it through the IsLoaded function
        Loaded = true;
    }
    
    // Used by parts of the game that need the GameManager to be loaded before they begin their activation
    public async Task<bool> IsLoaded() {
        while (!Loaded) {
            // GD.Print("Waiting for GameManager to load...");
            await Task.Delay(1000);
        }

        return true;
    }

    public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest) {
            GD.Print();
            GD.Print();
			GD.Print("Total number of LLM tokens(input and output string words) used for this game: " + LLMLibrary.TotalTokens.ToString());
            GD.Print("Total number of requests to the LLM: " + LLMLibrary.TotalNumberOfRequests.ToString());
            GD.Print("Average number of tokens per request: " + ((float)(LLMLibrary.TotalTokens / LLMLibrary.TotalNumberOfRequests)).ToString());
            GD.Print();
            GD.Print();
		}
	}

	public static async Task<string> AskLlama(string prompt)
    {
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
        var nativeRequest = JsonSerializer.Serialize(new
        {
            // Add the formatted prompt
            prompt = formattedPrompt,
            
            // Optional: Configure inference parameters
            temperature = 0.5,
            max_gen_len = 500
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
            LLMLibrary.TotalTokens +=  GlobalStringLibrary.NumberOfWords(prompt) + GlobalStringLibrary.NumberOfWords(responseText);
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
