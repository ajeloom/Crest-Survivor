using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float speed = 75.0f;

	private int playerId;

	private GameManager gm;
	private InputSynchronizer input;
	private Camera2D camera;
	private CanvasLayer hud;

	private Sprite2D sprite;
	private AnimationPlayer animPlayer;
	private AnimationTree animTree;

	public override void _EnterTree()
	{
		playerId = Convert.ToInt32(Name);

		if (Multiplayer.MultiplayerPeer != null)
		{
			SetMultiplayerAuthority(playerId);
		}
	}

	public override void _Ready()
	{
		gm = GetNode<GameManager>("/root/GameManager");
		camera = GetNode<Camera2D>("Camera2D");
		hud = GetNode<CanvasLayer>("HealthComponent/CanvasLayer");
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animTree = GetNode<AnimationTree>("AnimationTree");
		sprite = GetNode<Sprite2D>("Sprite2D");

		if (Multiplayer.GetUniqueId() == playerId)
		{
			camera.MakeCurrent();
		}
		else
		{
			camera.Enabled = false;
			hud.Visible = false;
		}

		if (!IsMultiplayerAuthority())
		{
			return;
		}

		input = GetNode<InputSynchronizer>("InputSynchronizer");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Control the player's movement locally
		if (IsMultiplayerAuthority() && input != null)
		{
			Movement();
		}

		// if (input != null)
		// {
		// 	PlayAnimation();
		// }
		
	}

	public override void _Process(double delta)
	{
	}

	private void Movement()
	{
		Vector2 velocity = Velocity;

		Vector2 direction = input.inputDirection;
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * speed;
			velocity.Y = direction.Y * speed;


			animTree.Set("parameters/conditions/moving", true);
			animTree.Set("parameters/conditions/idle", false);

			// if (velocity.X > 0.0f)
			// {
			// 	// sprite.FlipH = true;
			// 	// animPlayer.Play("MoveRight");
			// }
			// else
			// {
			// 	// sprite.FlipH = false;
			// 	// animPlayer.Play("MoveLeft");
			// }

			animTree.Set("parameters/Move/blend_position", direction);
			animTree.Set("parameters/Idle/blend_position", direction);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);

			// animPlayer.Play("Idle");

			animTree.Set("parameters/conditions/moving", false);
			animTree.Set("parameters/conditions/idle", true);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// private void PlayAnimation()
	// {
	// 	Vector2 direction = input.inputDirection;
	// 	if (direction != Vector2.Zero)
	// 	{
	// 		animTree.Set("parameters/conditions/moving", true);
	// 		animTree.Set("parameters/conditions/idle", false);
	// 		animTree.Set("parameters/Move/blend_position", direction);
	// 		animTree.Set("parameters/Idle/blend_position", direction);
	// 	}
	// 	else
	// 	{
	// 		animTree.Set("parameters/conditions/moving", false);
	// 		animTree.Set("parameters/conditions/idle", true);
	// 	}
	// }
}
