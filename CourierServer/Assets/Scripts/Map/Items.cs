using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    public static Dictionary<int, Items> items = new Dictionary<int, Items>();
    public static Dictionary<int, Vector3> start_position = new Dictionary<int, Vector3>();
    public static Dictionary<int, Quaternion> start_rotation = new Dictionary<int, Quaternion>();
    private static int nextItemId = 1;
    public int id;
    public int playerId;
    public string team;
    private static LayerMask PickupLayer;

    private void Start()
    {
        PickupLayer = LayerMask.NameToLayer("Pickup");
        id = nextItemId;
        nextItemId++;
        items.Add(id, this);
        // feed all dictionaries with start values in order to reset the game (at the end)
        start_position.Add(id, this.transform.position);
        start_rotation.Add(id, this.transform.rotation);
    }

    private void FixedUpdate()
    {
        ServerSend.ItemPosition(playerId, this);
        //if(this.transform.position.y < -10)
        if(this.transform.position.y < -2)
            StartCoroutine(ReturnItemToStartPosition(this));
    }

    IEnumerator ReturnItemToStartPosition(Items item)
    {
        yield return new WaitForSeconds(2f);
        foreach(var i in items)
        {
            if (i.Value == item)
            {
                var rigid = i.Value.GetComponent<Rigidbody>();
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                i.Value.transform.position = start_position[i.Key];
                i.Value.transform.rotation = start_rotation[i.Key];
                i.Value.gameObject.layer = PickupLayer;
            }
        }
    }

    public static void ReturnToStartPosition()
    {
        // return all items back to it's original position
        foreach(var item in items)
        {
            items[item.Key].transform.position = start_position[item.Key];
            items[item.Key].transform.rotation = start_rotation[item.Key];
            // set layer to Pickup, to reset the DonePickup layer
            items[item.Key].gameObject.layer = PickupLayer;
        }
    }
}
