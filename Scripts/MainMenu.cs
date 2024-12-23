using Godot;
using System;

public partial class MainMenu : Control
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

	private void PlayButtonPressed() 
	{
		gm.GoToScene("res://Scenes/main.tscn");
	}

	private void MultiplayerButtonPressed()
	{
		gm.GoToScene("res://Scenes/multiplayer_menu.tscn");
	}

	private void SettingsButtonPressed()
	{
		gm.GoToScene("res://Scenes/settings.tscn");
	}

	private void QuitButtonPressed()
	{
		GetTree().Quit();
	}
}
