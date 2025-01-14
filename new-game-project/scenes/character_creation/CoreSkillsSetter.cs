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
	
	public CharacterCreation CC;

	public override async void _Ready() {
		// await GameManager.Instance.IsLoaded();
		if (skillContainers == null) throw new System.Exception("skillContainers in CoresSkillSetter is null");
		// CC = GetNode<CharacterCreation>("%PanelContainer");
	}

	public void _on_add_button_pressed() {
		GD.Print("points: " + points_used);
		GD.Print("total: " + TotalPoints);

		if (points_used < TotalPoints) {
			points_used++;
		}

		if (points_used >= TotalPoints) {
			foreach (SkillContainer skillContainer in skillContainers) {
				skillContainer.allowAddition = false;
			}
		}

		if (CC == null) return;
		CC.ShowGameEntity(CC.GetGameEntity());

		// Test print of skill levels
		// var lines = GetAllSkillLevels().Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
		// GD.Print(string.Join("\n", lines));
	}

	public void SetSkillsByCharacter(Character character) {
		if (character == null) {
			GD.Print("character is null");
			return;
		}

		GD.Print("points: " + points_used);
		GD.Print("total: " + TotalPoints);

		int curPoints = 0;
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
		
		if (CC == null) return;
		CC.ShowGameEntity(CC.GetGameEntity());
	}

	public void _on_subtract_button_pressed() {
		GD.Print("points: " + points_used);
		GD.Print("total: " + TotalPoints);

		if (points_used <= 0) return;

		foreach (SkillContainer skillContainer in skillContainers) {
			skillContainer.allowAddition = true;
		}
		
		points_used--;

		if (CC == null) return;
		CC.ShowGameEntity(CC.GetGameEntity());
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

		if (CC == null) return allSkillLevels;
		CC.ShowGameEntity(CC.GetGameEntity());

		return allSkillLevels;
	}
}
