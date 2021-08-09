using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject projectilePrefab;

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

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(10, 26950);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0, -10f, 0), Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiateProjectile(Vector3 _shootOrigin)
    {
        return Instantiate(projectilePrefab, _shootOrigin, Quaternion.identity).GetComponent<Projectile>();
    }

    public Gatherer InstantiateBot(Vector3 position)
    {
        return Instantiate(botPrefab, position, Quaternion.identity).GetComponent<Gatherer>();
    }
}
