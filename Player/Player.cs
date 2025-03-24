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
	long playerId;
	bool hasControl = false;
	//start with 5 so that when players spawn in they have 5 seconds of spawn immunity
	float invulnTimer = 0;
	public bool isInvulnerable => invulnTimer > 0 && !isDead;
	bool isDead = false;

	public override void _Ready()
	{
		base._Ready();
		invulnTimer = invulnerableDuration;
	}
	public void SetMultiplayer(long id, PlayerInfo info)
	{
		synchronizer.SetMultiplayerAuthority((int)id);
		hasControl = synchronizer.IsMultiplayerAuthority();
		Name = id.ToString();
		playerId = id;
		Modulate = info.color;

		if (hasControl)
		{
			SetCameraParent(this);
		}
	}
	public void SetCameraParent(Node node)
	{
		Camera2D camera = GetViewport().GetCamera2D();
		camera.GetParent().RemoveChild(camera);
		node.AddChild(camera);
		camera.Position = Vector2.Zero;
	}

    public override void _Process(double delta)
    {
        if (isInvulnerable)
        {
            invulnTimer -= (float)delta;
            //makes the player flash when invulnerable
            Visible = invulnTimer % 0.2f > 0.1f;
        }
        else
        {
            Visible = !isDead;
        }

        //stops you controlling other players
        if (hasControl) Control(delta);
    }

    public void Control(double delta)
	{
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
		//if this is the server / host
		if (Multiplayer.GetUniqueId() == 1)
		{
			if (!isInvulnerable && !isDead) Rpc(nameof(Player.OnPlayerHit), playerId);
		}
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void OnPlayerHit(long hitId)
	{
		invulnTimer = invulnerableDuration;
		GameManager.Players[hitId].lives -= 1;
		if (hitId == playerId)
		{
			GetNode<UsernameLabel>("Username").UpdateInfo();
			if (GameManager.Players[hitId].lives < 0)
			{
				MatchManager.EliminatePlayer(hitId);
				if (hitId == Multiplayer.GetUniqueId())
				{
					GD.Print("You Died!");
					hasControl = false;
					SetCameraParent(GetParent());
					//zoom to see the whole map
					GetViewport().GetCamera2D().Zoom = Vector2.One * GetViewportRect().Size.Y / GameManager.instance.mapSize.Y;
				}
				invulnTimer = 0;
				isDead = true;
			}
		}
	}

}
