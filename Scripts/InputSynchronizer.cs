using Godot;
using System;

public partial class InputSynchronizer : MultiplayerSynchronizer
{
	private Player player;

	public Vector2 inputDirection;
	public bool inMenu = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Makes the process functions only run on the client
		// if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) {
		// 	SetProcess(false);
		// 	SetPhysicsProcess(false);
		// }

		if (!IsMultiplayerAuthority()) {
			return;
		}

		player = GetParent<Player>();
		inputDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!IsMultiplayerAuthority()) {
			return;
		}

		if (!inMenu) {
			inputDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		}
		else {
			inputDirection = Vector2.Zero;
		}
		
	}

	public override void _Process(double delta)
	{
		if (!IsMultiplayerAuthority()) {
			return;
		}

		if (Input.IsActionJustPressed("Pause") && !inMenu) {
			inMenu = true;
			var scene = GD.Load<PackedScene>("res://Scenes/pause_menu.tscn");
			AddChild(scene.Instantiate());
		}
		else if (Input.IsActionJustPressed("Pause") && inMenu) {
			inMenu = false;
			Control menu = GetNode<Control>("PauseMenu");
			menu.QueueFree();
		}
	}
}
