using Godot;
using System;

public partial class turret_fire : Node2D
{
	[Export] public PackedScene bullet_scene; 
 
	public override void _Process(double delta)
	{
		// If the player presses the "shoot" action
		if (Input.IsActionJustPressed("shoot"))
		{
			Node2D parentNode = (Node2D)GetParent();	
			
			if (bullet_scene == null)
		{
			GD.PrintErr("BulletScene is not assigned!");
			return;  // Exit if BulletScene is not assigned
		}
		
			// Instantiate the bullet
			Bullet bullet = (Bullet)bullet_scene.Instantiate();
			GetTree().Root.AddChild(bullet);  // Add the bullet to the scene

			// Calculate the direction the bullet should shoot based on the turret's rotation
			Vector2 shotDirection = new Vector2(0, -1).Rotated(parentNode.Rotation);

			// Set the bullet's position and direction
			bullet.Position = GlobalPosition;
			bullet.Direction = shotDirection;
		}
	}
}
