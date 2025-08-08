using Godot;
using System;

public partial class Drone : Node3D
{
    private Node3D targetPlayer;
    private Vector3 droneOffset = new Vector3(0,1.5f,0);

    public void SetTarget(Node3D player)
    {
        targetPlayer = player;
    }

    public override void _Process(double delta)
    {
        if (targetPlayer != null)
        {
            GlobalPosition = targetPlayer.GlobalPosition + droneOffset;
        }
    }
}
