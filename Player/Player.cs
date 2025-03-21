using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MaxSpeed { get; set; } = 300.0f;
	[Export] public float Acceleration { get; set; } = 600.0f;
	[Export] public float RotationSpeed { get; set; } = 3.5f;
	[Export] public float DampingFactor { get; set; } = 0.95f; // Slowdown when no input

	private Vector2 velocity = Vector2.Zero;

	//networking
	[Export] MultiplayerSynchronizer synchronizer;
	bool hasControl = false;

	public override void _Ready()
	{
		base._Ready();
		bodyent
	}
	public void SetMultiplayer(int id, PlayerInfo info)
	{
		synchronizer.SetMultiplayerAuthority(id);
		hasControl = synchronizer.IsMultiplayerAuthority();
		Name = id.ToString();
		Modulate = info.color;

		if (hasControl)
		{
			Camera2D camera = GetViewport().GetCamera2D();
			camera.GetParent().RemoveChild(camera);
			AddChild(camera);
			camera.Position = Vector2.Zero;
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		//stops you controlling other players
		if (!hasControl) return;

		float rotationInput = Input.GetActionStrength("rotate_right") - Input.GetActionStrength("rotate_left");
		float thrustInput = Input.GetActionStrength("accelerate");

		// Debugging Output
		//GD.Print($"Rotation Input: {rotationInput}, Thrust Input: {thrustInput}");

		// Rotate the player
		Rotation += rotationInput * RotationSpeed * (float)delta;

		// Apply thrust
		if (thrustInput > 0)
		{
			Vector2 thrust = new Vector2(0, -thrustInput * Acceleration * (float)delta).Rotated(Rotation);
			velocity += thrust;
			//GD.Print($"Thrust Applied! New Velocity: {velocity}");
		}
		else
		{
			// Apply damping (gradual slowdown)
			velocity *= DampingFactor;
		}

		// Limit speed
		velocity = velocity.LimitLength(MaxSpeed);

		// Move the player
		Velocity = velocity;
		//GD.Print($"Final Velocity: {Velocity}");

		MoveAndSlide();
	}
}
