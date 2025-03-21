using Godot;
using System;

public partial class UsernameLabel : Node2D
{

    public override void _Ready()
    {
        base._Ready();
        GetChild<Label>(0).Text = GetParent().Name;
    }
    public override void _Process(double delta)
    {
        GlobalRotation = 0;
    }

}
