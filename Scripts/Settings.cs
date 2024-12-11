using Godot;
using System;

public partial class Settings : Control
{
	string savePath = "user://settings.ini";
	private ConfigFile configFile = new ConfigFile();

	private float fps;
	private bool displayFPS;

	private GameManager gm;
	[Export] private SpinBox FPSSpinBox;
	[Export] private CheckBox checkBox;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gm = GetNode<GameManager>("/root/GameManager");

		// Load saved settings
		Load();

		FPSSpinBox.Value = fps;
		checkBox.ButtonPressed = displayFPS;
		
		ShowApplyButton(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Pause")) {
			BackButtonPressed();
		}
	}

	private void ApplyButtonPressed()
	{
		// Save settings that require the apply button to be saved
		Engine.MaxFps = (int)fps;
		configFile.SetValue("Settings", "MaxFPS", fps);
		Save();
		ShowApplyButton(false);
	}

	private void BackButtonPressed()
	{
		gm.GoToScene("res://Scenes/main_menu.tscn");
	}

	private void FPSValueChanged(float value)
	{
		fps = value;
		ShowApplyButton(true);
	}

	private void DisplayFPSPressed()
	{
		if (checkBox.ButtonPressed) {
			// Show FPS
			var nextScene = GD.Load<PackedScene>("res://Scenes/fps_display.tscn");
			GetTree().Root.AddChild(nextScene.Instantiate(), true);
			Control node = GetTree().Root.GetNode<Control>("FPSDisplay");
			GetTree().Root.MoveChild(node, 1);
		}
		else {
			// Don't show FPS
			Control scene = GetTree().Root.GetNode<Control>("FPSDisplay");
			if (scene != null) {
				scene.QueueFree();
			}
		}

		configFile.SetValue("Settings", "DisplayFPS", checkBox.ButtonPressed);
		Save();
	}

	private void Save()
	{
		configFile.Save(savePath);
	}

	private void Load()
	{
		// Load the saved settings
		Error error = configFile.Load(savePath);
		if (error != Error.Ok) {
			return;
		}

		fps = (float)configFile.GetValue("Settings", "MaxFPS");
		displayFPS = (bool)configFile.GetValue("Settings", "DisplayFPS");
	}

	private void ShowApplyButton(bool value)
	{
		Button button = GetNode<Button>("ApplyButton");
		button.Visible = value;
	}
}
