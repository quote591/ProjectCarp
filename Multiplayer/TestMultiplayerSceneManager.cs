using Godot;
using System;
using System.Linq;

public partial class TestMultiplayerSceneManager : Node3D
{
    [Export] PackedScene playerScene;

    public override void _Ready()
    {
        // find drone manager
        var droneManager = GetNode<VoiceDroneController>("VoiceDroneController");

        // get all the spawn points and sort them
        var spawnPoints = GetTree().GetNodesInGroup("PlayerSpawnPoints").OfType<Node3D>().OrderBy(n =>
        {
            if (int.TryParse(n.Name, out var v))
                return v;
            return int.MaxValue;
        }).ToArray();

        int index = 0;
        foreach (var item in GameManager.Players)
        {
            Player currentPlayer = playerScene.Instantiate<Player>();

            currentPlayer.Name = item.Id.ToString();
            currentPlayer.PlayerId = item.Id;
            currentPlayer.SetMultiplayerAuthority((int)item.Id);
            AddChild(currentPlayer);

            if (index < spawnPoints.Length)
            {
                currentPlayer.GlobalPosition = spawnPoints[index].GlobalPosition;
            }
            else
            {
                GD.PrintErr($"No spawn point for player index {index}");
            }
            index++;

            // attach drone to player (target not child)
            GD.Print("[" + Multiplayer.GetUniqueId() + "] RegisterPlayer currentPlayerId: "+currentPlayer.PlayerId);
            droneManager.RegisterPlayer(currentPlayer);
        }

    }
}
