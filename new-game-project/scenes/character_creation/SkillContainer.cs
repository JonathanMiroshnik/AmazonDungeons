using Godot;

public partial class SkillContainer : MarginContainer
{
	public bool allowAddition = true;
	public int skillPoints = 0;
	
	[Export]
	private Label SkillLabel;
	public string SkillName = "Skill";

	[Export]
	public ColorRect[] Levels; 

	[Export]
	public Color filledColor;
	[Export]
	public Color emptyColor;

	public override void _Ready() {
		if (Levels == null) return; // TODO: error

		for (int i = 0; i < Levels.Length; i++) {
			Levels[i].Color = emptyColor;
		}

		if (SkillLabel != null) SkillLabel.Text = SkillName;
	}	

	public void _on_add_button_pressed() {
		if (Levels.Length < skillPoints) return; // TODO: error
		if (!allowAddition) return;
		if (Character.MAX_CORE_SKILL_LEVEL <= skillPoints) return;

		skillPoints++;

		for (int i = 0; i < skillPoints; i++) {
			if (Levels[i].Color == emptyColor) {
				Levels[i].Color = filledColor;
			}
		}
	}

	public void _on_subtract_button_pressed() {
		if (Levels.Length < skillPoints) return; // TODO: error
		if (skillPoints <= 0) return;

		skillPoints--;

		for (int i = skillPoints; i < Levels.Length; i++) {
			if (Levels[i].Color == filledColor) {
				Levels[i].Color = emptyColor;
			}
		}
	}
}
