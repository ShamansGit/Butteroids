using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class GameManager : Node
{
    public static GameManager instance;
    //todo: change to dictionary?
    public static Dictionary<long,PlayerInfo> Players = new Dictionary<long,PlayerInfo>();
    public static int PlayerCount = 0;
    public static int UniquePlayerInstances = 0;
    [Export] public Vector2 mapSize = new Vector2(1600,1600);
    public Vector2 mapCentre => mapSize / 2f;
    public static Node levelInstance;
    public static bool GameRunning = false;
    public override void _Ready()
    {
        instance = this;
    }
    public static void AddPlayer(long id,PlayerInfo info){
        Players.Add(id,info);
		PlayerCount += 1;
    }
    public static void RemovePlayer(long id){
        Players.Remove(id);
		PlayerCount -= 1;

        var players = instance.GetTree().GetNodesInGroup("Player");
        foreach (var item in players)
        {
            if (item.Name == id.ToString())
            {
                item.QueueFree();
            }
        }
    }
    public static void StartGame(){
        Debug.Assert(!GameRunning,"A Game has to not be running already.");
        GameRunning = true;
        var newLevel = ResourceLoader.Load<PackedScene>("res://main_scene.tscn").Instantiate();
		instance.GetTree().Root.AddChild(newLevel);
        levelInstance = newLevel;
    }
    public static void EndGame(){
        Debug.Assert(GameRunning,"A Game has to be running to end it.");
        GameRunning = false;
        levelInstance.QueueFree();
    }
    public static Color GeneratePlayerColor(int playerNumber){
        return Color.FromHsv(0.2f * playerNumber,1f,1f);
    }
}
