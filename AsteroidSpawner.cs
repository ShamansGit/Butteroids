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
    private float screenPerimeter = 0f;
    private Vector2 screenSize;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Process(double delta)
    {
        timePasssedSinceSpawn += (float)delta;
        if (timePasssedSinceSpawn / spawnFrequency > 0f)
        {
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
            AddChild(asteroid);
            // Randomly place somewhere on the perimeter
            random = rng.RandfRange(0, screenPerimeter);
            asteroid.Position = new Vector2(random < 2 * screenSize.X ? random % screenSize.X : ((int)(random - screenSize.X * 2) / (int)screenSize.Y) * screenSize.X, 
                                            random < 2 * screenSize.X ? ((int)random / (int)screenSize.X) * screenSize.Y : (random - 2 * screenSize.X) % screenSize.Y);
        }
    }

    public override void _Ready()
    {
        Debug.Assert(asteroids.Length == asteroidsChance.Length, "Please configure each asteroid type with a chance");
        foreach (float asteroidChance in asteroidsChance)
        {
            totalChance += asteroidChance;
        }

        // Will need a refactor for multiplayer, this returns the screen size in pixels.
        // We want this to get some kind of relative screen size so that players on different monitors don't have a different experience
        screenSize = DisplayServer.ScreenGetSize();
        screenPerimeter = new Vector2(2,2).Dot(screenSize);
    }
}
