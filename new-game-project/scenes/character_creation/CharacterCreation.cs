using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterCreation : Control
{
	[Export]
	public CoreSkillsSetter coreSkillsSetter;
	private int gameEntityIndex = 0;

	// TODO: when do the characters get created really? is the character creation screen just a character "editing" screen? 
	//  same num of characters always?
	public override void _Ready() {
		if (GameManager.Instance.gameEntities.Count <= 0) return; // TODO: raise error?
		ShowGameEntity(GameManager.Instance.gameEntities[gameEntityIndex]);
	}

	public void ShowGameEntity(GameEntity gameEntity) {
		if (gameEntity == null) return;

		// If it is a character, there is a point in showing skills
		if (gameEntity is Character) {
			coreSkillsSetter.Visible = true;
			coreSkillsSetter.SetSkillsByCharacter((Character) gameEntity);
		}
		else {
			coreSkillsSetter.Visible = false;
		}

		// Showing GameEntity Type
		GetNode<Label>("%GameEntityType").Text = "Type: " + gameEntity.GameEntityType.ToString();

		// Showing the prompt the character was created with
		GetNode<TextEdit>("%PromptText").Text = gameEntity.CreationPrompt;
	}

	public void _on_next_character_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		gameEntityIndex++;
		gameEntityIndex %= GameManager.Instance.gameEntities.Count;
		ShowGameEntity(GameManager.Instance.gameEntities[gameEntityIndex]);
	}

	public void _on_save_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		GameEntity gameEntity = GameManager.Instance.gameEntities[gameEntityIndex];
		// TODO: save what is related solely to the GameEntity level of the object

		if (gameEntity is not Character) return;
		Character character = (Character) gameEntity;

		foreach (var container in coreSkillsSetter.skillContainers) {
			character.CoreSkills[container.coreSkill] = container.skillPoints;
		}
	}

	public void _on_randomize_character_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		// TODO: change text of character prompt/create relevate text
		// TODO: choose random values for attributes
		// TODO: choose name	
	}

	public void _on_start_button_pressed() {
		GetTree().ChangeSceneToFile("res://scenes/main_game/main_game.tscn");
	}
}
