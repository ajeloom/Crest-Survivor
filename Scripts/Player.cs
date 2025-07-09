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

	private PackedScene projectileScene;
	private float attackCoolDown = 3.0f;
	private bool isAttacking = false;

	private Area2D attackRangeArea;     // Only detects Node2D bodies in the enemy layer
	private bool checkDistance = false;
	private Node2D target;

	/*
	 * Attack Power - how much damage a player's attack does
	 * Attack Speed - how fast a player's attack moves
	 * Attack Rate - how often a player can attack
	 * Attack Range - how far an enemy can be before a player attacks 
	 * Move Speed - how fast a player moves
	 * Dodge Chance - how often a player doesn't take damage from an attack
	*/
	private float attackPower = 1.0f;
	private float attackSpeed = 1.0f;
	private float attackRate = 1.0f;
	private float attackRange = 1.0f;
	private float moveSpeed = 1.0f;
	private float dodgeChance = 1.0f;

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
		projectileScene = GD.Load<PackedScene>("res://Scenes/projectile.tscn");
		attackRangeArea = GetNode<Area2D>("AttackRange");

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

			if (attackRangeArea.HasOverlappingBodies())
			{
				GetClosestTarget();

				if (!isAttacking && target != null)
				{
					Rpc(MethodName.SpawnProjectileRpc, new Vector2(target.Position.X - GlobalPosition.X, target.Position.Y - GlobalPosition.Y));
				}
			}
			else
			{
				target = null;
			}
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

			animTree.Set("parameters/conditions/moving", true);
			animTree.Set("parameters/conditions/idle", false);

			animTree.Set("parameters/Move/blend_position", direction);
			animTree.Set("parameters/Idle/blend_position", direction);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);

			animTree.Set("parameters/conditions/moving", false);
			animTree.Set("parameters/conditions/idle", true);
		}

		Velocity = velocity;
		MoveAndSlide();
	}


	// Pick closest enemy to attack
	private void GetClosestTarget()
	{
		if (!checkDistance)
		{
			checkDistance = true;

			CollisionShape2D shape = attackRangeArea.GetNode<CollisionShape2D>("CollisionShape2D");
			CircleShape2D circle = (CircleShape2D)shape.Shape;
			double closest = circle.Radius * 2;

			Godot.Collections.Array<Node2D> array = attackRangeArea.GetOverlappingBodies();

			// Look at every enemy inside the attack range
			for (int i = 0; i < array.Count; i++)
			{
				Node2D temp = array[i];

				// Find the distance between the enemy and player
				float xPos = temp.Position.X - Position.X;
				float yPos = temp.Position.Y - Position.Y;
				double dist = Math.Sqrt(Math.Pow(xPos, 2) + Math.Pow(yPos, 2));

				// Target the closest enemy
				if (dist < closest)
				{
					closest = dist;
					target = temp;
				}
			}

			checkDistance = false;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private async void SpawnProjectileRpc(Vector2 direction)
	{
		if (!isAttacking)
		{
			isAttacking = true;

			Projectile projectile = (Projectile)projectileScene.Instantiate();
			projectile.GlobalPosition = GlobalPosition;
			projectile.Rotation = direction.Angle();
			GetTree().Root.AddChild(projectile, true);

			await ToSignal(GetTree().CreateTimer(attackCoolDown), SceneTreeTimer.SignalName.Timeout);
			isAttacking = false;
		}
	}
}
