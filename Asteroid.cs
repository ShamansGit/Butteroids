using Godot;
using System;

public partial class Asteroid : RigidBody2D
{
    [Export]
    PackedScene[] pieces; // Array of RigidBody2D parts the asteroid can break into.

    // Called when the node enters the scene
    public override void _Ready()
    {
        // Initialise with random speed, direction, and spin


    }

    // Called every frame
    public override void _Process(double delta)
    {
        // QueueFree() should be called when the object moves off the screen
    }

    /// <summary>
    /// Called whenever a body enters this one. Scene layers are set up such that this object can only collide with projectiles and asteroid pieces.
    /// </summary>
    /// <param name="body">The other colliding body</param>
    public void OnBodyEntered(Node body)
    {
        // Add all the pieces to the scene tree
        Node root = GetParent();
        foreach (PackedScene pieceScene in pieces)
        {
            RigidBody2D piece = pieceScene.Instantiate<RigidBody2D>();
            root.AddChild(piece);
        }
    }
}
