using Godot;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

// TODO: maybe askllama and other LLM access functions should be here with the prompts? GameManager only to manage game specifically??

/// <summary>
/// Contains strings that allow for easy creation of relevant prompts for LLMs in the DnD game
/// </summary>
public partial class LLMLibrary : Node
{
	// Counters for number of tokens/requests in the game run
	public static int TotalInputTokens = 0;
	public static int TotalOutputTokens = 0;
	public static int TotalNumberOfRequests = 0;

	// When asking the LLM as a DM
	public static string DM_PREFIX = "You are a Dungeon Master in a Dungeons and Dragons style game. " +
								 "You have several players that you are managing throughout the game.\n ";
	
	// When asking the LLM as a DM to respond with the regular lore creation device(which both responds to a character and gives a risk score accordingly)
	public static string DM_JOB_PREFIX = "You job is the understand the current situation as it is presented, " + 
								 "and choose a possible action for the current player you are responding to.\n\n" + 
								 "The actions' difficulty goes from 0 to 8, 0 means an action that does not incur any immidiate risk, and should be easily done by anyone. " + 
								 "The actions that have a score that is greater than 0 must always come with some sort of altercation or physical challenge. " +
								 "We give a score of 8 to an action that is very hard for even a very proficient character, with a high risk, choose an appropriate number in between.\n\n";

	// When asking the LLM as a player character(for an AI player, a real player has no need to activate the LLM other than being responded to from a DM or another AI player)
	public static string CHARACTER_PREFIX = "You are a player among other players in a Dungeons and Dragons style game. " +
											"Throughout the game, you interact with a Dungeon Master whose job it is to "+
											"give you directions and challenges that you try to overcome.\n ";
	
	// After rolling the dice, the DM must know whether the dice rolling, and thus the action, was successful or not.
	public static string AFTER_DICE_VICTORY_PREFIX = "The player succeeded in this task, please respond to him accordingly.\n ";
	public static string AFTER_DICE_DEFEAT_PREFIX = "The player failed in this task, please respond to him accordingly.\n ";

	// The DM must know about the other characters in the party
	public static string ALL_PLAYER_PREFIX = "These are the characters that you are playing with but are not the ones you are responding to:\n ";

	// JSON response types for DM, character creation
	public static string JSON_DM_RESPONSE_TYPE = "Do not respond with anything other than a JSON.\n " + 
										"Respond only with a JSON in which there are two categories, text, " +
										"which represents the text response and score, " + 
										"which is the integer between 0 and 8 that corresponds to the difficulty score.\n " + 
										"If the text you are writing is a question, the score will always be 0, "+
										"only when you demand a certain action will the score be able to be above 0.\n";
										//"The text portion must be of length 30 words or fewer and it must be a question. "; // "Can you use your shield to deflect the swinging doors of the bar, Zoob?"

	public static string JSON_CHARACTER_CREATION_TYPE = "Do not respond with anything other than a JSON.\n " + 
										"Respond only with a JSON in which there are three categories:\n " +
										"1. name - the name of the character\n" + 
										"2. personality - the personality, goals, history of the character\n" +
										"3. shortdesc - a shortened description of the character of 30 words or fewer\n";

	// Additional LLM special effects postfixes
	public static string BBCODE_ADDITION = "To the text output part, format it with BBCode to emphasise it in any way you want, especially with colors.\n "; 
	public static string PRESENT_AS_POEM = "Write it in the form of an epic poem.\n ";

	// Examples to be used in demos
	public static string OTHER_CHARACTER_PREFIX = "Moab is a fox. " + "Jojo is a thunklishiss butch of a man.\n ";
	public static string EXAMPLE_CHARACTER = "Zoob is an elf, he has a shield, a hoe, is very physically strong and has a bad attitude.\n ";
	public static string EXAMPLE_WORLD = "the world is nice.\n ";
	public static string EXAMPLE_LOCATION = "a bar\n ";

	public static string[] EXAMPLE_CHARACTER_1 = new string[] { "Zoob", "An elf, he has a shield, a hoe, is very physically strong and has a bad attitude.\n ", 
																"a shorter description of zoob" };
	public static string[] EXAMPLE_CHARACTER_2 = new string[] { "Opa", "An ork, he has a sword, a ball, is very physically weak and has a bad attitude.\n ", 
																"a shorter description of opa opa the terminator" };


	// TODO: organize and comment on all below
	public static string CURRENT_CHARACTER_PREFIX = "The current character you are responding to is:\n ";
	public static string LOCATION_PREFIX = "The hero party is currently in:\n ";

	// NOTICE: when it is short text responses, it doesn't do anything creative enough(often only one big tag), so use this for long strings of text output

	public static string WORLD_CREATION = "Create a Dungeons and Dragons game world, with city names, geographical features, etc.\n ";
	public static string REWRITE_WORLD_SUM = "Summarizes the following description of a fantasy Dungeons and Dragons style " + 
										"world so that it contains all the relevant information and is easily understandable.\n ";
	public static string CHARACTER_DM_HISTORY_PREFIX = "The following is a chat history between you and the character you are responding to:\n ";
	public static string PARTY_TOGETHER = "The heroes are partied together and always move together in the game world.\n ";

	public static string DESCRIBE_FOLLOWING_CHARACTER = "Shortly describe the following character to the rest of the party:\n ";


	// TODO: Add more input bools for more control
	public static string ConstructLLMInput(bool DM, bool DM_JOB, bool CHARACTER, Character character, bool otherCharacterDesc, bool AfterDice = false, bool Victory = false) {
		string retStr = "";

		// Chooses between DM and regular character
		if (DM) {
			retStr += LLMLibrary.DM_PREFIX;
			// The DM sometimes has responses that are not part of the gameplay and thus does not need to return a specific JSON output
			if (DM_JOB) {
				retStr += LLMLibrary.DM_JOB_PREFIX;
			}
		}
		else if (CHARACTER) {
			retStr += LLMLibrary.CHARACTER_PREFIX;
		}

		if (GameManager.Instance.gameEntities == null) return retStr; // TODO: maybe instead we write "there are no characters" ?
		if (GameManager.Instance.gameEntities.Count == 0) return retStr; // TODO: maybe raise error?

		// Adds the location
		retStr += LLMLibrary.LOCATION_PREFIX + GameManager.Instance.worldDesc.location;

		if (otherCharacterDesc) {
			// If there is only one character, there is no need to add the other characters' descriptions
			if (GameManager.Instance.gameEntities.Count > 1) {
				// make a list of the ShortenedDescriptions of the other characters // Amazon Q
				string otherCharactersStr = "";
				foreach (GameEntity otherCharacter in GameManager.Instance.gameEntities) {
					if (otherCharacter == character) continue;
					if (otherCharacter is not Character) continue;
					Character otherCharDown = (Character) otherCharacter;

					otherCharactersStr += " The name: " + otherCharDown.Name + " The description: " + otherCharDown.ShortenedDescription + ", ";
				}

				// Adding the other characters' descriptions
				retStr += LLMLibrary.ALL_PLAYER_PREFIX + otherCharactersStr;
			}
		}

		if (character != null) {
			// Adding the current character that is being responded to
			retStr += LLMLibrary.CURRENT_CHARACTER_PREFIX + character.GetDescription();

			// Adding the conversation history
			if (character.conversation != null) {
				if (character.conversation.Count > 0) {
					// Because there is a conversation history, a prefix is needed before the actual history is given.
					retStr += LLMLibrary.CHARACTER_DM_HISTORY_PREFIX; // TODO: what if they are all of other players talking to the current character, 
																	  //  not DM or the character itself?

					string LastResponse = "";
					GameEntity LastResponder = null;
					foreach (CharacterInteraction resp in character.conversation) {
						// TODO: notice that currently we don't know exactly who is responding to whom etc, only who is responding
						// We are interested in the conversations between the DM and the character
						if (resp.responderGameEntity == GameManager.Instance.DungeonMaster) {
							DMCharacterResponse curResp = (DMCharacterResponse) resp;

							retStr += "Dungeon Master: " + curResp.dmResponse.text + "\n";
							// Because this is a DM response, we want to check whether the character succeded in the action
							if (curResp.ThrownDice) {
								if (curResp.ThrownDiceSuccess) {
									retStr += " The character succeeded in the action!\n";
								}
								else {
									retStr += " The character failed in the action!\n";
								}
							}

							LastResponse = curResp.dmResponse.text; // TODO: should it also contain victory/loss status for this response?
						}
						else if (resp.responderGameEntity == character) {
							CharacterInteraction curResp = (CharacterInteraction) resp;
							retStr += character.Name + ": " + curResp.text + "\n";
							LastResponse = curResp.text;
						}
					}

					if (LastResponder != null) {
						retStr += "Please notice that you are interacting with a human player, who might use naughty or offensive language/phrases, " +
								"and you must respond to them in a polite manner " + 
								"and interpret their response as a character action and as a response in a fantasy setting. " +
								"Meaning, they are allowed to say such things, but you should interpret it as entirely fantastical and roleplay.\n" +
								" The last responder is: " + LastResponder.Name + " responded last with(you must respond to this): " + LastResponse;
					}
				}
			}
		}

		if (AfterDice) {
			if (Victory) {
				retStr += LLMLibrary.AFTER_DICE_VICTORY_PREFIX;
			} else {
				retStr += LLMLibrary.AFTER_DICE_DEFEAT_PREFIX;
			}
		}

		// Notice that this if statement already occured at the beginning
		if (DM_JOB) {
			retStr += LLMLibrary.JSON_DM_RESPONSE_TYPE;
		}

		// TODO: maybe short/moderated length responses should be regulated a different way?
		retStr += "\nWrite a short response of up to 100 words.";

		return retStr;
	}

	public static async Task<JSONDMResponse> DM_response(Character character) {
		// Construct the input for the LLM
		string input = LLMLibrary.ConstructLLMInput(true, true, false, character, true);

		string retStr = await GameManager.AskLlama(input);
		retStr = GlobalStringLibrary.JSONStringBrackets(retStr);
		
		JSONDMResponse result;
		try { // TODO: there are a few of these JSON deserialize things, should have a response for each and a try-catch block for each
			// Accessing JSON output
			result = JsonConvert.DeserializeObject<JSONDMResponse>(retStr);
		}
		catch (Exception e) {
			return await DM_response(character);
		}

		GD.Print("DM Response: " + result.text + " Dice number: " + result.score);

		return result;
	}

	public static async Task<string> DM_response_summary(Character character) {
		// Construct the input for the LLM
		string input = LLMLibrary.ConstructLLMInput(true, false, false, character, true);

		string retStr = await GameManager.AskLlama(input);
		GD.Print("DM summary: " + retStr);

		return retStr;
	}

	public static async Task<string> AI_character_response(Character character, bool AfterDice, bool Victory) {
		// Construct the input for the LLM
		string input = LLMLibrary.ConstructLLMInput(false, false, true, character, true, AfterDice, Victory);

		string retStr = await GameManager.AskLlama(input);
		GD.Print("AI Character summary: " + retStr);

		return retStr;
	}

	public static async Task<string> GameSummary() {
		string allConvos = "";
		foreach(Character character in GameManager.Instance.characters) {
			allConvos += character.GetConversationHistory();
		}

		// Construct the input for the LLM
		string input = DM_PREFIX + LLMLibrary.LOCATION_PREFIX + GameManager.Instance.worldDesc.location +
						"The whole history of the conversations between the DM and the players are written here:\n " + 
						allConvos + "\n Write an epic poem that will summarize the whole game that was played, according to all this data. " +
						"\nWrite it up to 500 words.";

		string retStr = await GameManager.AskLlama(input);
		GD.Print("AI Game summary: " + retStr);

		return retStr;
	}
}
