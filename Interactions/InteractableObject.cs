using Godot;
using System;

/*
Setup for the usage of this script

Root Object
|- Its static bounding box
|  |- Its collision box (optional but recommended)
|- Area3D box to know if we are near it (this script attaches here)

*/

public partial class InteractableObject : Area3D
{
    InteractionManager g_im = null;
    private uint defaultColl = 1;

    public override void _Ready()
    {
        this.BodyEntered += OnBodyEntered;
        this.BodyExited += OnBodyExited;

        g_im = (InteractionManager)GetNode("/root/InteractionManager");
        GD.Print("g_im: ", g_im);
    }

    // We are close enough to our object, register us to the interaction manager
    private void OnBodyEntered(Node body)
    {
        if (g_im == null)
            return;

        StaticBody3D staticNode = GetParentNode3D().GetChild<StaticBody3D>(0);
        staticNode.CollisionLayer |= 1U << InteractionManager.collisionMask;

        GD.Print("INSIDE");
        g_im.RegisterObject(staticNode);
    }

    // We have left the area of our object, unregister us from the interaction manager
    private void OnBodyExited(Node body)
    {
        if (g_im == null)
            return;

        // So go up to get the "object" we want to interact with, then we get its static box
        StaticBody3D staticNode = GetParentNode3D().GetChild<StaticBody3D>(0);
        staticNode.CollisionLayer = defaultColl;
        GD.Print("Exited");
        g_im.UnregisterObject(staticNode);
    }
}
