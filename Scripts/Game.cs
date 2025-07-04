using Godot;
using System;

public partial class Game : Node2D
{
	private PackedScene playerScene;
	private PackedScene enemyScene;

	public bool gameStarted = false;

	private bool playersSpawned = false;
	private bool enemiesSpawned = false;

	private MultiplayerSpawner enemySpawner;

	int playerNum = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// enemySpawner = GetNode<MultiplayerSpawner>("EnemySpawner");
		// enemySpawner.SpawnFunction = SpawnEnemies();

		playerScene = GD.Load<PackedScene>("res://Scenes/player.tscn");
		enemyScene = GD.Load<PackedScene>("res://Scenes/general.tscn");

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
	public async override void _Process(double delta)
	{
		if (!Multiplayer.IsServer()) {
			return;
		}

		// Spawn enemies every 8 seconds
		if (!enemiesSpawned) {
			enemiesSpawned = true;
			CallDeferred(MethodName.SpawnEnemies, null);
			await ToSignal(GetTree().CreateTimer(8.0f), SceneTreeTimer.SignalName.Timeout);
			enemiesSpawned = false;
		}
	}

	public void AddPlayer(int playerId)
	{
		Node2D players = GetNode<Node2D>("Players");
		var player = playerScene.Instantiate();
		player.Name = playerId.ToString();
		players.AddChild(player, true);
		Rpc(MethodName.PositionPlayerRpc, playerNum);
		playerNum++;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void DeletePlayer(int playerId)
	{
		var temp = GetNode("Game/Players/"+ playerId.ToString());
		temp.QueueFree();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void PositionPlayerRpc(int i)
	{
		Node2D players = GetNode<Node2D>("Players");
		Node2D player = (Node2D)players.GetChild(i);
		Node2D spawn = GetNode<Node2D>("PlayerSpawns/Spawn" + i.ToString());
		player.Position = spawn.Position;
	}

	private void SpawnEnemies()
	{
		Node2D enemies = GetNode<Node2D>("Enemies");
		Node2D enemy = (Node2D)enemyScene.Instantiate();

		// Set the enemy's position
		enemy.Position = GetRandomSpawnPosition();

		// Spawn them
		enemies.AddChild(enemy, true);
	}

	private Vector2 GetRandomSpawnPosition()
	{
		// Gets a random number between 0 and less than 4
		Random rnd = new Random();
		int num = rnd.Next(4);

		Node2D spawn = GetNode<Node2D>("Spawns/Spawn" + num.ToString());

		return spawn.Position;
	}
}
