using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

#define DEBUG

public partial class InteractionManager : Node
{
    // The area around the interactable item where we start checking if the user is looking
    // at the object or is attempting to interact with it
    LinkedList<Node3D> interactActiveArea;

    // Bit shift for our values
    public static readonly int collisionMask = 10;

    private static Player localPlayer;

    // Will only be set true once we have registered a player
    private static bool PlayerIsSet = false;
    public override void _Ready()
    {
        interactActiveArea = new LinkedList<Node3D>();
    }

    public void RegisterPlayer(Player player)
    {
        localPlayer = player;
        PlayerIsSet = true;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (interactActiveArea.Count < 2)
            return;

        // Determine closest
        Node3D[] orderedIObjects = interactActiveArea.ToArray();
        Array.Sort(orderedIObjects, (a, b) =>
        {
            var aDist = (a.GlobalPosition - localPlayer.GlobalPosition).Length();
            var bDist = (b.GlobalPosition - localPlayer.GlobalPosition).Length();
            return aDist.CompareTo(bDist);
        });

        // From here we can do things like display "E to interact" text etc for x closest interactables
        foreach (Area3D obj in orderedIObjects)
        {

        }

    }

    public void RegisterObject(Node3D obj)
    {
        if (obj == null)
        {
            GD.PrintErr("Its null");

        }
        if (interactActiveArea.Contains(obj) || obj == null)
            return;

        interactActiveArea.AddLast(obj);
        GD.Print($"IntMan: Registered: {obj.Name} Pos: {obj.GlobalPosition}");
    }

    public void UnregisterObject(Node3D obj)
    {
        interactActiveArea.Remove(interactActiveArea.Find(obj));
        GD.Print($"IntMan: UnRegistered: {obj.Name}");
    }

}
