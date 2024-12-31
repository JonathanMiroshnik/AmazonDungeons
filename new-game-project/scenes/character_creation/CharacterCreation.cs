using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterCreation : Control
{
	[Export]
	public CoreSkillsSetter coreSkillsSetter;
	private int characterIndex = 0;

	// TODO: when do the characters get created really? is the character creation screen just a character "editing" screen? 
	//  same num of characters always?
	public override void _Ready() {
		if (GameManager.Instance.gameEntities.Count <= 0) return; // TODO: raise error?
	}

	public void ShowCharacter(Character character) {
		if (character == null) return;

		coreSkillsSetter.SetSkillsByCharacter(character);
		GetNode<Label>("%GameEntityType").Text = "Type: " + character.GameEntityType.ToString();
	}

	public void _on_next_character_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		characterIndex++;
		characterIndex %= GameManager.Instance.gameEntities.Count;
		// ShowCharacter(GameManager.Instance.characters[characterIndex]); // FIXME:
	}

	public void _on_save_button_pressed() {
		if (GameManager.Instance.gameEntities.Count <= 0) return;

		GameEntity gameEntity = GameManager.Instance.gameEntities[characterIndex];
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
}
