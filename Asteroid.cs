using Godot;
using System;

public partial class Asteroid : Node2D
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
        
    }

    /// <summary>
    /// Called whenever a body enters this one. Scene layers are set up such that this object can only collide with projectiles and asteroid pieces.
    /// </summary>
    /// <param name="body">The other colliding body</param>
    public virtual void OnBodyEntered(Node body)
    {
        // Add all the pieces to the scene tree
        Node root = GetParent();
        foreach (PackedScene pieceScene in pieces)
        {
            Node2D piece = pieceScene.Instantiate<Node2D>();
            CallDeferred("AddChildDeferred", root, piece);
            piece.Position = Position;
        }
        QueueFree();
    }

    // Got an error when adding a child from OnBodyEntered, calling it deferred here fixes it.
    private void AddChildDeferred(Node parent, Node child)
    {
        parent.AddChild(child);
    }

}
