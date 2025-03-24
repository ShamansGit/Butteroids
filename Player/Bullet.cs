using Godot;
using System;

public partial class Bullet : Area2D
{
	public Vector2 Direction { get; set; } // Direction for bullet movement

	[Export] public float BulletSpeed = 700f; // Bullet speed

	public override void _Process(double delta)
	{
		// Move the bullet in the direction with the given speed
		Position += Direction * BulletSpeed * (float)delta;
	}

	// Destroy the bullet
	public override void _Ready()
	{
		// Get the VisibleOnScreenNotifier2D and connect its signal
		VisibleOnScreenNotifier2D notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		notifier.ScreenExited += OnScreenExited;
	}

	private void OnScreenExited()
	{
		QueueFree(); // Delete the bullet when it exits the screen
	}
		
}
