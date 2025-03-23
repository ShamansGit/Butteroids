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
	[Export] float invulnerableDuration = 4;
	bool hasControl = false;
	//start with 5 so that when players spawn in they have 5 seconds of spawn immunity
	float invulnTimer = 0;
	public bool isInvulnerable => invulnTimer > 0;

	public override void _Ready()
	{
		base._Ready();
		invulnTimer = invulnerableDuration;
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
		if (isInvulnerable){
			invulnTimer -= (float)delta;
			//makes the player flash when invulnerable
			Visible = invulnTimer % 0.2f > 0.1f;  
		}else{
			Visible = true;
		}

		//stops you controlling other players
		if (hasControl) Control(delta);
	}
	public void Control(double delta){
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

    public void Hit()
    {
		if (!isInvulnerable){
			invulnTimer = invulnerableDuration;
		}
		// if (hasControl){
		// 	//hasControl = false;
		// 	//Hide();
		// }
    }

}
