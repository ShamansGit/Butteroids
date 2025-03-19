using Godot;
using System;

public partial class AsteroidChunk : RigidBody2D
{
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
}
