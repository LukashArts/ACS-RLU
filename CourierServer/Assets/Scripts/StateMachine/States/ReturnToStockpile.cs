using UnityEngine;
using UnityEngine.AI;

internal class ReturnToStockpile : IState
{
    private readonly Gatherer _gatherer;
    private readonly NavMeshAgent _navMeshAgent;
    private readonly Animator _animator;
    private static readonly int Speed = Animator.StringToHash("Speed");

    public ReturnToStockpile(Gatherer gatherer, NavMeshAgent navMeshAgent)
    {
        _gatherer = gatherer;
        _navMeshAgent = navMeshAgent;
    }

    public void Tick()
    {

    }

    public void OnEnter()
    {
        var stockpiles = Object.FindObjectsOfType<StockPile>();
        //var stockPile = Object.FindObjectOfType<StockPile>();
        foreach(var stockPile in stockpiles)
        {
            if(stockPile.team == _gatherer.team)
            {
                _gatherer.StockPile = stockPile;
                _navMeshAgent.enabled = true;
                _navMeshAgent.SetDestination(_gatherer.StockPile.stockPilePosition);
            }
        }
        //_animator.SetFloat(Speed, 1f);
    }

    public void OnExit()
    {
        _gatherer.Drop();
        _navMeshAgent.enabled = false;
        //_animator.SetFloat(Speed, 0f);
    }
}