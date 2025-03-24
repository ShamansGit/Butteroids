using Godot;
using System;
using System.IO.Compression;

public partial class MultiplayerController : Control
{
	public static MultiplayerController instance;
	[ExportCategory("Lobby Management")]
	[Export] Button hostButton;
	[Export] Button joinButton;
	[Export] Button startButton;
	[Export] Button disconnectButton;
	[ExportCategory("Character Creator")]
	[Export] LineEdit nameEdit;
	[ExportCategory("Game Log")]
	[Export] RichTextLabel gameLog;
	[ExportCategory("Chatting")]
	[Export] Button submitMessageButton;
	[Export] TextEdit messageEdit;
	[ExportCategory("Network Settings")]
	[Export] LineEdit portEdit;
	[Export] LineEdit addressEdit;
	[Export] private int port = 80;
	[Export] private string address = "127.0.0.1";
	private ENetMultiplayerPeer peer;
	private const ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;
	//todo: UI state management
	UIState UiState = UIState.NotConnected;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;
		nameEdit.Text = CreateRandomName();

		hostButton.Pressed += OnHostPressed;
		joinButton.Pressed += OnJoinPressed;
		startButton.Pressed += OnStartPressed;
		disconnectButton.Pressed += OnDisconnectButtonPressed;
		submitMessageButton.Pressed += OnChatButtonPressed;

		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;

		peer = new ENetMultiplayerPeer();

		UpdateUiState(UIState.NotConnected);
	}

    private void OnDisconnectButtonPressed()
    {
		DisconnectFromServer();
    }
	
	private void DisconnectFromServer(){
        peer.Close();
		UpdateUiState(UIState.NotConnected);

		if (GameManager.GameRunning) GameManager.EndGame();
		GameManager.Players.Clear();
		GameManager.PlayerCount = 0;
		GameManager.UniquePlayerInstances = 0;
		PrintStatus("You disconnected from the server!","purple");
	}


    private void OnChatButtonPressed()
    {
		if (messageEdit.Text == "") return;
		//todo: change this to the actual username recognised by the Game Manager.
        Rpc(nameof(MessageServer),nameEdit.Text,messageEdit.Text);
		messageEdit.Clear();
    }
	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void MessageServer(string name,string message){
		PrintStatus(name + ": " + message,"yellow");
	}

    /// <summary>
    /// runs when the connection fails and only runs on the client
    /// </summary>
    private void ConnectionFailed()
    {
        PrintStatus("Connection Failed!");
		UpdateUiState(UIState.NotConnected);
    }

	/// <summary>
	/// runs when the connection is successful and only runs on the clients
	/// </summary>
    private void ConnectedToServer()
    {
        PrintStatus("Connected to Server!","green");
		// Id of "1" means that the RPC will only run on the host.
		RpcId(1,nameof(SendPlayerInformation), nameEdit.Text,Multiplayer.GetUniqueId(),-1);
		UpdateUiState(UIState.ConnectedAsClient);
    }

	/// <summary>
	/// runs when a player disconnects and runs on all peers
	/// </summary>
	/// <param name="id">id of the player that disconnected</param>
    private void PeerDisconnected(long id)
    {
        PrintStatus(GameManager.Players[id].Name.Capitalize() + " disconnected! (id:" + id.ToString() + ")","purple");
		GameManager.RemovePlayer(id);
		//if the peer that disconnected was the host, disconnect as well.
		if (id == 1){
			DisconnectFromServer();
		}
    }

	/// <summary>
	/// runs when a player connects and runs on all peers
	/// </summary>
	/// <param name="id">id of the player that connects</param>
    private void PeerConnected(long id)
    {
        //PrintStatus("Player Connected: " + id.ToString(),"green");
    }
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("toggle_menu")){
			Visible = !Visible;
		}
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerInformation(string name,long id,int playerInstanceNumber = -1){
		PlayerInfo playerInfo = new PlayerInfo(){
			Name = name,
			//if playerInstanceNumber == -1, then use UniquePlayerInstances as number instead
			//this will ensure a unique player colour for each player
			//color = GameManager.GeneratePlayerColor(playerInstanceNumber == -1 ? GameManager.UniquePlayerInstances : playerInstanceNumber)
		};

		if (!GameManager.Players.ContainsKey(id)){
			GameManager.AddPlayer(id,playerInfo);
			PrintStatus(name.Capitalize() + " joined the game! (id:" + id.ToString() +")" ,"green");
		}

		if (Multiplayer.IsServer()){
			foreach (long idKey in GameManager.Players.Keys)
			{
				Rpc(nameof(SendPlayerInformation),GameManager.Players[idKey].Name,idKey,GameManager.UniquePlayerInstances - 1);
				GameManager.UniquePlayerInstances += 1;
			}
		}
	}
	public void OnHostPressed(){
		GetPortAndAddress();
		var error = peer.CreateServer(port,32);
		if (error != Error.Ok){
			PrintStatus("Cannot Host! - Error: " + error.ToString(),"red");
			return;
		}
		//Set how the packets will be compressed
		peer.Host.Compress(compressionMode);

		//Sets peer as yourself, so you can play your own game as a host
		Multiplayer.MultiplayerPeer = peer;
		SendPlayerInformation(nameEdit.Text,1,0);

		PrintStatus("Waiting For Players...");

		UpdateUiState(UIState.ConnectedAsHost);
	}
	public void OnJoinPressed(){
		GetPortAndAddress();
		var error = peer.CreateClient(address,port);
		if (error != Error.Ok) {
			PrintStatus("Cannot connect to Server! - Error: " + error.ToString(),"red");
			return;
		}
		peer.Host.Compress(compressionMode);
		Multiplayer.MultiplayerPeer = peer;
		PrintStatus("Joining Game...");
		UpdateUiState(UIState.JoiningAsClient);
	}
	public void OnStartPressed(){
		Rpc(nameof(StartGame));
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame(){
		GameManager.StartGame();
		UpdateUiState(UIState.InGame);
		Hide();
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReturnToLobby(){
		GameManager.EndGame();
		UpdateUiState(UIState.ConnectedAsHost); //todo: ensure this isnt true for clients
		Show();
	}
	public void PrintStatus(string newStatus,string color = "white"){
		gameLog.Text += "\n[color=" + color +"] > " + newStatus + "[/color]";
		GD.Print(newStatus);
	}
	public void GetPortAndAddress(){
		try{
			if (portEdit.Text != "") port = portEdit.Text.ToInt();
			if (addressEdit.Text != "") address = addressEdit.Text;
		}catch{
			PrintStatus("Error parsing custom port and/or address!");
		}
	}
	public void UpdateUiState(UIState newState){
		UiState = newState;

		hostButton.Disabled = UiState != UIState.NotConnected;
		joinButton.Disabled = UiState != UIState.NotConnected;
		startButton.Disabled = UiState != UIState.ConnectedAsHost || UiState == UIState.InGame;
		disconnectButton.Disabled = UiState == UIState.NotConnected;

		nameEdit.Editable = UiState == UIState.NotConnected;
		messageEdit.Editable = UiState != UIState.NotConnected && UiState != UIState.JoiningAsClient;
		submitMessageButton.Disabled = UiState == UIState.NotConnected || UiState == UIState.JoiningAsClient;

		portEdit.Editable = UiState == UIState.NotConnected;
		addressEdit.Editable = UiState == UIState.NotConnected;
	}
	public string CreateRandomName(){
		string output = "";
		int syllableCount = GD.RandRange(2,4);
		string[] syllables = new string[]{
			"go","ga","arp","orp","ug","a","ve","ple","nop","gan","le","pe","se","ne","an","la"
		};
		for (int i = 0; i < syllableCount; i++)
		{
			output += syllables[GD.RandRange(0,syllables.Length - 1)];
		}
		return output.Capitalize();
	}
}
