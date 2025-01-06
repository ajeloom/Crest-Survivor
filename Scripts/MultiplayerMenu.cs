using Godot;
using System;

public partial class MultiplayerMenu : Control
{
	public static MultiplayerMenu Instance { get; private set; }

	private const string DefaultServerIP = "127.0.0.1"; // IPv4 localhost
	private const int PORT = 9999;
	private ENetMultiplayerPeer peer;

	private CanvasLayer server;
	private CanvasLayer lobby;
	private PackedScene playerScene;
	private PackedScene enemyScene;

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
		enemyScene = GD.Load<PackedScene>("res://Scenes/enemy.tscn");
		server = GetNode<CanvasLayer>("Server");
		lobby = GetNode<CanvasLayer>("Lobby");

		playerInfo.Clear();
		// playerInfo["Name2"] = "PlayerName2";

		// Will run these functions when these signals occur
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

	private Error HostButtonPressed()
	{
		// Create the server
		peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(PORT, 4);
		if (error != Error.Ok) {
			return error;
		}

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;

		JoinLobby();

		// Update the player's info
		players[1] = playerInfo;
		EmitSignal(SignalName.PlayerConnected, 1, playerInfo);

		return Error.Ok;
	}

	private Error JoinButtonPressed()
	{
		// Join the server
		peer = new ENetMultiplayerPeer();
		Error error = peer.CreateClient(DefaultServerIP, PORT);
		if (error != Error.Ok) {
			return error;
		}
		
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;

		JoinLobby();

		return Error.Ok;
	}

	private void StartButtonPressed()
	{
		peer.RefuseNewConnections = true;
		Rpc(MethodName.LoadGame, "res://Scenes/game.tscn");
	}

	private void BackButtonPressed()
	{
		if (Multiplayer.IsServer()) {
			ClearServer();
		}

		var gm = GetNode<GameManager>("/root/GameManager");
		gm.GoToScene("res://Scenes/main_menu.tscn");
	}

	

	private void JoinLobby()
	{
		// Hide the UI
		server.Visible = false;
		lobby.Visible = true;

		if (!Multiplayer.IsServer()) {
			Button startButton = GetNode<Button>("Lobby/StartButton");
			startButton.Visible = false;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void LoadGame(string gameScenePath)
    {
		var scene = ResourceLoader.Load<PackedScene>(gameScenePath);
		Node node = scene.Instantiate();
		GetTree().Root.AddChild(node);
		lobby.Visible = false;
    }

	// Every peer will call this when they have loaded the game scene.
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void PlayerLoaded()
    {
        if (Multiplayer.IsServer())
        {
            playersLoaded += 1;
            if (playersLoaded == players.Count)
            {
                GetNode<Game>("/root/Game").StartGame();
                playersLoaded = 0;
            }
        }
    }

	private void OnPlayerConnected(long id)
    {
		GD.Print("Player " + id.ToString() + " joined the game");
        RpcId(id, MethodName.RegisterPlayer, playerInfo);
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
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
    }

	// This function will disconnect any clients on the server when host disconnects
    private void OnServerDisconnected()
    {
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
        players.Clear();
        EmitSignal(SignalName.ServerDisconnected);

		Game game = GetNodeOrNull<Game>("/root/Game");
		if (game != null) {
			game.QueueFree();
		}

		var gm = GetNode<GameManager>("/root/GameManager");
		gm.GoToScene("res://Scenes/host_disconnection.tscn");
    }

	public void ClearServer()
	{
		var peer = (ENetMultiplayerPeer)Multiplayer.MultiplayerPeer;
		peer.Host.Dispose();
        Multiplayer.MultiplayerPeer.Dispose();
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private Error CanPlayerJoin()
	{
		if (Multiplayer.MultiplayerPeer.RefuseNewConnections == true) {
			return Error.CantConnect;
		}

		Game game = GetNodeOrNull<Game>("/root/Game");
		if (game == null) {
			return Error.Ok;
		}

		if (game.gameStarted) {
			return Error.Failed;
		}

		return Error.Ok;
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private Error HostDisconnect(long id)
	{
		Multiplayer.MultiplayerPeer.DisconnectPeer((int)id);
		return Error.Ok;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private Error DisconnectPlayer()
	{
		GD.Print("Can't join the server. Game is in progress.");
		
		// Multiplayer.MultiplayerPeer.Close();
		// Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();

		// var gm = GetNode<GameManager>("/root/GameManager");
		// gm.GoToScene("res://Scenes/main_menu.tscn");

		return Error.CantConnect;
	}
}
