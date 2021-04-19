using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointArea : MonoBehaviour
{
    public int BlueNumberOfObjects;
    public int RedNumberOfObjects;
    public TextMeshProUGUI BlueScore;
    public TextMeshProUGUI RedScore;

    private List<GameObject> BlueObjectsDone = new List<GameObject>();
    private List<GameObject> RedObjectsDone = new List<GameObject>();
    private LayerMask InsidePointLayer;

    void Start()
    {
        InsidePointLayer = LayerMask.NameToLayer("DonePickup");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("B") && this.gameObject.name.Contains("Blue"))
        {
            if (!BlueObjectsDone.Contains(collision.gameObject))
            {
                BlueNumberOfObjects++;
                BlueObjectsDone.Add(collision.gameObject);
                collision.gameObject.layer = InsidePointLayer;
                BlueScore.text = BlueObjectsDone.Count.ToString();
            }
        }
        else if (collision.gameObject.name.Contains("R") && this.gameObject.name.Contains("Red"))
        {
            if (!RedObjectsDone.Contains(collision.gameObject))
            {
                RedNumberOfObjects++;
                RedObjectsDone.Add(collision.gameObject);
                collision.gameObject.layer = InsidePointLayer;
                RedScore.text = RedObjectsDone.Count.ToString();
            }
        }
    }
}
