using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class CharacterCreation : Node
{
	[Export]
	public GlobalAudioLibrary globalAudioLibrary;

	[Export]
	public CoreSkillsSetter coreSkillsSetter;
	private int gameEntityIndex = 0;

	// TODO: when do the characters get created really? is the character creation screen just a character "editing" screen? 
	//  same num of characters always?
	public override async void _Ready() {
		await GameManager.Instance.IsLoaded();

		if (globalAudioLibrary == null) throw new Exception("globalAudioLibrary is null in CharacterCreation");

		if (GameManager.Instance.gameEntities.Count <= 0) return; // TODO: raise error?
		ShowGameEntity(GameManager.Instance.gameEntities[gameEntityIndex]);
	}

	public GameEntity GetGameEntity() {
		if (GameManager.Instance.gameEntities.Count <= 0) return null;
		return GameManager.Instance.gameEntities[gameEntityIndex];
	}

	public void ShowGameEntity(GameEntity gameEntity) {
		if (gameEntity == null) return;

		// If it is a character, there is a point in showing skills
		if (gameEntity is Character) {
			coreSkillsSetter.Visible = true;
			coreSkillsSetter.SetSkillsByCharacter((Character) gameEntity, this);
		}
		else {
			coreSkillsSetter.Visible = false;
		}

		// Showing GameEntity Type
		GetNode<Label>("%GameEntityType").Text = "Type: \n" + gameEntity.GameEntityType.ToString();

		// Showing the personality of the game entity
		GetNode<TextEdit>("%PersonalityText").Text = gameEntity.Personality;

		// Showind the name of the game entity
		GetNode<TextEdit>("%NameEdit").Text = gameEntity.Name;
	}

	public void _on_next_character_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);

		gameEntityIndex++;
		gameEntityIndex %= GameManager.Instance.gameEntities.Count;

		ShowGameEntity(GameManager.Instance.gameEntities[gameEntityIndex]);
	}

	public void _on_save_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;
		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);

		GameEntity gameEntity = GameManager.Instance.gameEntities[gameEntityIndex];

		// TODO: save what is related solely to the GameEntity level of the object
		gameEntity.Personality = GetNode<TextEdit>("%PersonalityText").Text;
		gameEntity.Name = GetNode<TextEdit>("%NameEdit").Text;

		if (gameEntity is not Character) return;
		Character character = (Character) gameEntity;

		foreach (var container in coreSkillsSetter.skillContainers) {
			character.CoreSkills[container.coreSkill] = container.skillPoints;
		}

		ShowGameEntity(gameEntity);
	}

	public async void _on_randomize_character_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;
		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);

		GameEntityType curType = GameManager.Instance.gameEntities[gameEntityIndex].GameEntityType;
		if (curType == GameEntityType.Player || curType == GameEntityType.DungeonMaster) return;

		Character newCharacter = await GameManager.Instance.CreateGameCharacter();
		GameManager.Instance.ExchangeGameEntity(gameEntityIndex, newCharacter);

		ShowGameEntity(GameManager.Instance.gameEntities[gameEntityIndex]);
	}

	private int indInCharacters() {
		int indInCharacters = -1;
		foreach (var GE in GameManager.Instance.gameEntities) {
			if (GE is Character) {
				foreach (var character in GameManager.Instance.characters) {
					if (character == GE) {
						indInCharacters = GameManager.Instance.characters.IndexOf(character);
						break;
					}
				}
			}
		}

		return indInCharacters;
	}

	public async void _on_start_button_pressed() {
		globalAudioLibrary?.PlayRandomSound(GlobalAudioLibrary.BUTTON_PATH);
		await Task.Delay(200);

		GetTree().ChangeSceneToFile("res://scenes/main_game/Scott_Interior_game.tscn");
	}
}
