using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager instance;
    //todo: change to dictionary?
    public static Dictionary<int,PlayerInfo> Players = new Dictionary<int,PlayerInfo>();
    public static int PlayerCount = 0;
    public static int UniquePlayerInstances = 0;
    [Export] public Vector2 mapSize = new Vector2(1600,1600);
    public Vector2 mapCentre => mapSize / 2f;
    public override void _Ready()
    {
        instance = this;
    }
    public static void AddPlayer(int id,PlayerInfo info){
        Players.Add(id,info);
		PlayerCount += 1;
    }
    public static void RemovePlayer(int id){
        Players.Remove(id);
		PlayerCount -= 1;
    }
    public static Color GeneratePlayerColor(int playerNumber){
        return Color.FromHsv(0.2f * playerNumber,1f,1f);
    }
}
