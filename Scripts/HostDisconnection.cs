using Godot;
using System;

public partial class HostDisconnection : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ButtonPressed()
	{
		var gm = GetNode<GameManager>("/root/GameManager");
		gm.GoToScene("res://Scenes/main_menu.tscn");
	}
}
