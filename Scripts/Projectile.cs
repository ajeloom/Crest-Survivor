using Godot;
using System;

public partial class Projectile : CharacterBody2D
{
	[Export] public float speed = 40.0f;
	public Vector2 direction;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		direction = Vector2.Right.Rotated(Rotation);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}

		Vector2 velocity = Velocity;

		velocity.X = direction.X * speed;
		velocity.Y = direction.Y * speed;

		Velocity = velocity;
		MoveAndSlide();
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("Hurtbox") && area.GetParent().IsInGroup("Enemy"))
		{
			Destroy();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		Destroy();
	}

	private void OnTimerTimeout()
	{
		Destroy();
	}

	private void Destroy()
	{
		if (Multiplayer.IsServer())
		{
			QueueFree();
		}
	}
}
