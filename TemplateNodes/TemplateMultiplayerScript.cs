/// <summary>
/// 
/// All the player spawning and building has already been completed
/// spawn points at in the range of -5 0 -5 to 5 0 5 
/// Up to 8 player spawns
/// Feel free to change the spawn points to your desired location
/// 
/// How to use the Template Multiplayer Script And Scene
/// 
/// 1) Copy the scene into res://Scenes 
/// 2) Change the name
/// 
/// Basically this script should handle all the scene events
/// currenlty only 3D Audio (proxy chat) is possible
/// 
/// </summary>

using Godot;
using System;
using System.Linq;

public partial class TemplateMultiplayerScript : Node3D
{
    // of course you can place you global and export varibles here x



    // ===== Multiplayer spawning and instantiaton start =====
    [Export] PackedScene playerScene;

    public override void _Ready()
    {
        // get the drone manager from the tree
        var droneManager = GetNode<VoiceDroneController>("VoiceDroneController");

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
            // attach a drone to the player
            GD.Print("[" + Multiplayer.GetUniqueId() + "] RegisterPlayer currentPlayerId: "+currentPlayer.PlayerId);
            droneManager.RegisterPlayer(currentPlayer);
        }

        // ===== Multiplayer spawning and instantiaton end =====

        // here you can place all the code you want to run when the scene is first made x
    }

    public override void _PhysicsProcess(double delta)
    {
        // Implement code to run at every game tick below x
    }

    // Of course you can have your own functions here as well x
}
