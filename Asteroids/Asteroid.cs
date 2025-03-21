using Godot;
using System;
using System.Diagnostics;

public partial class Asteroid : Area2D
{
    [Export] PackedScene pieceScene; // Array of RigidBody2D parts the asteroid can break into.
    [Export] int chunkCount = 3;
    [Export] float speed = 10;
    Vector2 bounds;
    Vector2 margin;
    Vector2 velocity;
    [Export] CollisionShape2D collisionShape;
    [Export] Sprite2D sprite;
    [Export] Godot.Collections.Array<Texture2D> asteroidTextures = new Godot.Collections.Array<Texture2D>();
    public static int asteroidCount = 0;

    // Called when the node enters the scene
    public override void _Ready()
    {
        // Initialise with random speed, direction, and spin

        Vector2 direction = RandomDirection();
        velocity = direction * speed;

        // Set up edge for wraparound
        bounds = GetNode<Globals>("/root/Globals").mapSize;
        margin = bounds * 0.1f;


        sprite.Texture = asteroidTextures.PickRandom();
        BodyEntered += Collide;
        AreaEntered += Collide;

        asteroidCount += 1;

        //Simplified the collision shapes and reduced number of sprites for performance

        // // Random sprite and corresponding hitbox
        // int nOptions = GetNode("Sprites").GetChildCount();
        // int number = GD.RandRange(0, nOptions - 1);
        // Debug.Assert(nOptions == GetNode("Area2D/CollisionShapes").GetChildCount(), "Mismatched number of sprites/hitboxes");
        // Debug.Assert(nOptions > 0, "Asteroid must have at least 1 sprite");
        // Node hitbox = GetNode("Area2D/CollisionShapes").GetChild(number);
        // Node sprite = GetNode("Sprites").GetChild(number);
        // for (int i = 0; i < nOptions; i++)
        // {
        //     if (i != number)
        //     {
        //         // Keep iterating because they are not actually removed yet, just queued
        //         GetNode("Sprites").GetChild(i).QueueFree();
        //         GetNode("Area2D/CollisionShapes").GetChild(i).QueueFree();
        //     }
        // }
        // GetNode("Area2D/CollisionShapes").RemoveChild(hitbox);
        // GetNode("Area2D/CollisionShapes").QueueFree();
        // GetNode("Area2D").AddChild(hitbox);
        hasSpawnImmunity = true;
    }

    private Vector2 RandomDirection() => new Vector2((float)GD.RandRange(-1.0,1.0),(float)GD.RandRange(-1.0,1.0)).Normalized();
    // Called every frame

    public bool hasSpawnImmunity = true;
    float graceTime = 0;
    public override void _PhysicsProcess(double delta)
    {
        // Hopefully more efficient than 'if' statements to check if out of bounds

        Position += velocity * (float)delta;

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
    /// <param name="body">The other colliding body</param>
    /// 
    public void Collide(Node body){
        if (body is Player){
            GD.Print("PLAYER HIT");
            ((Player)body).Hit(); 
        }
        if (!hasSpawnImmunity || body is not Asteroid){
            CallDeferred(nameof(Split));
        }
    }
    public void Split(){
        // Add all the pieces to the scene tree
        if (pieceScene != null){
            Node root = GetTree().CurrentScene;
            for (int i = 0; i < chunkCount; i++)
            {
                Asteroid piece = pieceScene.Instantiate<Asteroid>();
                //piece.rng = rng;
                piece.hasSpawnImmunity = true;
                root.AddChild(piece);
                piece.Position = Position;
                //CallDeferred(nameof(AddChildDeferred), root, piece);
            }
        }
        asteroidCount -= 1;
        QueueFree();
    }

    // Got an error when adding a child from OnBodyEntered, calling it deferred here fixes it.
    // private void AddChildDeferred(Node parent, Node child)
    // {
    //     parent.AddChild(child);
    // }

}
