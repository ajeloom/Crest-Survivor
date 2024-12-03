using Godot;
using System;

public partial class HitboxComponent : Area2D
{
	[Export] private float damageNumber = 5.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnAreaEntered(Area2D area)
	{
		// Damage the character associated with the hurtbox
		if (area.IsInGroup("Hurtbox")) {
			HealthComponent healthComponent = area.GetParent().GetNode<HealthComponent>("HealthComponent");
			healthComponent.Damage(damageNumber);
		}
	}
}
