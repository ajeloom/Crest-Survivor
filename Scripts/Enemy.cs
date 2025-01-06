using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float speed = 40.0f;
	// private Node2D parent = GetParent().GetParent();

	private bool checkDistance = false;
	private Node2D target;

	public override void _Ready()
	{
		PickTarget();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Multiplayer.IsServer()) {
			Movement();
		}
		else {
			return;
		}
	}

	private void Movement()
	{
		Vector2 velocity = Velocity;

		Vector2 direction = GetDirection();
		if (direction != Vector2.Zero) {
			velocity.X = direction.X * speed;
			velocity.Y = direction.Y * speed;
		}
		else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private Vector2 GetDirection()
	{
		Vector2 direction;

		direction.X = target.Position.X - Position.X;
		direction.Y = target.Position.Y - Position.Y;

		return direction.Normalized();
	}

	// The enemy will pick the closest player
	private void PickTarget()
	{
		if (!checkDistance) {
			checkDistance = true;

			Node2D players = GetNode<Node2D>("/root/Game/Players");
			double closest = 0.0;

			for (int i = 0; i < players.GetChildCount(); i++) {
				// Check each player
				Node2D temp = players.GetChild<Node2D>(i);

				// Find the distance between the enemy and player
				float xPos = temp.Position.X - Position.X;
				float yPos = temp.Position.Y - Position.Y;
				double dist = Math.Sqrt(Math.Pow(xPos, 2) + Math.Pow(yPos, 2));

				// Pick the target that is closest
				if (dist < closest || closest == 0.0) {
					closest = dist;
					target = temp;
				}
			}
			
			checkDistance = false;
		}
	}
}
