using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject cam;
    public float sensitivity = 100f;
    public float clampAngle = 85f;

    public float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            ClientSend.PickupItem();
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>Sends player input to the server.</summary>
    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space),
            Input.GetKey(KeyCode.LeftShift)
        };

        ClientSend.PlayerMovement(_inputs);
    }
}
