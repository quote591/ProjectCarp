using Godot;
using System;

public partial class Server : Node
{

    private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

    [Export]
    public PackedScene PlayerScene { get; set; }
    private bool serverIsReady;

    [Export]
    public NodePath SpawnLocation;



    public override void _Process(double delta)
    {

        if (serverIsReady) peer.Poll();
    }




    private void _on_host_button_down()
    {
        // create server
        var error = peer.CreateServer(8910);
        // check if any errors
        if (error != Error.Ok)
        {
            GD.Print("server has failed to start " + error);
        }
        // set peer as host "hey i am the server"
        Multiplayer.MultiplayerPeer = peer;
        // instances the server's node
        Node player = PlayerScene.Instantiate();
        GetNode<Node>(SpawnLocation).AddChild(player);
        player.Name = "1";
        player.GetNode<AudioManager>("AudioManager").SetupAudio(1);
        serverIsReady = true;
    }
}
