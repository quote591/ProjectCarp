using Godot;
using System;
using System.Linq;

public partial class TestMultiplayerSceneManager : Node3D
{
    [Export] PackedScene playerScene;

    public override void _Ready()
    {
        // get all the spawn points and sort them
        var spawnPoints = GetTree().GetNodesInGroup("PlayerSpawnPoints").OfType<Node3D>().OrderBy(n =>
        {
            if (int.TryParse(n.Name, out var v))
                return v;
            return int.MaxValue;
        }).ToArray();



        // careful not to go past how many we have
        // TODO add a check for max players
        int index = 0;
        foreach (var item in GameManager.Players)
        {
            Player currentPlayer = playerScene.Instantiate<Player>();
            // this gives you a demo name if you dont set one
            //var desiredName = string.IsNullOrWhiteSpace(item.Name) ? $"Player{index}" : item.Name;
            currentPlayer.Name = item.Id.ToString();
            currentPlayer.PlayerId = item.Id;
            AddChild(currentPlayer);
            currentPlayer.GlobalPosition = spawnPoints[index].GlobalPosition;
            index++;
        }
    }
}
