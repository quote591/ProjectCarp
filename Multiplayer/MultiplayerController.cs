/// <summary>
/// 
/// Welcome to the Multiplayer Controller!
/// 
/// Handles all the networking for making a server and loading into the first scene
/// 
/// </summary>

using Godot;
using System;
using System.Diagnostics.SymbolStore;
using System.Linq;


public partial class MultiplayerController : Control
{
    // Here is the first scene the game will load into when a user presses start
    // Set it in the inspector
    [Export]
    public PackedScene MultiplayerScene { get; set; }

    [Export]
    public PackedScene BackgroundScene { get; set; }

    [Export]
    private int port = 8910;
    // I would recommend ports:
    // 49152 – 65535

    [Export]
    private string address = "127.0.0.1";
    // For local use: 127.0.0.1

    [Export]
    private int numberOfPlayers = 8;
    // i know this seems like its easy to just increase the number of players
    // however before you do that, make sure all scenes have 8 player spawn locations

    private ENetMultiplayerPeer peer;

    private string[] listOfLobbyNames;
    private long[] listOfLobbyIds;
    private int numberOfPlayersInLobby = 1;

    public override void _Ready()
    {
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
        Multiplayer.ServerDisconnected += ServerDisconnected;

        var sceneBack = BackgroundScene.Instantiate<Node3D>();
        GetTree().Root.CallDeferred("add_child", sceneBack);

        // this just captures the mouse again when server is ready
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void ServerDisconnected()
    {
        GD.Print("Disconnected from server!");
        RestartGame();
    }


    private void ConnectionFailed()
    {
        GD.Print("Connection Failed!");
        GetNode<Label>("LoobyC/VBoxContainer/LabelHost").Text = "Connection Failed ;(";
        GetNode<Button>("VBC/Go Back").Visible = true;
    }

    private void ConnectedToServer()
    {
        GD.Print("Connected To Server!");
        RpcId(1, "SendPlayerInformation", GetNode<LineEdit>("VBC/HB Name/NameLineEdit").Text, Multiplayer.GetUniqueId()); // only sends RPC call to the host aka 1
        GetNode<Label>("LoobyC/VBoxContainer/LabelHost").Text = "Lobby";
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Player " + id.ToString() + " Disconnected!");
        GD.Print($"Player {id} disconnected!");

        // Remove from GameManager
        var playerToRemove = GameManager.Players.FirstOrDefault(p => p.Id == id);
        if (playerToRemove != null)
        {
            GameManager.Players.Remove(playerToRemove);
        }

        // Update UI (e.g., lobby display)
        UpdateLobbyUI();
    }

    public void PeerConnected(long id)
    {
        GD.Print("Found New Player! \tPlayer " + id.ToString());
    }

    public void _on_host_button_down()
    {
        peer = new ENetMultiplayerPeer();
        port = int.Parse(GetNode<LineEdit>("VBC/HB Port/PortLineEdit").Text);
        GD.Print("Using Port: " + port);
        var error = peer.CreateServer(port, numberOfPlayers);
        if (error != Error.Ok)
        {
            GD.Print("error cannot host!: " + error.ToString());
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Waiting For Players!");

        // will get the canvas name and pass it to itself when hosting
        GetNode<Label>("LoobyC/VBoxContainer/LabelHost").Text = "Lobby (Host)";
        GetNode<Button>("VBC/Host").Hide();
        GetNode<Button>("VBC/Join").Hide();
        GetNode<Button>("VBC/Start Game").Visible = true;
        SendPlayerInformation(GetNode<LineEdit>("VBC/HB Name/NameLineEdit").Text, 1);


        HideNameIpPort();
    }

    public void _on_join_button_down()
    {
        peer = new ENetMultiplayerPeer();
        address = GetNode<LineEdit>("VBC/HB IP/IPLineEdit").Text;
        port = int.Parse(GetNode<LineEdit>("VBC/HB Port/PortLineEdit").Text);
        GD.Print("Using address: " + address);
        GD.Print("Using Port: " + port);
        peer.CreateClient(address, port);

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Joining Game!");
        GetNode<Label>("LoobyC/VBoxContainer/LabelHost").Text = "Attempting to join...";
        HideNameIpPort();
    }

    public void _on_start_game_button_down()
    {
        Rpc("startGame");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void startGame()
    {
        // change in inspector window with what you want the first scene to be
        if (MultiplayerScene != null)
        {
            var scene = MultiplayerScene.Instantiate<Node3D>();
            GetTree().Root.AddChild(scene);
        }
        else
        {
            GD.PrintErr("No scene assigned in the Inspector!");
        }
        this.Hide(); // hide the canvas screen so player can play
    }

    // for adding players to the GamaManger PlayerInfo
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendPlayerInformation(string name, long id)
    {
        PlayerInfo playerInfo = new PlayerInfo()
        {
            Name = name,
            Id = id
        };

        // checks if player has already registered, if yes, dont add
        bool isInThere = false;
        foreach (var test in GameManager.Players)
        {
            if (test.Id == playerInfo.Id)
            {
                isInThere = true;
            }
        }
        if (!isInThere)
        {
            GameManager.Players.Add(playerInfo);
            AddPlayerToLobby(name, id);
        }

        // when receiving a new connection, send out all current players to everyone
        if (Multiplayer.IsServer())
        {
            foreach (var item in GameManager.Players)
            {
                Rpc("SendPlayerInformation", item.Name, item.Id);
            }
        }
    }

    // Non Multiplayer UI
    public void _on_quit_button_down()
    {
        GetTree().Quit();
    }

    public void _on_go_back_button_down()
    {
        GetNode<Label>("VBC/HB Name/Name").Visible = true;
        GetNode<LineEdit>("VBC/HB Name/NameLineEdit").Visible = true;

        GetNode<Label>("VBC/HB IP/IP").Visible = true;
        GetNode<LineEdit>("VBC/HB IP/IPLineEdit").Visible = true;

        GetNode<Label>("VBC/HB Port/Port").Visible = true;
        GetNode<LineEdit>("VBC/HB Port/PortLineEdit").Visible = true;

        GetNode<Button>("VBC/Host").Visible = true;
        GetNode<Button>("VBC/Join").Visible = true;

        GetNode<Button>("VBC/Go Back").Hide();
        GetNode<Label>("LoobyC/VBoxContainer/LabelHost").Text = "";
    }

    public void AddPlayerToLobby(string name, long id)
    {
        GetNode<Label>("LoobyC/VBoxContainer/Player" + numberOfPlayersInLobby + "C/Player" + numberOfPlayersInLobby + "Name").Text = name;
        GetNode<Label>("LoobyC/VBoxContainer/Player" + numberOfPlayersInLobby + "C/Player" + numberOfPlayersInLobby + "UniqueID").Text = id.ToString();
        numberOfPlayersInLobby++;
    }

    public void HideNameIpPort()
    {
        GetNode<Label>("VBC/HB Name/Name").Hide();
        GetNode<LineEdit>("VBC/HB Name/NameLineEdit").Hide();

        GetNode<Label>("VBC/HB IP/IP").Hide();
        GetNode<LineEdit>("VBC/HB IP/IPLineEdit").Hide();

        GetNode<Label>("VBC/HB Port/Port").Hide();
        GetNode<LineEdit>("VBC/HB Port/PortLineEdit").Hide();

        GetNode<Button>("VBC/Host").Hide();
        GetNode<Button>("VBC/Join").Hide();
    }

    public void _on_settings_button_down()
    {
        GetNode<Control>("Settings").Visible = true;
    }

    public void RestartGame()
    {
        Multiplayer.MultiplayerPeer = null;
        GameManager.Players.Clear();
        foreach (Node child in GetTree().Root.GetChildren())
        {
            if (child.Name != "MultiplayerController" && child.Name != "GameManager")
            {
                child.QueueFree();
            }
        }
        GetTree().ChangeSceneToFile("res://Multiplayer/MainMenu.tscn");
    }

    private void UpdateLobbyUI()
    {
        for (int i = 1; i <= numberOfPlayers; i++)
        {
            GetNode<Label>($"LoobyC/VBoxContainer/Player{i}C/Player{i}Name").Text = "";
            GetNode<Label>($"LoobyC/VBoxContainer/Player{i}C/Player{i}UniqueID").Text = "";
        }

        int slot = 1;
        foreach (var player in GameManager.Players)
        {
            GetNode<Label>($"LoobyC/VBoxContainer/Player{slot}C/Player{slot}Name").Text = player.Name;
            GetNode<Label>($"LoobyC/VBoxContainer/Player{slot}C/Player{slot}UniqueID").Text = player.Id.ToString();
            slot++;
        }

        numberOfPlayersInLobby = slot;
    }
}
