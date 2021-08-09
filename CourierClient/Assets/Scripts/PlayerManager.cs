using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public string team;
    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    public MeshRenderer model;
    public Transform Destination;

    public Material material_blue;
    public Material material_red;

    public void Initialize(int _id, string _username, string _team)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        team = _team;
    }

    public void SetHealth(float _health)
    {
        health = _health;
        if (health <= 0f)
            Die();
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }

    public void SetColor(string team)
    {
        if (team == "blue")
            model.material = material_blue;
        else if (team == "red")
            model.material = material_red;
    }
}
