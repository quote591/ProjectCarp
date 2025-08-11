using Godot;
using System;

namespace Interaction
{
    public interface InteractableInterface
    {
        void OnInteraction(Player player);
        abstract string interactionText { get; }
    }
}