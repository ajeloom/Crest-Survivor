using Godot;
using System;

public partial class PauseMenu : Control
{
	private GameManager gm;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gm = GetNode<GameManager>("/root/GameManager");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void Unpause()
	{
		InputSynchronizer input = GetParent<InputSynchronizer>();
		input.inMenu = false;
		gm.isPaused = false;
		GetTree().Paused = false;
	}

	private void ResumeButtonPressed()
	{
		Unpause();
		QueueFree();
	}

	private void QuitButtonPressed()
	{
		if (Multiplayer.HasMultiplayerPeer()) {
			if (Multiplayer.IsServer()) {
				MultiplayerMenu menu = GetNode<MultiplayerMenu>("/root/MultiplayerMenu");
				menu.ClearServer();
			}
			else {
				Multiplayer.MultiplayerPeer.Close();
				Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
			}

			// Close multiplayer game
			var temp = GetTree().Root.GetNode("Game");
			temp.QueueFree();
		}

		Unpause();
		gm.GoToScene("res://Scenes/main_menu.tscn");
	}
}
