using Godot;
using System;
using System.Diagnostics;

public partial class AsteroidSpawner : Node
{
    [Export]
    float spawnFrequency = 5f; // Number of spawns per second
    [Export]
    PackedScene[] asteroids;
    [Export]
    float[] asteroidsChance;

    private float timePasssedSinceSpawn = 0f;
    private float totalChance = 0f;
    private Vector2 mapSize;
    private float mapPerimeter = 0f;
    private RandomNumberGenerator rng = new();

    public override void _Process(double delta)
    {
        timePasssedSinceSpawn += (float)delta;
        if ((int)(timePasssedSinceSpawn / spawnFrequency) > 0)
        {
            timePasssedSinceSpawn = 0;
            // Spawn something
            float random = rng.RandfRange(0, totalChance);
            // Select a random asteroid
            int index = -1;
            while (random > 0)
            {
                index++;
                random -= asteroidsChance[index];
            }
            Asteroid asteroid = asteroids[index].Instantiate<Asteroid>();
            // Share rng
            //asteroid.rng = rng;
            AddChild(asteroid);
            // Randomly place somewhere on the perimeter
            random = rng.RandfRange(0, mapPerimeter);
            asteroid.Position = new Vector2(random < 2 * mapSize.X ? random % mapSize.X : ((int)(random - mapSize.X * 2) / (int)mapSize.Y) * mapSize.X, 
                                            random < 2 * mapSize.X ? ((int)random / (int)mapSize.X) * mapSize.Y : (random - 2 * mapSize.X) % mapSize.Y);
        }
    }

    public override void _Ready()
    {
        mapSize = GetNode<Globals>("/root/Globals").mapSize;

        Debug.Assert(asteroids.Length == asteroidsChance.Length, "Please configure each asteroid type with a chance");
        foreach (float asteroidChance in asteroidsChance)
        {
            totalChance += asteroidChance;
        }

        mapPerimeter = new Vector2(2,2).Dot(mapSize);
    }
}
