using Godot;
using System;
using System.Diagnostics.SymbolStore;
using System.Linq;

public partial class MultiplayerController : Control
{
    [Export]
    private int port = 8910;

    [Export]
    private string address = "127.0.0.1";
    // 127.0.0.1

    [Export]
    private int numberOfPlayers = 8;

    private int numberOfPlayersOnline = 0;

    private ENetMultiplayerPeer peer;

    private int howManyPeopleDoIThinkConnected = 1; // its one because we are assuming there is always a host

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
        GD.Print("[ConnectToServer] LineEdit: " + GetNode<LineEdit>("LineEdit").Text);
        GD.Print("[ConnectToServer] GetUniqueId: " + Multiplayer.GetUniqueId());
        RpcId(1, "SendPlayerInformation", GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId()); // host only sends info
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Player " + id.ToString() + " Disconnected!");
    }

    public void PeerConnected(long id)
    {
        GD.Print("[" + Multiplayer.GetUniqueId() + "] " + "Player " + id.ToString() + " Is in the Lobby!");
        howManyPeopleDoIThinkConnected++;
        GD.Print("[" + Multiplayer.GetUniqueId() + "] [IMPORTANT] people count:" + howManyPeopleDoIThinkConnected.ToString());
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

        howManyPeopleDoIThinkConnected = 1; // its one because we are the host?
        GD.Print("[" + Multiplayer.GetUniqueId() + "] [IMPORTANT] people count:" + howManyPeopleDoIThinkConnected.ToString());
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
        GD.Print("===== [" + Multiplayer.GetUniqueId() + "] " + "HAS STARTED THE GAME =====");
        Rpc("startGame");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void startGame()
    {

        GD.Print("Starting game with connected players:");

        numberOfPlayersOnline = GameManager.Players.Count;
        GD.Print("[startGame()] Trying to connect " + numberOfPlayersOnline + " Players!");
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
    private void SendPlayerInformation(string name, long id)
    {
        GD.Print("[" + Multiplayer.GetUniqueId() + "] " + "===== Adding new players =====");
        PlayerInfo playerInfo = new PlayerInfo()
        {
            Name = name,
            Id = id
        };
        //GameManager.Players.Add(playerInfo);
        GD.Print("["+Multiplayer.GetUniqueId()+"] "+"Attempting to add:\t" + playerInfo.Id + "\tname:\t" + playerInfo.Name);

        // Contains is fucked, this is the bodge temp solution to adding to local maps
        bool isInThere = false;
        foreach (var test in GameManager.Players)
        {
            GD.Print("["+Multiplayer.GetUniqueId()+"] "+"[ARE THESE THE SAME] " + test.Id + " == " + playerInfo.Id);
            if (test.Id == playerInfo.Id)
            {
                isInThere = true;
            }
        }
        if (!isInThere)
        {
            GameManager.Players.Add(playerInfo);
            GD.Print("[" + Multiplayer.GetUniqueId() + "] " + "Adding unique player:\t" + playerInfo.Id + "\tname:\t" + playerInfo.Name);
            GD.Print("[" + Multiplayer.GetUniqueId() + "] " + "GetConnectedPlayerCount(): " + GetConnectedPlayerCount());
            GD.Print("[" + Multiplayer.GetUniqueId() + "] " + "GameManager.Players.Count: " + GameManager.Players.Count);
        }
        else
        {
            GD.Print("[" + Multiplayer.GetUniqueId() + "] " + "Player already exists, next person");
        }
        //GD.Print("["+Multiplayer.GetUniqueId()+"] "+"Adding unique player:\t" + playerInfo.Id + "\tname:\t" + playerInfo.Name);
        //GD.Print("["+Multiplayer.GetUniqueId()+"] "+"GetConnectedPlayerCount(): "+GetConnectedPlayerCount());
        //GD.Print("["+Multiplayer.GetUniqueId()+"] "+"GameManager.Players.Count: "+GameManager.Players.Count);

        // better check if we have the player
        // for (int index = 0; index < GameManager.Players.Count; index++)
        // {
        //     if (GameManager.Players[index].Id == playerInfo.Id)
        //     {
        //         GD.Print("[Debug] GameManager.Players[index].Id == playerInfo.Id");
        //     }
        //     else
        //     {
        //         GameManager.Players.Add(playerInfo);
        //         GD.Print("Adding unique player:\t" + playerInfo.Id + "\tname:\t" + playerInfo.Name);
        //     }
        // }
        // GD.Print("GetConnectedPlayerCount(): "+GetConnectedPlayerCount());


        // if (!GameManager.Players.Contains(playerInfo))
        // {
        //     GameManager.Players.Add(playerInfo);
        //     GD.Print("Adding unique player:\t" + playerInfo.Id + "\tname:\t" + playerInfo.Name);
        // }

        if (Multiplayer.IsServer())
        {
            GD.Print("[HOST]: Hey I'm gonna send SendPlayerInformation");
            foreach (var item in GameManager.Players)
            {
                Rpc("SendPlayerInformation", item.Name, item.Id);
            }
        }
    }

    public int GetConnectedPlayerCount()
    {
        var peers = Multiplayer.GetPeers(); // Returns an array of peer IDs
        return peers.Count();
    }

    
}
