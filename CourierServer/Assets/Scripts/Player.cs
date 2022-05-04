using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<int, Player> players = new Dictionary<int, Player>();
    public int id;
    public string username;
    public float health;
    public string team;
    public float maxHealth = 100f;

    private bool[] inputs;
    private float yVelocity = 0;
    public float speed = 100f;
    public CharacterController controller;
    public MeshRenderer model;

    // Gravity
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    private LayerMask groundMask;
    private LayerMask pickupLayer;
    private LayerMask waterLayer;
    public float JumpHeight = 3f;

    Vector3 velocity;
    bool isGrounded;
    bool isWater;
    public bool isPicked = false;
    public float pickUpDistance = 2f;
    public Transform carriedObject = null;
    public Transform destination = null;

    public TextMeshPro AgentState;
    public TextMeshPro AgentStatePerc;

    private bool ShiftRun = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        groundMask = 1 << LayerMask.NameToLayer("Ground");
        pickupLayer = 1 << LayerMask.NameToLayer("Pickup");
        waterLayer = 1 << LayerMask.NameToLayer("Water");

        ServerSend.RedPoint(PointArea.RedNumberOfObjects);
        ServerSend.BluePoint(PointArea.BlueNumberOfObjects);
    }

    public void Initialize(int _id, string _username, string _team)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        team = _team;
        InitSpawnPosition();
        if (team == "blue")
            model.material.color = Color.blue;
        else if (team == "red")
            model.material.color = Color.red;

        inputs = new bool[6];
        players.Add(this.id, this);
    }

    public void SetColors(Dictionary<int, string> players)
    {
        ServerSend.PlayerColors(this, players);
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (health <= 0f)
            return;

        if (inputs == null)
        {
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
            _inputDirection.y += .01f;
        if (inputs[1])
            _inputDirection.y -= .01f;
        if (inputs[2])
            _inputDirection.x -= .01f;
        if (inputs[3])
            _inputDirection.x += .01f;
        ShiftRun = inputs[5];

        Move(_inputDirection);
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Move(Vector2 _inputDirection)
    {
        // Ground check for gravity
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask) || Physics.CheckSphere(groundCheck.position, groundDistance, pickupLayer);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Check for water collision
        isWater = Physics.CheckSphere(groundCheck.position, groundDistance, waterLayer);
        if (isWater)
            TakeDamage(100);

        // Movement
        Vector3 direction = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        if (ShiftRun)
            direction *= speed * 100 + 600;
        else
            direction *= speed * 100;

        // Jumping
        if (isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
                yVelocity = Mathf.Sqrt(JumpHeight * -2f * gravity);
        }
        yVelocity += gravity * Time.deltaTime;
        direction.y = yVelocity;
        if (controller.enabled)
            controller.Move(direction * Time.deltaTime);

        if (carriedObject != null) // Check if we found something
        {
            // Set the box in front of character
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            carriedObject.rotation = destination.rotation;
            carriedObject.position = destination.position;
        }

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0f)
            return;

        health -= _damage;
        if (health <= 0f)
        {
            Assets.Scripts.Log.Write($"{DateTime.Now}\tGot hit\tID:{this.id}\tname:{this.username}\tteam:{this.team}");
            health = 0f;
            controller.enabled = false;
            RespawnPlayerAtPosition();
            Drop();
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    public void RespawnPlayerAtPosition()
    {
        if (team == "blue")
        {
            var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_blue);
            var keys = temp_dict.Keys;
            foreach (var key in keys)
                if (Server.spawn_points_blue[key] == id)
                    transform.position = key;
        }
        else if (team == "red")
        {
            var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_red);
            var keys = temp_dict.Keys;
            foreach (var key in keys)
                if (Server.spawn_points_red[key] == id)
                    transform.position = key;
        }
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    public bool PickupItem()
    {
        // if it already has a item, drop it
        if (isPicked)
        {
            Drop();
            isPicked = false;
            return isPicked;
        }

        // Collect every pickups around. Make sure they have a collider and the layer Pickup
        Collider[] pickups = Physics.OverlapSphere(transform.position, pickUpDistance, pickupLayer);

        // Find the closest
        float dist = Mathf.Infinity;
        for (int i = 0; i < pickups.Length; i++)
        {
            float newDist = (transform.position - pickups[i].transform.position).sqrMagnitude;
            if (newDist < dist)
            {
                carriedObject = pickups[i].transform;
                dist = newDist;
                isPicked = true;
                ServerSend.ItemPickedUp(this.id, carriedObject.name);
            }
        }

        if (carriedObject != null) // Check if we found something
        {
            // Set the box in front of character
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            carriedObject.rotation = destination.rotation;
            carriedObject.position = destination.position;
        }

        return isPicked;
    }

    public void Drop()
    {
        //PlayerMovement.DropObjects = false;
        if (carriedObject != null)
        {
            carriedObject.GetComponent<Rigidbody>().AddForce(this.gameObject.GetComponent<CharacterController>().velocity * 40);
            carriedObject.GetComponent<Rigidbody>().useGravity = true;
            carriedObject.GetComponent<BoxCollider>().enabled = true;
            carriedObject = null;
        }
    }

    public void InitSpawnPosition()
    {
        if (team == "blue")
        {
            var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_blue);
            var keys = temp_dict.Keys;
            foreach (var key in keys)
            {
                if (Server.spawn_points_blue[key] == 0)
                {
                    transform.position = key;
                    Server.spawn_points_blue[key] = id;
                    break;
                }
            }
        }
        else if (team == "red")
        {
            var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_red);
            var keys = temp_dict.Keys;
            foreach (var key in keys)
            {
                if (Server.spawn_points_red[key] == 0)
                {
                    transform.position = key;
                    Server.spawn_points_red[key] = id;
                    break;
                }
            }
        }
    }
}
