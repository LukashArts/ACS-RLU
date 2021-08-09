using System;
using System.Collections;
using System.Collections.Generic;
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
        }

        foreach (var bot in Server.bots)
            bot.Value.TakeDamage(100);

        Items.ReturnToStartPosition();
        GatherableResource.ReturnAvailable();
        BlueNumberOfObjects = 0;
        RedNumberOfObjects = 0;
    }
}
