using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float speed = 50.0f;

	private GameManager gm;

	public override void _EnterTree()
	{
		if (Multiplayer.MultiplayerPeer != null) {
			SetMultiplayerAuthority(Convert.ToInt32(Name));
		}
	}

    public override void _Ready()
	{
		gm = GetNode<GameManager>("/root/GameManager");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Multiplayer.MultiplayerPeer != null && !IsMultiplayerAuthority()) {
			return;
		}

		if (!gm.isPaused) {
			Vector2 velocity = Velocity;

			Vector2 direction = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
			if (direction != Vector2.Zero)
			{
				velocity.X = direction.X * speed;
				velocity.Y = direction.Y * speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
				velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);
			}

			Velocity = velocity;
			MoveAndSlide();
		}

		if (Input.IsActionJustPressed("Pause") && !gm.isPaused) {
			var scene = GD.Load<PackedScene>("res://Scenes/pause_menu.tscn");
			AddChild(scene.Instantiate());
			gm.isPaused = true;
			GetTree().Paused = true;
		}
		else if (Input.IsActionJustPressed("Pause") && gm.isPaused) {
			Control node = GetNode<Control>("PauseMenu");
			node.QueueFree();
			gm.isPaused = false;
			GetTree().Paused = false;
		}
	}
}
