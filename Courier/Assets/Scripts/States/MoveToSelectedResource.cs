using UnityEngine;
using UnityEngine.AI;

internal class MoveToSelectedResource : IState
{
    private readonly Gatherer _gatherer;
    private readonly NavMeshAgent _navMeshAgent;

    private Vector3 _lastPosition = Vector3.zero;
    
    public float TimeStuck;

    public MoveToSelectedResource(Gatherer gatherer, NavMeshAgent navMeshAgent)
    {
        _gatherer = gatherer;
        _navMeshAgent = navMeshAgent;
    }
    
    public void Tick()
    {
        if (Vector3.Distance(_gatherer.transform.position, _lastPosition) <= 0f)
            TimeStuck += Time.deltaTime;

        _lastPosition = _gatherer.transform.position;
    }

    public void OnEnter()
    {
        TimeStuck = 0f;
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(_gatherer.Target.transform.position);
    }

    public void OnExit()
    {
        _navMeshAgent.enabled = false;
    }
}