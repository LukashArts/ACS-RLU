using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public Transform cam;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // Gravity
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    private LayerMask groundMask;
    private LayerMask pickupLayer;
    private LayerMask waterLayer;
    public float JumpHeight = 3f;

    public static bool DropObjects = false;

    Vector3 velocity;
    bool isGrounded;
    bool isWater;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();
        groundMask = 1 << LayerMask.NameToLayer("Ground");
        pickupLayer = 1 << LayerMask.NameToLayer("Pickup");
        waterLayer = 1 << LayerMask.NameToLayer("Water");
    }

    /// <summary>
    /// Update function for player movement.
    /// </summary>
    void Update()
    {
        // Ground check for gravity
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask) || Physics.CheckSphere(groundCheck.position, groundDistance, pickupLayer);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Check for water collision
        isWater = Physics.CheckSphere(groundCheck.position, groundDistance, waterLayer);
        if (isWater)
        {
            controller.enabled = false;
            controller.transform.position = new Vector3(15, 2, 36);
            controller.enabled = true;
        }

        // Movement
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f)
        {
            float target_angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, target_angle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }

        // Jumping control
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(JumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Missile")
        {
            controller.enabled = false;
            controller.transform.position = new Vector3(15, 2, 36);
            controller.enabled = true;
            DropObjects = true;
        }
    }
}
