using Godot;
using System;

public partial class MultiplayerController : Control
{
    [Export]
    private int port = 8910;

    [Export]
    private string address = "127.0.0.1";
    // 127.0.0.1

    [Export]
    private int numberOfPlayers = 8;

    private ENetMultiplayerPeer peer;

    public override void _Ready()
    {
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
    }

    private void ConnectionFailed()
    {
        GD.Print("Connection Failed!");
    }

    private void ConnectedToServer()
    {
        GD.Print("Connected To Server!");
        RpcId(1, "SendPlayerInformation", GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId()); // host only sends info
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Player " + id.ToString() + " Disconnected!");
    }

    public void PeerConnected(long id)
    {
        GD.Print("Player " + id.ToString() + " Connected!");
    }

    public void _on_host_button_down()
    {
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port, numberOfPlayers);
        if (error != Error.Ok)
        {
            GD.Print("error cannot host!: " + error.ToString());
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Waiting For Players!");

        SendPlayerInformation(GetNode<LineEdit>("LineEdit").Text, 1); // register to ourselves 
    }

    public void _on_join_button_down()
    {
        peer = new ENetMultiplayerPeer();
        peer.CreateClient(address, port);

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Joining Game!");
    }

    public void _on_start_game_button_down()
    {
        Rpc("startGame");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void startGame()
    {

        GD.Print("Starting game with connected players:");

        foreach (var playerInfo in GameManager.Players)
        {
            GD.Print($"Player Name: {playerInfo.Name}, Player ID: {playerInfo.Id}");
        }

        var scene = ResourceLoader.Load<PackedScene>("res://Multiplayer/TestMultiplayerScene.tscn").Instantiate<Node3D>();
        GetTree().Root.AddChild(scene);
        this.Hide(); // hide the canvas screen
    }

    // sending player information
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendPlayerInformation(string name, int id)
    {
        PlayerInfo playerInfo = new PlayerInfo()
        {
            Name = name,
            Id = id
        };
        if (!GameManager.Players.Contains(playerInfo))
        {
            GameManager.Players.Add(playerInfo);
        }
        if (Multiplayer.IsServer())
        {
            foreach (var item in GameManager.Players)
            {
                Rpc("SendPlayerInformation", item.Name, item.Id);
            }
        }
    }


    
}
