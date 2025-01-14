using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterCreation : Node
{
	[Export]
	public CoreSkillsSetter coreSkillsSetter;
	private int gameEntityIndex = 0;

	// TODO: when do the characters get created really? is the character creation screen just a character "editing" screen? 
	//  same num of characters always?
	public override async void _Ready() {
		GD.Print("sds");

		await GameManager.Instance.IsLoaded();

		GD.Print("sds2");
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
			GD.Print("wew1");
			coreSkillsSetter.Visible = true;
			coreSkillsSetter.SetSkillsByCharacter((Character) gameEntity, this);
			GD.Print("wew2");
		}
		else {
			coreSkillsSetter.Visible = false;
		}

		GD.Print("wew3");
		// Showing GameEntity Type
		GetNode<Label>("%GameEntityType").Text = "Type: \n" + gameEntity.GameEntityType.ToString();

		GD.Print("wew4");
		// Showing the personality of the game entity
		GetNode<TextEdit>("%PersonalityText").Text = gameEntity.Personality;


		GD.Print("wew5");
		// Showind the name of the game entity
		GetNode<TextEdit>("%NameEdit").Text = gameEntity.Name;
	}

	public void _on_next_character_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		gameEntityIndex++;
		gameEntityIndex %= GameManager.Instance.gameEntities.Count;

		ShowGameEntity(GameManager.Instance.gameEntities[gameEntityIndex]);
	}

	public void _on_save_button_pressed() {
		GD.Print("opop");

		if (GameManager.Instance.gameEntities.Count <= 0) return;

		GameEntity gameEntity = GameManager.Instance.gameEntities[gameEntityIndex];

		// TODO: save what is related solely to the GameEntity level of the object
		gameEntity.Personality = GetNode<TextEdit>("%PersonalityText").Text;
		gameEntity.Name = GetNode<TextEdit>("%NameEdit").Text;

		if (gameEntity is not Character) return;
		Character character = (Character) gameEntity;

		foreach (var container in coreSkillsSetter.skillContainers) {
			character.CoreSkills[container.coreSkill] = container.skillPoints;
		}

		// coreSkillsSetter.SetSkillsByCharacter((Character) gameEntity);

		ShowGameEntity(gameEntity);
	}

	public async void _on_randomize_character_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		GD.Print(GameManager.Instance.characters.Count);

		GameEntityType curType = GameManager.Instance.gameEntities[gameEntityIndex].GameEntityType;
		if (curType == GameEntityType.Player || curType == GameEntityType.DungeonMaster) return;

		Character newCharacter = await GameManager.Instance.CreateGameCharacter();
		GameManager.Instance.ExchangeGameEntity(gameEntityIndex, newCharacter);

		GD.Print(GameManager.Instance.characters.Count);

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

	public void _on_start_button_pressed() {
		GetTree().ChangeSceneToFile("res://scenes/main_game/Scott_Interior_game.tscn");
	}
}
