using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MatchManager : Node2D
{
    public static MatchManager instance;
    [Export] PackedScene playerScene;
    [Export] float playerRingSize = 200;
    [Export] Label banner;
    static List<long> PlayersAlive = new List<long>();
    public override void _Ready()
    {
        base._Ready();
        instance = this;
        StartMatch(1);
    }
    public static void StartMatch(int matchNumber){
        PlayersAlive.Clear();
        int index = 0;
        foreach (var player in GameManager.Players)
        {
            long playerid = player.Key;
            Player playerInstance = instance.playerScene.Instantiate<Player>();
            playerInstance.Name = playerid.ToString();
            player.Value.color = GameManager.GeneratePlayerColor(index);

            //resets player health
            player.Value.lives = 3;

            //spawns players in an even ring around the centre
            float ringAngle = (float)index / Mathf.Max(1,GameManager.PlayerCount) * Mathf.Pi * 2;
            playerInstance.Position = new Vector2(Mathf.Sin(ringAngle),Mathf.Cos(ringAngle)) * instance.playerRingSize;

            //adds to scene
            instance.AddChild(playerInstance);
            playerInstance.SetMultiplayer(playerid,player.Value);

            index += 1;
            PlayersAlive.Add(playerid);
        }
        instance.banner.GetParent<Control>().Scale = new Vector2(1,0);
        ShowBannerMessage("Start!");
    }
    public static void EliminatePlayer(long id){
        if (PlayersAlive.Contains(id)){
            PlayersAlive.Remove(id);
            if (PlayersAlive.Count == 1 && GameManager.PlayerCount > 1){
                DeclareWinner(PlayersAlive[0]);
                instance.GetTree().CreateTimer(5).Timeout += EndMatch;
            }else if(PlayersAlive.Count == 0){
                ShowBannerMessage("Game Over!");
                instance.GetTree().CreateTimer(5).Timeout += EndMatch;
            }else{
                ShowBannerMessage(GameManager.Players[id].Name.Capitalize() + " was eliminated!");
            }
        }
    }
    public static void DeclareWinner(long id){
        if (PlayersAlive.Contains(id)){
            ShowBannerMessage(GameManager.Players[id].Name.Capitalize() + " is the winner!");
        }
    }
    public static void EndMatch(){
        //todo: There are errors for a few frames here if the host ends before the clients do.
        if (instance.Multiplayer.GetUniqueId() == 1) MultiplayerController.instance.Rpc(nameof(MultiplayerController.ReturnToLobby));
    }
    public static void ShowBannerMessage(string text){
        instance.banner.Text = text;
        GD.Print(text);
        Tween tween = instance.GetTree().CreateTween();
        tween.TweenProperty(instance.banner.GetParent(),"scale",Vector2.One,0.5f);
        tween.TweenInterval(2);
        tween.TweenProperty(instance.banner.GetParent(),"scale",new Vector2(1,0),0.5f);
    }
}
