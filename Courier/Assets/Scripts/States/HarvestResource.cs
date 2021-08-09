using UnityEngine;

internal class HarvestResource : IState
{
    private readonly Gatherer _gatherer;

    public HarvestResource(Gatherer gatherer)
    {
        _gatherer = gatherer;
    }

    public void Tick()
    {
        if (_gatherer.Target != null)
            _gatherer.TakeFromTarget();
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }
}