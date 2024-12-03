using Godot;
using System;

public partial class HealthComponent : Node2D
{
	[Export] private float health = 10.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called by a hitbox component
	public void Damage(float damageNumber)
	{
		health -= damageNumber;

		// Destroy the object
		if (health <= 0.0f) {
			GetParent().QueueFree();
		}
	}
}
