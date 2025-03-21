using Godot;
using System;

public partial class MatchManager : Node2D
{
    [Export] PackedScene playerScene;
    public override void _Ready()
    {
        base._Ready();
        int position = 0;
        foreach (var player in GameManager.Players)
        {
            int playerid = player.Key;
            Player playerInstance = playerScene.Instantiate<Player>();
            playerInstance.Name = playerid.ToString();
            playerInstance.Position = new Vector2(1,position);

            AddChild(playerInstance);
            playerInstance.SetMultiplayer(playerid,player.Value);

            position += 100;
        }
    }
}
