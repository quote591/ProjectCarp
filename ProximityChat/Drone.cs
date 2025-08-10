/// <summary>
/// 
/// Welcome to Drone.cs
/// 
/// all it does is gets a target from the voice drone controller when its made
/// then it will just follow the player always
/// 
/// the voiceDroneController will send data to the correct drone to send information 
/// 
/// </summary>

using Godot;
using System;

public partial class Drone : Node3D
{
    private Node3D targetPlayer;

    // this sets the offset for where the drone is compared to the bottom of the player
    // so increased the y to compensate
    private Vector3 droneOffset = new Vector3(0, 1.5f, 0);

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
