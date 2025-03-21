using Godot;
using System;
using System.Diagnostics;

public partial class Asteroid : Node2D
{
    [Export]
    PackedScene[] pieces; // Array of RigidBody2D parts the asteroid can break into.
    [Export] float speed = 10;
    Vector2 bounds;
    Vector2 margin;
    Vector2 velocity;

    // Called when the node enters the scene
    public override void _Ready()
    {
        // Initialise with random speed, direction, and spin

        Vector2 direction = new Vector2((float)GD.RandRange(-1.0,1.0),(float)GD.RandRange(-1.0,1.0)).Normalized();
        velocity = direction * speed;

        // Set up edge for wraparound
        bounds = GetNode<Globals>("/root/Globals").mapSize;
        margin = bounds * 0.1f;

        // Random sprite and corresponding hitbox
        int nOptions = GetNode("Sprites").GetChildCount();
        int number = GD.RandRange(0, nOptions - 1);
        Debug.Assert(nOptions == GetNode("Area2D/CollisionShapes").GetChildCount(), "Mismatched number of sprites/hitboxes");
        Debug.Assert(nOptions > 0, "Asteroid must have at least 1 sprite");
        Node hitbox = GetNode("Area2D/CollisionShapes").GetChild(number);
        Node sprite = GetNode("Sprites").GetChild(number);
        for (int i = 0; i < nOptions; i++)
        {
            if (i != number)
            {
                // Keep iterating because they are not actually removed yet, just queued
                GetNode("Sprites").GetChild(i).QueueFree();
                GetNode("Area2D/CollisionShapes").GetChild(i).QueueFree();
            }
        }
        GetNode("Area2D/CollisionShapes").RemoveChild(hitbox);
        GetNode("Area2D/CollisionShapes").QueueFree();
        GetNode("Area2D").AddChild(hitbox);
    }

    // Called every frame
    public override void _PhysicsProcess(double delta)
    {
        // Hopefully more efficient than 'if' statements to check if out of bounds

        Position += velocity * (float)delta;

        // Effectively, p is wrapped around iff p >= bounds + margin OR p < -margin
        Position = (Position + margin).PosMod(bounds + margin * 2) - margin;
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
            Asteroid piece = pieceScene.Instantiate<Asteroid>();
            //piece.rng = rng;
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
