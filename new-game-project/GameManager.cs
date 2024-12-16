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

	public List<Character> characters;
	// TODO: add dungeon master

    public string testStr = ""; // TODO: delete this


    private string DM_PREFIX = "You are a Dungeon Master in a Dungeons and Dragons style game. " +
                                 "You have several players that you are managing throughout the game. " +
                                 "You job is the understand the current situation as it is presented, " + 
                                 "and choose a possible action for the current player you are responding to.\n\n" + 
                                 "The actions' difficulty goes from 0 to 8, 0 means an action that does not incur any immidiate risk, and should be easily done by anyone. " + 
                                 "The actions that have a score that is greater than 0 must always come with some sort of altercation or physical challenge. " +
                                 "We give a score of 8 to an action that is very hard for even a very proficient character, with a high risk, choose an appropriate number in between.";
    
    private string ALL_PLAYER_PREFIX = "These are the characters that you are playing with but are not the ones you are responding to: ";
    private string OTHER_CHARACTER_PREFIX = "Moab is a fox. " + "Jojo is a thunklishiss butch of a man. ";
    private string CURRENT_CHARACTER_PREFIX = "The current character you are responding to is: ";
    private string LOCATION_PREFIX = "The hero party is currently is Grifindarar, a bar in the country of Noot";
    private string JSON_RESPONSE_TYPE = "Do not respond with anything other than a JSON. " + 
                                        "Respond only with a JSON in which there are two categories, text, " +
                                        "which represents the text response and score, " + 
                                        "which is the integer between 0 and 8 that corresponds to the difficulty score. ";
                                        //"The text portion must be of length 30 words or fewer and it must be a question. "; // "Can you use your shield to deflect the swinging doors of the bar, Zoob?"

    // NOTICE: when it is short text responses, it doesn't do anything creative enough(often only one big tag), so use this for long strings of text output
    private string BBCODE_ADDITION = "To the text output part, format it with BBCode to emphasise it in any way you want, especially with colors. "; 

    private string EXAMPLE_CHARACTER = "Zoob is an elf, he has a shield, a hoe, is very physically strong and has a bad attitude. ";

    private string WORLD_CREATION = "Create a Dungeons and Dragons game world, with city names, geographical features, etc. ";
    private string PRESENT_AS_POEM = "Write it in the form of an epic poem. ";
    private string REWRITE_WORLD_SUM = "Summarizes the following description of a fantasy Dungeons and Dragons style " + 
                                        "world so that it contains all the relevant information and is easily understandable";
    public override async void _Ready()
    {
        Instance = this;

		// TODO: DELETE THIS CODE, TEST CODE
		//create new character
		characters = new List<Character>();
		Character character = new Character("bobo", "ee", 2, 0, 0);
		characters.Add(character);

        // testStr = await AskLlama(DM_PREFIX + LOCATION_PREFIX + ALL_PLAYER_PREFIX + OTHER_CHARACTER_PREFIX + 
        //                          CURRENT_CHARACTER_PREFIX + EXAMPLE_CHARACTER + JSON_RESPONSE_TYPE);
        var worldStr = await AskLlama(WORLD_CREATION + PRESENT_AS_POEM);
        GD.Print(worldStr);
        testStr = await AskLlama("A description of a world in the form of an epic poem is thus:\n" + worldStr + REWRITE_WORLD_SUM);
        GD.Print(testStr);
    }

	public static async Task<string> AskLlama(string prompt)
    {
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
