using Godot;
using System;

public partial class Bullet : Area2D
{
	public Vector2 Direction { get; set; } // Direction for bullet movement

	[Export] public float BulletSpeed = 700f; // Bullet speed
	float lifeTime = 6;

	public override void _Process(double delta)
	{
		// Move the bullet in the direction with the given speed
		Position += Direction * BulletSpeed * (float)delta;
		lifeTime -= (float)delta;
		if (lifeTime < 0)
		{
			Destroy();
		}
	}

	// Destroy the bullet
	public override void _Ready()
	{
		// Get the VisibleOnScreenNotifier2D and connect its signal
		//VisibleOnScreenNotifier2D notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		//notifier.ScreenExited += OnScreenExited;
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			((Player)body).Hit();
		}
		Destroy();
	}

	public void Destroy()
	{
		QueueFree(); // Delete the bullet when it exits the screen
	}

}
