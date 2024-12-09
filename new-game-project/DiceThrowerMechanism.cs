using Godot;
using System;
using System.Threading.Tasks;

public partial class DiceThrowerMechanism : Node
{
	public Dice dice;
	public bool thrown = false;
	public bool stopped = false;

	private float MAX_MILLISECS_TO_STOP = 5000;

	private void ResetDice() {
		thrown = false;
		stopped = false;
	}

	public async Task<int> ThrowDice() {
		if (thrown) return -1;

		if (thrown && stopped) {
			int topSide = dice.WhichSideUp();
			ResetDice();
			return topSide;
		}
		else {

		}
	}
}
