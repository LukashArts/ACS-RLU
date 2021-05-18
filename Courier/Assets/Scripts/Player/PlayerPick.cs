using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPick : MonoBehaviour
{
    public float pickUpDistance = 2f;
    public Transform Destination;

    private Transform carriedObject = null;
    private int pickupLayer;

    public bool isPicked = false;

    void Start()
    {
        pickupLayer = 1 << LayerMask.NameToLayer("Pickup");
    }

    void Update()
    {
        if (PlayerMovement.DropObjects)
        {
            Drop();
            isPicked = false;
        }

        if (isPicked && carriedObject != null)
        {
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            carriedObject.rotation = Destination.rotation;
            carriedObject.position = Destination.position;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (isPicked && carriedObject != null)
            {
                Drop();
                isPicked = false;
            }
            else
            {
                PickUp();
                isPicked = true;
            }
        }
    }

    private void Drop()
    {
        PlayerMovement.DropObjects = false;
        if(carriedObject != null)
        {
            carriedObject.GetComponent<Rigidbody>().useGravity = true;
            carriedObject.GetComponent<BoxCollider>().enabled = true;
            carriedObject = null;
        }
    }

    private void PickUp()
    {
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
            }
        }

        if (carriedObject != null) // Check if we found something
        {
            // Set the box in front of character
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            carriedObject.rotation = Destination.rotation;
            carriedObject.position = Destination.position;
        }
    }
}
