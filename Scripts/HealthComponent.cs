using Godot;
using System;

public partial class HealthComponent : Node2D
{
	private float currentHealth;
	[Export] private float maxHealth = 10.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentHealth = maxHealth;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GetParent().IsInGroup("Player")) {
			TextureProgressBar bar = GetNode<TextureProgressBar>("CanvasLayer/TextureProgressBar");
			Label label = GetNode<Label>("CanvasLayer/Label");
			
			bar.MaxValue = maxHealth;
			bar.Value = currentHealth;
			label.Text = currentHealth.ToString() + "/" + maxHealth.ToString();
		}
	}

	// Called by a hitbox component
	public void Damage(float damageNumber)
	{
		currentHealth -= damageNumber;

		// Destroy the object
		if (currentHealth <= 0.0f) {
			GetParent().QueueFree();
		}
	}
}
