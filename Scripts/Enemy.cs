using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float speed = 40.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		Vector2 direction = getDirection();
		
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

	private Vector2 getDirection()
	{
		Vector2 direction;
		Player player = GetNode<Player>("/root/Node2D/Player");
		
		direction.X = player.Position.X - Position.X;
		direction.Y = player.Position.Y - Position.Y;


		// GD.Print(direction.Normalized());

		return direction.Normalized();
	}
}
