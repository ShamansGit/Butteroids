using Godot;
using System;
using System.Diagnostics;

public partial class AsteroidSpawner : Node
{
    public static AsteroidSpawner instance;
    [Export]
    float spawnFrequency = 5f; // Number of spawns per second
    [Export(PropertyHint.Dir)]
    string[] asteroids;
    [Export]
    float[] asteroidsChance;
    [Export] int maxAsteroids = 30;
    [Export] bool enabled = true;

    private float timePasssedSinceSpawn = 0f;
    private float totalChance = 0f;
    private Vector2 mapSize;
    private float mapPerimeter = 0f;
    private RandomNumberGenerator rng = new();
    bool isHost;
    public override void _Ready()
    {
        instance = this;
        isHost = Multiplayer.GetUniqueId() == 1;
        if (!isHost) return;
        mapSize = GetNode<Globals>("/root/Globals").mapSize;

        Debug.Assert(asteroids.Length == asteroidsChance.Length, "Please configure each asteroid type with a chance");
        foreach (float asteroidChance in asteroidsChance)
        {
            totalChance += asteroidChance;
        }

        mapPerimeter = new Vector2(2, 2).Dot(mapSize);
    }

    public override void _Process(double delta)
    {
        if (!isHost) return;
        //puts a cap on how many asteroids can spawn
        if (Asteroid.asteroidCount > maxAsteroids || !enabled) return;

        timePasssedSinceSpawn += (float)delta;
        if ((int)(timePasssedSinceSpawn / spawnFrequency) > 0)
        {
            timePasssedSinceSpawn = 0;
            SelectAsteroid();
        }
    }
    public void SelectAsteroid()
    {
        // Spawn something
        float selected = rng.RandfRange(0, totalChance);
        // Select a random asteroid
        int index = -1;
        while (selected > 0)
        {
            index++;
            selected -= asteroidsChance[index];
        }
        float random = rng.RandfRange(0, mapPerimeter);
        // Randomly place somewhere on the perimeter
        Vector2 borderPosition = new Vector2(random < 2 * mapSize.X ? random % mapSize.X : ((int)(random - mapSize.X * 2) / (int)mapSize.Y) * mapSize.X,
                                        random < 2 * mapSize.X ? ((int)random / (int)mapSize.X) * mapSize.Y : (random - 2 * mapSize.X) % mapSize.Y);

        Rpc(nameof(SpawnAsteroid),asteroids[index],borderPosition,RandomDirection());
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnAsteroid(string scenePath,Vector2 position, Vector2 direction){
        PackedScene scene = GD.Load<PackedScene>(scenePath);
        Asteroid asteroid = scene.Instantiate<Asteroid>();
        instance.AddChild(asteroid);
        asteroid.Position = position;
        asteroid.SetDirection(direction);
    }
    public static Vector2 RandomDirection() => new Vector2((float)GD.RandRange(-1.0,1.0),(float)GD.RandRange(-1.0,1.0)).Normalized();

}
