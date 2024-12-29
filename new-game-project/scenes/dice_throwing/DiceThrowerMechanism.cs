using Godot;
using System;
using System.ComponentModel.DataAnnotations;
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


	public override async void _Ready() {
		ResetDice();
		int final = await ThrowDice();

		GD.Print("FINAL " + final.ToString());
	}

	// Deletes the currently existing dice
	private void DeleteDice() {
		if (instDice == null) return;

		// for loop over instDice deleting each one
		foreach (Dice curDice in instDice)
		{
			if (curDice == null) continue;
			GD.Print("wwww");
			curDice.QueueFree();
		}

		instDice.Clear();

		// instDice = null;
	}
	
	// Recreates/resets the current dice
	private async void ResetDice() {
		DeleteDice();
		
		// Creating the dice
		instDice = new List<Dice>();
		for (int i = 0; i < numDice; i++)
		{
			await Task.Delay(100);

			Dice curDice = dice.Instantiate<Dice>();
			instDice.Add(curDice);			
			AddChild(curDice);

			if (curDice == null) GD.Print("curDice is null");
			
			// Notice: the global position show only be set AFTER the object is already in the global tree, given by AddChild	
			// instDice[i].GetChild<CollisionShape3D>(0).GlobalScale(this.Scale);
			curDice.GetChild<CollisionShape3D>(0).Scale = this.Scale;
			curDice.LinearVelocity = Vector3.Zero;			
			curDice.AngularVelocity = Vector3.Zero;
			curDice.Mass = START_DICE_MASS;

			Random curRand = new Random();
			float SIZE_OF_MOVE = 5f;

			curDice.GlobalPosition = GetNode<Node3D>("%DiceSpawnPos").GlobalPosition + new Vector3((float) (2 * curRand.NextDouble() - 1)  * SIZE_OF_MOVE, 
																									(float) (2 * curRand.NextDouble() - 1)  * SIZE_OF_MOVE,
																									(float) (2 * curRand.NextDouble() - 1)  * SIZE_OF_MOVE);		
		}
	}

	// Throws a given die
	private void PhysicallyThrowToMiddle(RigidBody3D curDice) {
		Random curRand = new Random();

		float MAX_FORCE = 50;

		// curDice.ApplyForce(new Vector3((float) curRand.NextDouble() * MAX_FORCE, (float) curRand.NextDouble() * MAX_FORCE, (float) curRand.NextDouble() * MAX_FORCE));
		curDice.ApplyImpulse(new Vector3((float) (2 * curRand.NextDouble() - 1)  * MAX_FORCE,
		 								 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE, 
										 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE));
		curDice.ApplyTorqueImpulse(new Vector3((float) (2 * curRand.NextDouble() - 1) * MAX_FORCE * 25, 
												(float) (2 * curRand.NextDouble() - 1) * MAX_FORCE * 25, 
												(float) (2 * curRand.NextDouble() - 1) * MAX_FORCE* 25));
		curDice.ApplyCentralImpulse(new Vector3((float) (2 * curRand.NextDouble() - 1) * MAX_FORCE,
		 								 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE, 
										 (float) (2 * curRand.NextDouble() - 1) * MAX_FORCE));
	}

	// Throws dice and gets the number of victourious dice	
	public async Task<int> ThrowDice() {
		ResetDice();
		GD.Print("wew1");
		if (instDice == null) return -1;

		// Throwing the dice randomly
		foreach (Dice curDice in instDice)
		{
			if (curDice == null) continue;
			GD.Print("wwwweteetete");
			PhysicallyThrowToMiddle(curDice);
		}

		GD.Print("wew2");

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

		GD.Print("wew3");

		// Finding how many of the dice are victorious
		int num_of_victor_dice = 0;
		foreach (Dice curDice in instDice)
		{
			int curSide = curDice.WhichSideUp();
			if (curSide >= MIN_WINNING_DICE) {
				num_of_victor_dice++;
			}

			curDice.Mass = END_DICE_MASS;
			curDice.LinearVelocity = Vector3.Zero;			
			curDice.AngularVelocity = Vector3.Zero;
		}

		GD.Print("wew4");
		return num_of_victor_dice;
	}

	// TODO: need non-victorious die throwing mechanism, for e.x. D20
}
