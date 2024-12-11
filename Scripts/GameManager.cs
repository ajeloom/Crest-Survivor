using Godot;
using System;

public partial class GameManager : Node2D
{
	string savePath = "user://settings.ini";
	private ConfigFile configFile = new ConfigFile();

	public Node currentScene { get; set; }

	public bool isPaused = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Viewport root = GetTree().Root;
		currentScene = root.GetChild(root.GetChildCount() - 1);

		// Load title screen
		GoToScene("res://Scenes/main_menu.tscn");

		CallDeferred(MethodName.LoadSettings, null);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void LoadScene(string name) {
		// It is now safe to remove the current scene.
		currentScene.Free();

		// Load a new scene.
		var nextScene = GD.Load<PackedScene>(name);

		// Instance the new scene.
		currentScene = nextScene.Instantiate();

		// Add it to the active scene, as child of root.
		GetTree().Root.AddChild(currentScene);

		// Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
		GetTree().CurrentScene = currentScene;
	}

	public void GoToScene(string path)
	{
		// This function will usually be called from a signal callback,
		// or some other function from the current scene.
		// Deleting the current scene at this point is
		// a bad idea, because it may still be executing code.
		// This will result in a crash or unexpected behavior.

		// The solution is to defer the load to a later time, when
		// we can be sure that no code from the current scene is running:

		CallDeferred(MethodName.LoadScene, path);
	}

	private void LoadSettings()
	{
		// Load the saved settings
		Error error = configFile.Load(savePath);
		if (error != Error.Ok) {
			return;
		}

		bool displayFPS = (bool)configFile.GetValue("Settings", "DisplayFPS");
		if (displayFPS) {
			var nextScene = GD.Load<PackedScene>("res://Scenes/fps_display.tscn");
			GetTree().Root.AddChild(nextScene.Instantiate(), true);
			Control node = GetTree().Root.GetNode<Control>("FPSDisplay");
			GetTree().Root.MoveChild(node, 1);
		}

		float fps = (float)configFile.GetValue("Settings", "MaxFPS");
		Engine.MaxFps = (int)fps;
	}
}
