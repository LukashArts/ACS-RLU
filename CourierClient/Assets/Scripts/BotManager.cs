using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public int id;
    //public string username;
    public string team;
    public float health;
    public float maxHealth = 100f;

    public MeshRenderer model;
    public Transform Destination;

    public Material material_blue;
    public Material material_red;

    public void Initialize(int _id, string _team)
    {
        id = _id;
        health = maxHealth;
        team = _team;
        if (team == "red")
            model.material = material_red;
        else if (team == "blue")
            model.material = material_blue;
    }

    public void SetHealth(float _health)
    {
        health = _health;
        if (health <= 0)
            Die();
        else if (health == 100)
            Respawn();
    }

    public void Die()
    {
        model.enabled = false;
        health = maxHealth;
    }

    public void Respawn()
    {
        model.enabled = true;
    }

    public void SetColor(string team)
    {
        if (team == "blue")
            model.material = material_blue;
        else if (team == "red")
            model.material = material_red;
    }
}
