using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, BotManager> bots = new Dictionary<int, BotManager>();
    public static Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();
    public static Dictionary<string, ItemManager> items = new Dictionary<string, ItemManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject projectilePrefab;

    public TextMeshProUGUI Timer;
    public TextMeshProUGUI BlueScore;
    public TextMeshProUGUI RedScore;
    public GameObject WinnerRedTeam;
    public GameObject WinnerBlueTeam;
    public GameObject Draw;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    /// <summary>Spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_name">The player's name.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    /// <param name="_team">The player's starting team.</param>
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, string _team)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        else
            _player = Instantiate(playerPrefab, _position, _rotation);

        _player.GetComponent<PlayerManager>().Initialize(_id, _username, _team);
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }

    public void SpawnBot(int _id, Vector3 _position, Quaternion _rotation, string _team)
    {
        GameObject _bot;
        _bot = Instantiate(botPrefab, _position, _rotation);
        var botManager = _bot.GetComponent<BotManager>();
        botManager.Initialize(_id, _team);
        if (!bots.ContainsKey(_id))
            bots.Add(_id, botManager);
    }

    public void SpawnProjectile(int _id, Vector3 _position)
    {
        GameObject _projectile = Instantiate(projectilePrefab, _position, Quaternion.identity);
        _projectile.GetComponent<ProjectileManager>().Initialize(_id);
        projectiles.Add(_id, _projectile.GetComponent<ProjectileManager>());

        StartCoroutine(ExplodeAfterTime(_projectile, _id));
    }

    private IEnumerator ExplodeAfterTime(GameObject projectile, int id)
    {
        yield return new WaitForSeconds(3f);
        projectiles.Remove(id);
        Destroy(projectile);
    }

    public void SetRedScore(int redScore)
    {
        RedScore.text = redScore.ToString();
    }

    public void SetBlueScore(int blueScore)
    {
        BlueScore.text = blueScore.ToString();
    }

    public void SetWinner(string winner)
    {
        if (winner == "blue")
            WinnerBlueTeam.SetActive(true);
        else if (winner == "red")
            WinnerRedTeam.SetActive(true);
        else
            Draw.SetActive(true);
        StartCoroutine(HideWinner());
    }

    IEnumerator HideWinner()
    {
        yield return new WaitForSeconds(5f);
        WinnerBlueTeam.SetActive(false);
        WinnerRedTeam.SetActive(false);
        Draw.SetActive(false);
    }
}
