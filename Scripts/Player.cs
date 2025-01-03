using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float speed = 50.0f;

	private int playerId;

	private GameManager gm;
	private InputSynchronizer input;
	private Camera2D camera;
	private CanvasLayer hud;

	public override void _EnterTree()
	{
		playerId = Convert.ToInt32(Name);

		if (Multiplayer.MultiplayerPeer != null) {
			SetMultiplayerAuthority(playerId);
		}
	}

    public override void _Ready()
	{
		gm = GetNode<GameManager>("/root/GameManager");
		camera = GetNode<Camera2D>("Camera2D");
		hud = GetNode<CanvasLayer>("HealthComponent/CanvasLayer");

		if (Multiplayer.GetUniqueId() == playerId) {
			camera.MakeCurrent();
		}
		else {
			camera.Enabled = false;
			hud.Visible = false;
		}

		if (!IsMultiplayerAuthority()) {
			return;
		}

		input = GetNode<InputSynchronizer>("InputSynchronizer");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Control the player's movement locally
		if (IsMultiplayerAuthority()) {
			Movement();
		}
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
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
