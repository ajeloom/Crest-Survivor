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
		Unpause();
		gm.GoToScene("res://Scenes/main_menu.tscn");
	}
}
