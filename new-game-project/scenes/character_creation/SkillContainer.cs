using System.Collections.Generic;
using Godot;

// TODO: documentation
public partial class SkillContainer : MarginContainer
{
	[Export]
	public int NumberOfLevels { get; set; } = 2; // TODO: needs to be positive int
	[Export]
	public Color filledColor { get; set; }
	[Export]
	public Color emptyColor { get; set; }
	[Export]
	public CoreSkill coreSkill { get; set; }

	public bool allowAddition { get; set; } = true;
	public int skillPoints { get; set; } = 0;

	private Label SkillLabel;
	private HBoxContainer HLevelContainer;
	private ColorRect[] Levels; 
	
	private int CUSTOM_MINIMUM_SIZE_X = 30;

	[Signal]
	public delegate void AddLevelEventHandler();
	[Signal]
	public delegate void SubtractLevelEventHandler();

	public override async void _Ready() {
		await GameManager.Instance.IsLoaded();

		// Number of levels must be positive int
		if (NumberOfLevels <= 0) return;

		// Getting container that will hold the level representation
		HLevelContainer = GetNode<HBoxContainer>("%HLevelContainer");
		if (HLevelContainer == null) return;

		// Creating level representation
		Levels = new ColorRect[NumberOfLevels];
		for (int i = 0; i < Levels.Length; i++) {
			Levels[i] = new ColorRect();
			Levels[i].CustomMinimumSize = new Vector2(CUSTOM_MINIMUM_SIZE_X, 0);
			Levels[i].Color = emptyColor;

			// Adding level representation to the world
			HLevelContainer.AddChild(Levels[i]);
		}

		// Getting the skill name label
		SkillLabel = GetNode<Label>("%SkillLabel");
		SkillLabel.Text = coreSkill.ToString(); // FIXME: should this remain like this?
	}	

	private void Reset() {
		if (Levels == null) return;

		for (int i = 0; i < Levels.Length; i++) {
			Levels[i].Color = emptyColor;
		}

		allowAddition = true;
		skillPoints = 0;
	}

	public void SetPoints(int points) {
		if (points < 0 || points > NumberOfLevels) return;

		Reset();

		skillPoints = points;

		for (int i = 0; i < skillPoints; i++) {
			if (Levels[i].Color == emptyColor) {
				Levels[i].Color = filledColor;
			}
		}
	}

	public void _on_add_button_pressed() {
		if (Levels.Length < skillPoints) return; // FIXME: raise error
		if (!allowAddition) return;
		if (Character.MAX_CORE_SKILL_LEVEL <= skillPoints) {
			allowAddition = false;
			return;
		}

		skillPoints++;

		for (int i = 0; i < skillPoints; i++) {
			if (Levels[i].Color == emptyColor) {
				Levels[i].Color = filledColor;
			}
		}

		EmitSignal(SignalName.AddLevel);
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

		EmitSignal(SignalName.SubtractLevel);
	}

	public Dictionary<CoreSkill, int> GetSkillLevels() {
		if (SkillLabel == null) return null;

		Dictionary<CoreSkill, int> skillLevels = new Dictionary<CoreSkill, int>();
		skillLevels[coreSkill] = skillPoints;

		return skillLevels;
	}
}
