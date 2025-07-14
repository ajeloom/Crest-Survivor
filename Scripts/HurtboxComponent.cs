using Godot;
using System;

public partial class HurtboxComponent : Area2D
{
	private bool tookDamage = false;
	public bool isInvincible = false;
	private HitboxComponent hitbox;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hitbox = null;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public async override void _Process(double delta)
	{
		if (GetParent().IsInGroup("Player"))
		{
			if (tookDamage)
			{
				// Player will be invincible for 2.5 seconds after taking damage
				if (!isInvincible)
				{
					isInvincible = true;
					await ToSignal(GetTree().CreateTimer(2.5f), SceneTreeTimer.SignalName.Timeout);
					isInvincible = false;
					tookDamage = false;
				}
			}
			else if (!tookDamage && HasOverlappingAreas() && hitbox != null)
			{
				// Damage player if are colliding with a hitbox after the invincibility ends
				CallDamage();
			}
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("Hitbox"))
		{
			hitbox = area.GetParent().GetNode<HitboxComponent>("HitboxComponent");
			
			if (!isInvincible)
			{
				CallDamage();
			}
		}
	}

	private void OnAreaExited(Area2D area)
	{
		if (!HasOverlappingAreas())
		{
			hitbox = null;
		}
	}

	// Calls the Damage() function in HealthComponent
	private void CallDamage()
	{
		tookDamage = true;
		HealthComponent healthComponent = GetParent().GetNode<HealthComponent>("HealthComponent");
		healthComponent.Damage(hitbox.GetDamageNumber());
	}
}
