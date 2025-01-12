using Godot;
using System.Collections.Generic;
using System.Linq;

// TODO: documentation
public partial class CoreSkillsSetter : MarginContainer
{
	[Export]
	public SkillContainer[] skillContainers;
	[Export]
	public int TotalPoints = 2;
	private int points_used = 0;

	// private int strength_points = 0;
	// private int reflex_points = 0;
	// private int intelligence_points = 0;

	public override void _Ready() {
		if (skillContainers == null) return; // FIXME: raise error, also check by length?
	}

	public void _on_add_button_pressed() {
		if (points_used < TotalPoints) {
			points_used++;
		}

		if (points_used >= TotalPoints) {
			foreach (SkillContainer skillContainer in skillContainers) {
				skillContainer.allowAddition = false;
			}
		}

		// Test print of skill levels
		// var lines = GetAllSkillLevels().Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
		// GD.Print(string.Join("\n", lines));
	}

	public void SetSkillsByCharacter(Character character) {
		int curPoints;
		points_used = 0;

		foreach (var container in skillContainers) {
			curPoints = character.CoreSkills[container.coreSkill];
			// GD.Print(curPoints);
			container.SetPoints(curPoints);
			points_used += curPoints;
		}

		if (points_used >= TotalPoints) { // FIXME: redone code, why?
			foreach (SkillContainer skillContainer in skillContainers) {
				skillContainer.allowAddition = false;
			}
		}
	}

	public void _on_subtract_button_pressed() {
		if (points_used <= 0) return;

		foreach (SkillContainer skillContainer in skillContainers) {
			skillContainer.allowAddition = true;
		}
		
		points_used--;
	}

	public Dictionary<CoreSkill, int> GetAllSkillLevels() {
		Dictionary<CoreSkill, int> allSkillLevels = new Dictionary<CoreSkill, int>();
		foreach (SkillContainer skillContainer in skillContainers) {
			Dictionary<CoreSkill, int> curSkillDict = skillContainer.GetSkillLevels();
			foreach (CoreSkill skill in curSkillDict.Keys) {
				if (!allSkillLevels.ContainsKey(skill)) {
					allSkillLevels[skill] = curSkillDict[skill];
				}
			}
		}

		return allSkillLevels;
	}
}
