using Godot;
using Interaction;
using System;

// Interactable object script, if we interact with this object i.e. pressing interact on it
// it will call this method of the object with a current state of the player

public partial class InteractableObject3D : StaticBody3D, InteractableInterface
{
    public void OnInteraction(Player playerState)
    {
        GD.Print("There has been an interaction.");
    }

    public string interactionText { get { return "Press E to interact."; } }
}
