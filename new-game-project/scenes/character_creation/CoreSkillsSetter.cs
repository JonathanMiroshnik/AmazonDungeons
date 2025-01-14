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
	
	public Character curCharacter;

	public override async void _Ready() {
		await GameManager.Instance.IsLoaded();
		if (skillContainers == null) throw new System.Exception("skillContainers in CoresSkillSetter is null");
	}

	public void SetSkillsByCharacter(Character character) {
		if (character == null) {
			GD.Print("character is null");
			return;
		}

		curCharacter = character;

		int curPoints = 0;
		points_used = 0;

		foreach (var container in skillContainers) {
			curPoints = curCharacter.CoreSkills[container.coreSkill];
			// GD.Print(curPoints);
			container.SetPoints(curPoints);
			points_used += curPoints;
		}

		if (points_used >= TotalPoints) { // FIXME: redone code, why?
			foreach (SkillContainer skillContainer in skillContainers) {
				skillContainer.allowAddition = false;
			}
		}
		else {
			foreach (SkillContainer skillContainer in skillContainers) {
				skillContainer.allowAddition = true;
			}
		}

		GD.Print("points: " + points_used);
		GD.Print("total: " + TotalPoints);
	}

	public void _on_container_add_level() {
		if (curCharacter == null) return;

		if (points_used < TotalPoints) {
			points_used++;
		}

		if (points_used >= TotalPoints) {
			foreach (SkillContainer skillContainer in skillContainers) {
				skillContainer.allowAddition = false;
			}
		}
		
		// curCharacter.CoreSkills = GetAllSkillLevels();

		GD.Print("points: " + points_used);
		GD.Print("total: " + TotalPoints);
	}

	public void _on_container_subtract_level() {
		if (curCharacter == null) return;
		if (points_used <= 0) return;

		foreach (SkillContainer skillContainer in skillContainers) {
			skillContainer.allowAddition = true;
		}
		
		points_used--;

		// curCharacter.CoreSkills = GetAllSkillLevels();

		GD.Print("points: " + points_used);
		GD.Print("total: " + TotalPoints);
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
