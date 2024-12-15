using Godot;
using System;
using System.Collections.Generic;

public partial class CharacterCreation : Control
{
	[Export]
	public CoreSkillsSetter coreSkillsSetter;
	private int characterIndex = 0;

	//do ready
	public override void _Ready() {
		if (GameManager.Instance.characters.Count <= 0) return; // TODO: raise error?
	}

	public void ShowCharacter(Character character) {
		if (character == null) return;

		coreSkillsSetter.SetSkillsByCharacter(character);
		GetNode<Label>("%CharacterType").Text = "Type: " + character.CharacterType.ToString();
	}

	public void _on_next_character_button_pressed() {
		if (GameManager.Instance.characters.Count <= 0) return;

		characterIndex++;
		characterIndex %= GameManager.Instance.characters.Count;
		ShowCharacter(GameManager.Instance.characters[characterIndex]);
	}

	public void _on_save_button_pressed() {
		if (GameManager.Instance.characters.Count <= 0) return;

		var character = GameManager.Instance.characters[characterIndex];
		foreach (var container in coreSkillsSetter.skillContainers) {
			character.CoreSkills[container.coreSkill] = container.skillPoints;
		}
	}
}
