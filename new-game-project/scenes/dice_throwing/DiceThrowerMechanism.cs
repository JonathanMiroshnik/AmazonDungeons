using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Allows to throw dice and get their values after their tumbling
/// </summary>
public partial class DiceThrowerMechanism : Node3D
{
	// Die to be thrown
	[Export]
	public PackedScene dice;

	[Export]
	// The number of Dice we throw  // TODO: make it always >= 1
	public int numDice = 1;

	// Instantiated Dice
	private List<Dice> instDice = null; 

	// Time given to the dice to stop tumbling
	private int MAX_MILLISECS_TO_STOP = 5000;

	// Value of a die to be considered a victorious dice
	private int MIN_WINNING_DICE = 4;
	private float START_DICE_MASS = 0.1f;
	private float END_DICE_MASS = 100f;

	private float SIZE_OF_RANDOM_SPAWN_CHANGE = 1;
	float MAX_FORCE_PUSH = 0.25f;


	public override async void _Ready() {
		// We assume a uniform scaling
		SIZE_OF_RANDOM_SPAWN_CHANGE = this.Scale.X;
		MAX_FORCE_PUSH *= this.Scale.X;

		ResetDice();

		// int final = await ThrowDice();
		// GD.Print("FINAL " + final.ToString()); // TODO: delete
	}

	// Deletes the currently existing dice
	private void DeleteDice() {
		if (instDice == null) return;

		// for loop over instDice deleting each one
		foreach (Dice curDice in instDice)
		{
			if (curDice == null) continue;
			curDice.QueueFree();
		}

		instDice.Clear();
	}
	
	// Recreates/resets the current dice
	private void ResetDice() {
		DeleteDice();
		
		// Creating the dice
		instDice = new List<Dice>();
		for (int i = 0; i < numDice; i++)
		{
			Dice curDice = dice.Instantiate<Dice>();
			instDice.Add(curDice);			
			AddChild(curDice);
			
			// Notice: the global position show only be set AFTER the object is already in the global tree, given by AddChild	
			// instDice[i].GetChild<CollisionShape3D>(0).GlobalScale(this.Scale);
			curDice.GetChild<CollisionShape3D>(0).GlobalScale(this.Scale * 5);
			curDice.LinearVelocity = Vector3.Zero;			
			curDice.AngularVelocity = Vector3.Zero;
			curDice.Freeze = true;
			curDice.Mass = START_DICE_MASS;

			Random curRand = new Random();

			curDice.GlobalPosition = GetNode<Node3D>("%DiceSpawnPos").GlobalPosition + new Vector3((float) (2 * curRand.NextDouble() - 1)  * SIZE_OF_RANDOM_SPAWN_CHANGE, 
																									(float) (2 * curRand.NextDouble() - 1)  * SIZE_OF_RANDOM_SPAWN_CHANGE,
																									(float) (2 * curRand.NextDouble() - 1)  * SIZE_OF_RANDOM_SPAWN_CHANGE);		
		}
	}

	// Throws a given die
	private void PhysicallyThrowToMiddle(RigidBody3D curDice) {
		Random curRand = new Random();

		curDice.Freeze = false;
		curDice.ApplyImpulse(new Vector3((float) (2 * curRand.NextDouble() - 1)  * MAX_FORCE_PUSH,
		 								 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH, 
										 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH));
		
		float NEEDED_MULTIPLIER = 25f;
		curDice.ApplyTorqueImpulse(new Vector3((float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH * NEEDED_MULTIPLIER, 
												(float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH * NEEDED_MULTIPLIER, 
												(float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH* NEEDED_MULTIPLIER));
		curDice.ApplyCentralImpulse(new Vector3((float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH,
		 								 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH, 
										 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE_PUSH));
	}

	// Throws dice and gets the number of victourious dice	
	public async Task<int> ThrowDice() {
		ResetDice();
		if (instDice == null) return -1;

		// Throwing the dice randomly
		foreach (Dice curDice in instDice)
		{
			if (curDice == null) continue;
			PhysicallyThrowToMiddle(curDice);
		}

		Vector3 checkPos;
		int milliseconds = MAX_MILLISECS_TO_STOP;
		int MILLISECS_REDUCTION_EACH_CHECK = 100;

		// Waiting for the dice to stop tumbling
		foreach (Dice curDice in instDice)
		{
			checkPos = curDice.GlobalPosition;

			if (milliseconds <= 0) {
				break;
			}

			while(milliseconds > 0) {
				await Task.Delay(MILLISECS_REDUCTION_EACH_CHECK);

				if (checkPos == curDice.GlobalPosition) {
					break;
				}
				else {
					checkPos = curDice.GlobalPosition;
				}

				milliseconds -= MILLISECS_REDUCTION_EACH_CHECK;
			}
		}

		Color GREEN = new Color(0,1,0);
		Color RED = new Color(1,0,0);

		// Finding how many of the dice are victorious
		int num_of_victor_dice = 0;
		foreach (Dice curDice in instDice)
		{
			int curSide = curDice.WhichSideUp();
			if (curSide >= MIN_WINNING_DICE) {
				num_of_victor_dice++;
				curDice.PaintDice(GREEN); // Green
			}
			else {
				curDice.PaintDice(RED);
			}

			curDice.Mass = END_DICE_MASS;
			curDice.LinearVelocity = Vector3.Zero;			
			curDice.AngularVelocity = Vector3.Zero;
		}

		return num_of_victor_dice;
	}


	public async Task<bool> PlayDice(int NumDice, int MinWinningDice) {
		if (numDice < MinWinningDice) return false;
		
		numDice = NumDice;
		MIN_WINNING_DICE = MinWinningDice;

		int final = await ThrowDice();

		return final >= MIN_WINNING_DICE;
	}

	// TODO: need non-victorious die throwing mechanism, for e.x. D20
}
