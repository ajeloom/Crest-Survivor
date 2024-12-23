using Godot;
using System;

public partial class MultiplayerMenu : Node2D
{
	public static MultiplayerMenu Instance { get; private set; }

	private const string DefaultServerIP = "127.0.0.1"; // IPv4 localhost
	private const int PORT = 9999;
	// private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private CanvasLayer canvasLayer;
	private PackedScene playerScene;

	private Node2D map;

	[Signal] public delegate void PlayerConnectedEventHandler(int peerId, Godot.Collections.Dictionary<string, string> playerInfo);
	[Signal] public delegate void PlayerDisconnectedEventHandler(int peerId);
    [Signal] public delegate void ServerDisconnectedEventHandler();

	private int playersLoaded = 0;

	// contains the player info for every player
	private Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> players = new Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>>();

	// local player info
	private Godot.Collections.Dictionary<string, string> playerInfo = new Godot.Collections.Dictionary<string, string>()
    {
        { "Name", "PlayerName" },
    };

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		playerScene = GD.Load<PackedScene>("res://Scenes/player.tscn");
		canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
		map = GetNode<Node2D>("Map");

		playerInfo.Clear();
		playerInfo["Name2"] = "PlayerName2"; 

		Multiplayer.PeerConnected += OnPlayerConnected;
		Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectOk;
        Multiplayer.ConnectionFailed += OnConnectionFail;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void HostButtonPressed()
	{
		// Create the server
		var peer = new ENetMultiplayerPeer();
		peer.CreateServer(PORT, 4);
		Multiplayer.MultiplayerPeer = peer;
		JoiningGame();

		// Update the player's info
		players[1] = playerInfo;
		EmitSignal(SignalName.PlayerConnected, 1, playerInfo);

		// Instantiate a player node
		if (!OS.HasFeature("dedicated_server")) {
			AddPlayer(Multiplayer.GetUniqueId());
		}
	}

	private void JoinButtonPressed()
	{
		// Join the server
		var peer = new ENetMultiplayerPeer();
		peer.CreateClient(DefaultServerIP, PORT);
		Multiplayer.MultiplayerPeer = peer;
		JoiningGame();

		if (!Multiplayer.IsServer()) {
			AddPlayer(Multiplayer.GetUniqueId());
		}
	}

	private void StartButtonPressed()
	{
		canvasLayer.Visible = false;
		// Rpc(MethodName.LoadGame, "res://Scenes/game.tscn");
	}

	private void JoiningGame()
	{
		// Hide the UI
		canvasLayer.Visible = false;
		map.Visible = true;
	}

	[Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void LoadGame(string gameScenePath)
    {
        GetTree().ChangeSceneToFile(gameScenePath);
    }

	private void AddPlayer(int playerId)
	{
		var player = playerScene.Instantiate();
		player.Name = playerId.ToString();
		AddChild(player, true);
	}

	private void Spawn(Node2D player)
	{
		var spawn = GetNode<Node2D>("Spawn1");
		player.Position = spawn.Position;
	}

	private void DeletePlayer(int playerId)
	{
		var temp = GetNode(playerId.ToString());
		temp.QueueFree();
	}

	private void OnPlayerConnected(long id)
    {
        RpcId(id, MethodName.RegisterPlayer, playerInfo);
		AddPlayer((int)id);
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(Godot.Collections.Dictionary<string, string> newPlayerInfo)
    {
        int newPlayerId = Multiplayer.GetRemoteSenderId();
        players[newPlayerId] = newPlayerInfo;
        EmitSignal(SignalName.PlayerConnected, newPlayerId, newPlayerInfo);
    }

	private void OnPlayerDisconnected(long id)
    {
		GD.Print("Player " + id.ToString() + " disconnected from the game");
		players.Remove(id);
		DeletePlayer((int)id);

		if (id == 1) {
			OnServerDisconnected();
		}
    }

	private void OnConnectOk()
    {
        int peerId = Multiplayer.GetUniqueId();
        players[peerId] = playerInfo;
        EmitSignal(SignalName.PlayerConnected, peerId, playerInfo);
    }

	private void OnConnectionFail()
    {
        Multiplayer.MultiplayerPeer = null;
    }

	// This function will disconnect any clients on the server when host disconnects
    private void OnServerDisconnected()
    {
        Multiplayer.MultiplayerPeer = null;
        players.Clear();
        EmitSignal(SignalName.ServerDisconnected);

		var gm = GetNode<GameManager>("/root/GameManager");
		gm.GoToScene("res://Scenes/host_disconnection.tscn");
    }
}
