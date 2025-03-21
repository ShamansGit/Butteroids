using Godot;
using System;

public partial class AsteroidChunk : Asteroid
{
    // Override to prevent the chunk breaking into more chunks
    public override void OnBodyEntered(Node body)
    {
        QueueFree();
    }
}
