using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public int id;

    private void Start()
    {
        Initialize(this.name);
    }

    public void Initialize(string name)
    {
        GameManager.items.Add(this.name, this);
    }
}
