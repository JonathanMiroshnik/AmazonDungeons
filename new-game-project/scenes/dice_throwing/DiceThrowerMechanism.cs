using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
	private Dice[] instDice; 

	// Time given to the dice to stop tumbling
	private int MAX_MILLISECS_TO_STOP = 5000;

	// Value of a die to be considered a victorious dice
	private int MIN_WINNING_DICE = 4;
	private float START_DICE_MASS = 0.1f;
	private float END_DICE_MASS = 100f;


	// public override async void _Ready() {
	// 	ResetDice();
	// 	int final = await ThrowDice();

	// 	GD.Print("FINAL " + final.ToString());
	// }

	// Deletes the currently existing dice
	private void DeleteDice() {
		if (instDice == null) return;

		// for loop over instDice deleting each one
		for (int i = 0; i < instDice.Length; i++)
		{
			if (instDice[i] == null) continue;
			instDice[i].QueueFree();
		}

		instDice = null;
	}
	
	// Recreates/resets the current dice
	private void ResetDice() {
		DeleteDice();
		
		// Creating the dice
		instDice = new Dice[numDice];
		for (int i = 0; i < numDice; i++)
		{
			instDice[i] = dice.Instantiate<Dice>();					
			AddChild(instDice[i]);
			
			// Notice: the global position show only be set AFTER the object is already in the global tree, given by AddChild	
			// instDice[i].GetChild<CollisionShape3D>(0).GlobalScale(this.Scale);
			instDice[i].GetChild<CollisionShape3D>(0).Scale = this.Scale;
			instDice[i].LinearVelocity = Vector3.Zero;			
			instDice[i].AngularVelocity = Vector3.Zero;
			instDice[i].Mass = START_DICE_MASS;
			instDice[i].GlobalPosition = GetNode<Node3D>("%DiceSpawnPos").GlobalPosition;			
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
		if (instDice == null) return -1;

		// Throwing the dice randomly
		for (int i = 0; i < instDice.Length; i++)
		{
			if (instDice[i] == null) return -1;
			PhysicallyThrowToMiddle(instDice[i]);
		}

		Vector3 checkPos;
		int milliseconds = MAX_MILLISECS_TO_STOP;
		int MILLISECS_REDUCTION_EACH_CHECK = 100;

		// Waiting for the dice to stop tumbling
		for (int i = 0; i < instDice.Length; i++)
		{
			checkPos = instDice[i].GlobalPosition;

			if (milliseconds <= 0) {
				break;
			}

			while(milliseconds > 0) {
				await Task.Delay(MILLISECS_REDUCTION_EACH_CHECK);

				if (checkPos == instDice[i].GlobalPosition) {
					break;
				}
				else {
					checkPos = instDice[i].GlobalPosition;
				}

				milliseconds -= MILLISECS_REDUCTION_EACH_CHECK;
			}
		}
	

		// Finding how many of the dice are victorious
		int num_of_victor_dice = 0;
		for (int i = 0; i < instDice.Length; i++)
		{
			int curSide = instDice[i].WhichSideUp();
			if (curSide >= MIN_WINNING_DICE) {
				num_of_victor_dice++;
			}

			instDice[i].Mass = END_DICE_MASS;
			instDice[i].LinearVelocity = Vector3.Zero;			
			instDice[i].AngularVelocity = Vector3.Zero;
		}

		return num_of_victor_dice;
	}

	// TODO: need non-victorious die throwing mechanism, for e.x. D20
}
