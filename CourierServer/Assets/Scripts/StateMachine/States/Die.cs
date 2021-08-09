using System;
using UnityEngine;
using UnityEngine.AI;

public class Die : IState
{
    private readonly Gatherer _gatherer;
    private NavMeshAgent _navMeshAgent;

    public Die(Gatherer gatherer, NavMeshAgent navMeshAgent)
    {
        _gatherer = gatherer;
        _navMeshAgent = navMeshAgent;
    }

    public void OnEnter()
    {
        Assets.Scripts.Log.Write($"{DateTime.Now}\tGot hit\tID:{_gatherer.id}\tname:{_gatherer.name}\tteam:{ _gatherer.team }");
        _navMeshAgent.enabled = false;
        ServerSend.BotHealth(_gatherer);
        _gatherer.model.enabled = false;
        _gatherer.RespawnPlayerAtPosition();
        _gatherer.Drop();
        _gatherer.StartCoroutine(_gatherer.Respawn());
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
        _gatherer.health = 100;
    }
}