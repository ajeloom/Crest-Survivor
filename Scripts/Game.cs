using Godot;
using System;

public partial class Game : Node2D
{
	private PackedScene playerScene;
	private PackedScene enemyScene;

	public bool gameStarted = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerScene = GD.Load<PackedScene>("res://Scenes/player.tscn");
		enemyScene = GD.Load<PackedScene>("res://Scenes/enemy.tscn");

		MultiplayerMenu.Instance.RpcId(1, MultiplayerMenu.MethodName.PlayerLoaded);

		// if (Multiplayer.IsServer()) {
		// 	MultiplayerMenu.Instance.RpcId(1, MultiplayerMenu.MethodName.PlayerLoaded);
		// }
		// else {
		// 	Error error = RpcId(1, MethodName.DidGameStart, null);
		// 	if (error != Error.Ok) {
		// 		// Joined before game starts
		// 		MultiplayerMenu.Instance.RpcId(1, MultiplayerMenu.MethodName.PlayerLoaded);
		// 	}
		// 	else {
		// 		// Joined during the game
		// 		// GD.Print("Joined when game in progress");
		// 		// RpcId(1, MethodName.OnPlayerConnected, Multiplayer.MultiplayerPeer.GetUniqueId());
		// 		AddPlayer(Multiplayer.MultiplayerPeer.GetUniqueId());
		// 		// AddPlayer(Multiplayer.GetUniqueId());

		// 		// // Add all the clients
		// 		// foreach (int id in Multiplayer.GetPeers()) {
		// 		// 	AddPlayer(id);
		// 		// }
		// 	}
		// }
	}

	// Called only on the server.
	// All peers are ready to receive RPCs in this scene.
    public void StartGame()
    {
		if (!Multiplayer.IsServer()) {
			return;
		}

		// Add host
		AddPlayer(Multiplayer.GetUniqueId());

		// Add all the clients
		foreach (int id in Multiplayer.GetPeers()) {
			AddPlayer(id);
		}

		// Spawn enemies
		CallDeferred(MethodName.SpawnEnemies, null);

		gameStarted = true;
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private Error DidGameStart()
	{
		if (!gameStarted) {
			// Multiplayer.MultiplayerPeer.DisconnectPeer((int)id);
			return Error.Failed;
		}

		return Error.Ok;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddPlayer(int playerId)
	{
		Node2D players = GetNode<Node2D>("Players");
		var player = playerScene.Instantiate();
		player.Name = playerId.ToString();
		players.AddChild(player, true);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void DeletePlayer(int playerId)
	{
		var temp = GetNode("Game/Players/"+ playerId.ToString());
		temp.QueueFree();
	}

	private void SpawnPlayer(Node2D player)
	{
		var spawn = GetNode<Node2D>("Spawn1");
		player.Position = spawn.Position;
	}

	private void SpawnEnemies()
	{
		Node2D enemies = GetNode<Node2D>("Enemies");
		Node2D enemy = (Node2D)enemyScene.Instantiate();

		// Set the enemy's position
		// enemy.Position = ;

		// Spawn them
		enemies.AddChild(enemy, true);
	}
}
