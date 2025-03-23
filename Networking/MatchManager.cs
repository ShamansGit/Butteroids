using Godot;
using System;
using System.Collections.Generic;

public partial class MatchManager : Node2D
{
    [Export] PackedScene playerScene;
    [Export] float playerRingSize = 200;
    static List<long> PlayersAlive = new List<long>();
    public override void _Ready()
    {
        base._Ready();
        int position = 0;
        int index = 0;
        foreach (var player in GameManager.Players)
        {
            long playerid = player.Key;
            Player playerInstance = playerScene.Instantiate<Player>();
            playerInstance.Name = playerid.ToString();

            float ringAngle = (float)index / Mathf.Max(1,GameManager.PlayerCount - 1) * Mathf.Pi;
            playerInstance.Position = new Vector2(Mathf.Sin(ringAngle),Mathf.Cos(ringAngle)) * playerRingSize;

            AddChild(playerInstance);
            playerInstance.SetMultiplayer(playerid,player.Value);

            position += 100;
            index += 1;
            PlayersAlive.Add(playerid);
        }
    }
    public static void EliminatePlayer(long id){
        if (PlayersAlive.Contains(id)){

            GD.Print(GameManager.Players[id].Name," was eliminated!");
            PlayersAlive.Remove(id);
            if (PlayersAlive.Count == 1 && GameManager.PlayerCount > 1){
                DeclareWinner(PlayersAlive[0]);
            }
        }
    }
    public static void DeclareWinner(long id){
        if (PlayersAlive.Contains(id)){
            GD.Print(GameManager.Players[id].Name," is the winner!");
        }
    }
}
