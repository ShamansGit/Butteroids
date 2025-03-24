using Godot;
using System;

public partial class TurretFire : Node2D
{
	[Export] public PackedScene bullet_scene;
	[Export] public float fireDelay = 4;
	float fireTime = 0;
	public bool canFire => fireTime <= 0;
	public long id;
	public bool hasControl = false;
	public override void _Ready()
	{
		base._Ready();
	}
	public override void _Process(double delta)
	{
		fireTime -= (float)delta;
		Visible = canFire;
	}
	public void Fire()
	{
		
		Rpc(nameof(FireBullet),GlobalPosition, GlobalRotation,id);
		//FireBullet(GlobalPosition, GlobalRotation);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void FireBullet(Vector2 position, float rotation,long shooter)
	{
		if (shooter == id){
			fireTime = fireDelay;
		}
		if (bullet_scene == null)
		{
			GD.PrintErr("BulletScene is not assigned!");
			return;  // Exit if BulletScene is not assigned
		}

		// Instantiate the bullet
		Bullet bullet = (Bullet)bullet_scene.Instantiate();
		GetTree().Root.AddChild(bullet);  // Add the bullet to the scene

		// Calculate the direction the bullet should shoot based on the turret's rotation
		Vector2 shotDirection = new Vector2(0, -1).Rotated(rotation);

		// Set the bullet's position and direction
		bullet.Position = position;
		bullet.Direction = shotDirection;
	}
}
