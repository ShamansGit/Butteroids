using Godot;
using System;
[Obsolete]
//this is now obsolete, as OnBodyEntered is important for checking player/projectile collisions
//so it was easier to just check if the asteroids PackedScene == null and not spawning it that way.
public partial class AsteroidChunk : Asteroid
{
    // Override to prevent the chunk breaking into more chunks
    // public override void OnBodyEntered(Node body)
    // {
    //     QueueFree();
    // }
}
