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
	public override void _Process(double delta)
	{
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
			GD.Print("Hit enemy");
			DestroyRpc();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		DestroyRpc();
	}

	private void OnTimerTimeout()
	{
		DestroyRpc();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void DestroyRpc()
	{
		QueueFree();
	}
}
