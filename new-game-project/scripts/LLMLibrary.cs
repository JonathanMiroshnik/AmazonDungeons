using Godot;

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

}
