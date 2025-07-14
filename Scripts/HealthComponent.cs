using Godot;
using System;

public partial class HealthComponent : Node2D
{
	[Export] private float currentHealth;
	[Export] private float maxHealth = 100.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentHealth = maxHealth;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (IsMultiplayerAuthority() && GetParent().IsInGroup("Player"))
		{
			TextureProgressBar bar = GetNode<TextureProgressBar>("CanvasLayer/TextureProgressBar");
			Label label = GetNode<Label>("CanvasLayer/Label");

			bar.MaxValue = maxHealth;
			bar.Value = currentHealth;
			label.Text = currentHealth.ToString() + "/" + maxHealth.ToString();
		}
	}

	// Called by a hurtbox component
	public void Damage(float damageNumber)
	{
		currentHealth -= damageNumber;

		// Play damage animation for player
		if (GetParent().IsInGroup("Player"))
		{
			AnimationPlayer animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			animPlayer.Play("DamageFlash");
		}

		// Destroy the object
		if (Multiplayer.IsServer() && currentHealth <= 0.0f)
		{
			GetParent().QueueFree();
		}
	}
}
