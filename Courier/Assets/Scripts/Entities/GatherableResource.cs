using System;
using UnityEngine;
using UnityEngine.AI;

public class GatherableResource : MonoBehaviour
{
    [SerializeField] private int _totalAvailable = 1;
    
    private int _available;
    public bool IsDepleted => _available <= 0;

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

    [ContextMenu("Snap")]
    private void Snap()
    {
        if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
    }

    public void SetAvailable(int amount) => _available = amount;
}