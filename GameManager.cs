using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager instance;
    //todo: change to dictionary?
    public static Dictionary<int,PlayerInfo> Players = new Dictionary<int,PlayerInfo>();
    public static int PlayerCount = 0;
    public static void AddPlayer(int id,PlayerInfo info){
        Players.Add(id,info);
		PlayerCount += 1;
    }
    public static void RemovePlayer(int id){
        Players.Remove(id);
		PlayerCount -= 1;
    }
}
