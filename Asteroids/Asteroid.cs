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
    [Export] CollisionShape2D collisionShape;
    [Export] Sprite2D sprite;
    [Export] Godot.Collections.Array<Texture2D> asteroidTextures = new Godot.Collections.Array<Texture2D>();
    [Export] MultiplayerSynchronizer synchronizer;
    public static int asteroidCount = 0;
    bool hasAuthority;

    // Called when the node enters the scene
    public override void _Ready()
    {
        // Initialise with random speed, direction, and spin

        //Vector2 direction = RandomDirection();
        //velocity = direction * speed;

        // Set up edge for wraparound
        bounds = GetNode<Globals>("/root/Globals").mapSize;
        margin = bounds * 0.2f;


        sprite.Texture = asteroidTextures.PickRandom();
        BodyEntered += CollideEnter;
        AreaEntered += CollideEnter;

        asteroidCount += 1;

        //sets the synchronisers authority to 1
        //synchronizer = (MultiplayerSynchronizer)FindChild("MultiplayerSynchronizer");
        //synchronizer.SetMultiplayerAuthority(1);
        hasAuthority = Multiplayer.GetUniqueId() == 1;

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
    public void SetDirection(Vector2 direction){
        velocity = direction.Normalized() * speed;
    }
    // Called every frame

    public bool hasSpawnImmunity = true;
    float graceTime = 0;
    public override void _PhysicsProcess(double delta)
    {
        //if (!hasAuthority) return;
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
    /// <param name="node">The other colliding body</param>
    /// 
    public void CollideEnter(Node node){
        //if (!hasAuthority) return;
        if (hasAuthority && node is Player){
            GD.Print("PLAYER HIT");
            ((Player)node).Hit(); 
        }
        //GD.Print("HELLO");
        if (!hasSpawnImmunity || !(node is Asteroid)){
            CallDeferred(nameof(Split));
            asteroidCount -= 1;
            QueueFree();
        }
    }
    public void Split(){
        if (!hasAuthority) return;
        //GD.Print("TRUE" + pieceScene);
        // Add all the pieces to the scene tree
        if (pieceScene != null){
            Node root = GetParent();
            for (int i = 0; i < chunkCount; i++)
            {
                AsteroidSpawner.instance.Rpc(nameof(AsteroidSpawner.instance.SpawnAsteroid),pieceScene,GlobalPosition,AsteroidSpawner.RandomDirection());
                //Asteroid piece = pieceScene.Instantiate<Asteroid>();
                //piece.rng = rng;
                //piece.hasSpawnImmunity = true;
                //piece.GlobalPosition = GlobalPosition;
                //root.AddChild(piece);
                //CallDeferred(nameof(AddChildDeferred), root, piece);
            }
        }
    }

    // Got an error when adding a child from OnBodyEntered, calling it deferred here fixes it.
    // private void AddChildDeferred(Node parent, Node child)
    // {
    //     parent.AddChild(child);
    // }

}
