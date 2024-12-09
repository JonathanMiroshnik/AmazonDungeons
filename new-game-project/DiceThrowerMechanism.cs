using Godot;
using System;
using System.Threading.Tasks;

public partial class DiceThrowerMechanism : Node
{
	[Export]
	public Dice dice;

	public bool stopped = false;

	private int MAX_MILLISECS_TO_STOP = 5000;

	// do Ready
	public override async void _Ready() {
		ResetDice();
		var final = await ThrowDice();

		GD.Print("FINAL " + final.ToString());
	}

	private void ResetDice() {
		// TODO: maybe stopped is also unnecessary?
		stopped = false;
	}

	private void PhysicallyThrowToMiddle() {
		Random curRand = new Random();
		dice.ApplyForce(new Vector3(curRand.NextInt64(0, 30), curRand.NextInt64(0, 30), curRand.NextInt64(0, 30)));
		dice.ApplyTorqueImpulse(new Vector3(curRand.NextInt64(0, 30), curRand.NextInt64(0, 30), curRand.NextInt64(0, 30)));
		return;
	}

	public async Task<int> ThrowDice() {
		PhysicallyThrowToMiddle();

		Vector3 checkPos = dice.GlobalPosition;
		
		int milliseconds = MAX_MILLISECS_TO_STOP;
		int MILLISECS_REDUCTION_EACH_CHECK = 100;

		while(milliseconds > 0) {
			await Task.Delay(MILLISECS_REDUCTION_EACH_CHECK);

			if (checkPos == dice.GlobalPosition) {
				break;
			}
			else {
				checkPos = dice.GlobalPosition;
			}

			milliseconds -= MILLISECS_REDUCTION_EACH_CHECK;
		}

		// TODO: maybe stopped is also unnecessary?
		stopped = true;

		if (stopped) {
			int topSide = dice.WhichSideUp();
			ResetDice();
			return topSide;
		}
		
		// TODO: maybe stopped is also unnecessary? as above
		return 0;
	}
}
