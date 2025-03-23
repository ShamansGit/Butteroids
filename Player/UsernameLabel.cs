using Godot;
using System;
using System.Text;

public partial class UsernameLabel : Node2D
{
    PlayerInfo playerInfo;
    public override void _Ready()
    {
        base._Ready();
        UpdateInfo();
    }
    public override void _Process(double delta)
    {
        GlobalRotation = 0;
    }
    public void UpdateInfo(){
        PlayerInfo info = GameManager.Players[GetParent().Name.ToString().ToInt()];
        GetChild<Label>(0).Text = 
            info.Name + "\n" + 
            new StringBuilder().Insert(0, "🤍", Mathf.Max(0,info.lives)).ToString();
    }

}
