using System.Linq;
using UnityEngine;

public class SearchForResource : IState
{
    private readonly Gatherer _gatherer;

    public SearchForResource(Gatherer gatherer)
    {
        _gatherer = gatherer;
    }
    public void Tick()
    {
        _gatherer.Target = ChooseOneOfTheNearestResources(5);
    }

    private GatherableResource ChooseOneOfTheNearestResources(int pickFromNearest)
    {
        var gatherableResource = Object.FindObjectsOfType<GatherableResource>().Where(x => x.team == _gatherer.team).ToList();
        var orderIt = gatherableResource.OrderBy(t => Vector3.Distance(_gatherer.transform.position, t.transform.position)).ToList();
        var notdepleted = orderIt.Where(t => t.IsDepleted == false).ToList();
        var take = notdepleted.Take(pickFromNearest).ToList();
        var order_max = take.OrderBy(t => Random.Range(0, int.MaxValue)).ToList();
        var first = order_max.FirstOrDefault();

        return first;
    }

    public void OnEnter() { }
    public void OnExit() { }
}