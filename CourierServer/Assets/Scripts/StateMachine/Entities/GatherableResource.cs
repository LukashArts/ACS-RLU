using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GatherableResource : MonoBehaviour
{
    [SerializeField] private int _totalAvailable = 1;
    public static Dictionary<int, GatherableResource> gatherable_resources = new Dictionary<int, GatherableResource>();

    public int _id;
    private static int nextId = 1;
    public int _available;
    public bool IsDepleted => _available <= 0;
    public string team;

    private void Start()
    {
        _id = nextId;
        nextId++;
        gatherable_resources.Add(_id, this);
    }

    private void OnEnable()
    {
        _available = _totalAvailable;
    }

    public bool Take()
    {
        if (_available <= 0)
            return false;
        _available--;
        
        return true;
    }

    //[ContextMenu("Snap")]
    //private void Snap()
    //{
    //    if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
    //    {
    //        transform.position = hit.position;
    //    }
    //}

    public static void ReturnAvailable()
    {
        foreach(var gr in gatherable_resources)
            gr.Value._available = 1;
    }

    public void SetAvailable(int amount) => _available = amount;
}