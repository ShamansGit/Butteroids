using Godot;
using System;
using System.Diagnostics;

public partial class Asteroid : Area2D
{
    [Export(PropertyHint.Dir)] string pieceScene; // Array of RigidBody2D parts the asteroid can break into.
    [Export] int chunkCount = 3;
    [Export] float speed = 10;
    Vector2 bounds;
    Vector2 margin;
    Vector2 velocity;
    float rotationSpeed;
    [Export] CollisionShape2D collisionShape;
    [Export] Sprite2D sprite;
    [Export] Godot.Collections.Array<Texture2D> asteroidTextures = new Godot.Collections.Array<Texture2D>();
    [Export] MultiplayerSynchronizer synchronizer;
    public static int asteroidCount = 0;
    bool hasAuthority;
    public bool hasSpawnImmunity = true;
    float graceTime = 0;

    // Called when the node enters the scene
    public override void _Ready()
    {
        // Set up edge for wraparound
        bounds = GetNode<Globals>("/root/Globals").mapSize;
        margin = bounds * 0.2f;


        sprite.Texture = asteroidTextures.PickRandom();
        BodyEntered += CollideEnter;
        AreaEntered += CollideEnter;

        asteroidCount += 1;

        //sets the synchronisers authority to 1
        hasAuthority = Multiplayer.GetUniqueId() == 1;

        hasSpawnImmunity = true;
    }
    public void SetDirection(Vector2 direction)
    {
        velocity = direction.Normalized() * speed;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += velocity * (float)delta;
        sprite.Rotate(rotationSpeed * (float)delta);

        // Hopefully more efficient than 'if' statements to check if out of bounds
        // Effectively, p is wrapped around iff p >= bounds + margin OR p < -margin
        Position = (Position + margin).PosMod(bounds + margin * 2) - margin;

        //spawn immunity makes sure that the asteroid doesnt collide with other asteroids when it spawns, so therefore if there
        //are no longer any collisions we can remove the immunity to allow for collisions with asteroids again.
        graceTime += (float) delta;
        if (graceTime > 2f && hasSpawnImmunity && !HasOverlappingAreas()){
            hasSpawnImmunity = false;
        }
    }

    /// <summary>
    /// Called whenever a body enters this one. Scene layers are set up such that this object can only collide with projectiles and asteroid pieces.
    /// </summary>
    /// <param name="node">The other colliding body</param>
    /// 
    public void CollideEnter(Node node)
    {
        if (hasAuthority && node is Player){
            GD.Print("PLAYER HIT");
            ((Player)node).Hit(); 
        }
        if (!hasSpawnImmunity || !(node is Asteroid)){
            if (node is Asteroid)
            {
                CallDeferred(nameof(Split), node as Asteroid);
            } else
            {
                CallDeferred(nameof(Split), null);
            }
            asteroidCount -= 1;
            QueueFree();
        }
    }
    public void Split(Asteroid other)
    {
        if (!hasAuthority) return;
        // Add all the pieces to the scene tree
        if (pieceScene != null){
            Node root = GetParent();
            for (int i = 0; i < chunkCount; i++)
            {
                float randOffsetSize = (collisionShape.Shape as CircleShape2D).Radius;
                Vector2 randomOffset = new ((float)GD.RandRange(-randOffsetSize, randOffsetSize), (float)GD.RandRange(-randOffsetSize, randOffsetSize));
                if (other is null)
                {
                    AsteroidSpawner.instance.Rpc(nameof(AsteroidSpawner.instance.SpawnAsteroid), pieceScene, GlobalPosition + randomOffset, velocity, rotationSpeed);
                } 
                else
                {
                    float centreDist = randomOffset.Length();
                    Vector2 linearVel = velocity + new Vector2(rotationSpeed, rotationSpeed) * centreDist;
                    AsteroidSpawner.instance.Rpc(nameof(AsteroidSpawner.instance.SpawnAsteroid), pieceScene, GlobalPosition, linearVel + other.velocity, rotationSpeed + other.rotationSpeed);
                }
            }
        }
    }
}
