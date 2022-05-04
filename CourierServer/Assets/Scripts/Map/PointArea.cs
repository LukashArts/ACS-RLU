using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PointArea : MonoBehaviour
{
    public static int BlueNumberOfObjects = 0;
    public static int RedNumberOfObjects = 0;
    public static int PlayingToScore = 5;
    public TextMeshProUGUI BlueScore;
    public TextMeshProUGUI RedScore;

    private static List<GameObject> BlueObjectsDone = new List<GameObject>();
    private static List<GameObject> RedObjectsDone = new List<GameObject>();
    private LayerMask InsidePointLayer;

    void Start()
    {
        InsidePointLayer = LayerMask.NameToLayer("DonePickup");
    }

    private void Update()
    {
        if (RedNumberOfObjects >= PlayingToScore)
            ResetScoreBoard("red");
        else if (BlueNumberOfObjects >= PlayingToScore)
            ResetScoreBoard("blue");

        if (Timer.timeRemaining <= 0)
        {
            if (BlueNumberOfObjects > RedNumberOfObjects)
                ResetScoreBoard("blue");
            else if (BlueNumberOfObjects < RedNumberOfObjects)
                ResetScoreBoard("red");
            else
                ResetScoreBoard("draw");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("B") && this.gameObject.name.Contains("Blue"))
        {
            if (!BlueObjectsDone.Contains(collision.gameObject))
            {
                BlueObjectsDone.Add(collision.gameObject);
                BlueNumberOfObjects++;
                collision.gameObject.layer = InsidePointLayer;
                BlueScore.text = BlueNumberOfObjects.ToString();
                Assets.Scripts.Log.Write($"{DateTime.Now}\tBlue team scored!");
                ServerSend.BluePoint(BlueNumberOfObjects);
            }
        }
        else if (collision.gameObject.name.Contains("R") && this.gameObject.name.Contains("Red"))
        {
            if (!RedObjectsDone.Contains(collision.gameObject))
            {
                RedObjectsDone.Add(collision.gameObject);
                RedNumberOfObjects++;
                collision.gameObject.layer = InsidePointLayer;
                RedScore.text = RedNumberOfObjects.ToString();
                Assets.Scripts.Log.Write($"{DateTime.Now}\tRed team scored!");
                ServerSend.RedPoint(RedNumberOfObjects);
            }
        }
    }

    private void ResetScoreBoard(string winner)
    {
        Assets.Scripts.Log.Write($"{DateTime.Now}\tTime remaining:{Timer.timeRemaining}");
        Assets.Scripts.Log.Write($"{DateTime.Now}\tWinner:{winner}\tScore:{BlueNumberOfObjects} - {RedNumberOfObjects}");

        // Shuffle teams so agent doesn't determine bot/player on their start position
        Server.spawn_points_red = ShuffleTeam(Server.spawn_points_red);
        Server.spawn_points_blue = ShuffleTeam(Server.spawn_points_blue);

        ServerSend.Wins(winner);
        ServerSend.BluePoint(0);
        ServerSend.RedPoint(0);
        BlueScore.text = "0";
        RedScore.text = "0";

        BlueObjectsDone = new List<GameObject>();
        RedObjectsDone = new List<GameObject>();

        Timer.timeRemaining = 185;

        // return all players to start position
        foreach (var player in Player.players)
        {
            var p = player.Value;
            p.controller.enabled = false;
            p.RespawnPlayerAtPosition();
            p.Drop();
            ServerSend.PlayerPosition(p);
            p.health = 0;
            ServerSend.PlayerHealth(p);
            //ServerSend.DisablePlayerModel(p);
            StartCoroutine(p.Respawn());

            // after 0.31 training
            //var ACSystem = p.GetComponent<ACSystem>();
            // 0.5 training DISCRETE actions
            var ACSystem = p.GetComponentInChildren<ACSystemDiscrete>();
            ACSystem.EndEpisode();
        }

        foreach (var bot in Server.bots)
        {
            bot.Value.TakeDamage(100);
            // after 0.31 training
            //var ACSystem = bot.Value.GetComponent<ACSystem>();
            // 0.5 training DISCRETE actions
            var ACSystem = bot.Value.GetComponentInChildren<ACSystemDiscrete>();
            ACSystem.EndEpisode();
        }

        Items.ReturnToStartPosition();
        GatherableResource.ReturnAvailable();
        BlueNumberOfObjects = 0;
        RedNumberOfObjects = 0;

        //AntiCheatAgent.agent.EndEpisode();
    }

    private System.Random rand = new System.Random();
    private Dictionary<Vector3, int> ShuffleTeam(Dictionary<Vector3, int> dict)
    {
        var red_value_list = dict.Values.ToList();
        var red_key_list = dict.Keys.ToList();
        var shuffled_list = red_value_list.OrderBy(x => rand.Next()).ToList();
        return red_key_list.Zip(shuffled_list, (x, y) => new { x, y }).ToDictionary(x => x.x, y => y.y);
    }
}
