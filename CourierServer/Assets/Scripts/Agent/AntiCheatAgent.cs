using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AntiCheatAgent : Agent
{
    public static AntiCheatAgent agent;

    private void Start()
    {
        // create static instance and connect with other scripts
        agent = this;
    }

    public override void OnEpisodeBegin()
    {
        // shuffle positions of players and bots
        base.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // max positions are 10 (players and bots) ?
        // add current score

        // monitor players
        foreach (var p in Player.players)
        {
            var player = p.Value;
            // 3 floats
            sensor.AddObservation(player.transform.localPosition);
            // 1 float
            sensor.AddObservation(player.transform.localRotation.y);
            // 3 floats
            sensor.AddObservation(player.controller.velocity);
            // 1 bool
            sensor.AddObservation(player.isPicked);
        }

        // monitor bots
        // 3 bots
        foreach (var b in Server.bots)
        {
            var bot = b.Value;
            // 3 floats
            sensor.AddObservation(bot.transform.localPosition);
            // 1 float
            sensor.AddObservation(bot.transform.localRotation.y);
            // 3 floats
            sensor.AddObservation(bot.navMeshAgent.velocity);
            // 1 bool
            sensor.AddObservation(bot.isPicked);
        }

        // monitor items
        foreach (var i in Items.items)
        {
            var item = i.Value;
            // 3 floats
            sensor.AddObservation(item.transform.localPosition);
            // 3 floats
            sensor.AddObservation(item.transform.localRotation);
        }

        // monitor overall score
        // 1 float
        sensor.AddObservation(PointArea.BlueNumberOfObjects);
        // 1 float
        sensor.AddObservation(PointArea.RedNumberOfObjects);

        base.CollectObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // set TRUE, NEUTRAL or FALSE to every player
        //  TRUE - player is cheating or bot
        //  NEUTRAL - unknown
        //  FALSE - player is human

        // set REWARDS
        //  +1 reward if agent chose correctly
        //  0  reward if agent didn't take action (neutral)
        //  -1 reward if agent chose wrongly

        // if action is
        //      = 1  ---- player is cheating
        //      = -1 ---- player is human
        //      = 0  ---- neutral

        //var action = actions.ContinuousActions[0];
        int i = 0;
        foreach (var p in Player.players)
        {
            var action = actions.ContinuousActions[i];
            if (action > 0)
            {
                p.Value.AgentState.text = "True";
                SetReward(-5f);
            }
            else if (action == 0)
                p.Value.AgentState.text = "Neutral";
            else if (action < 0)
            {
                p.Value.AgentState.text = "False";
                SetReward(2f);
            }
        }

        foreach (var b in Server.bots)
        {
            var action = actions.ContinuousActions[i];
            if (action > 0)
            {
                b.Value.AgentState.text = "True";
                SetReward(5f);
            }
            else if (action == 0)
                b.Value.AgentState.text = "Neutral";
            else if (action < 0)
            {
                b.Value.AgentState.text = "False";
                SetReward(-2f);
            }
        }

        base.OnActionReceived(actions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.F1))
            continuousActionsOut[0] = 1;
        else
            continuousActionsOut[0] = -1;

        if (Input.GetKey(KeyCode.F2))
            continuousActionsOut[1] = 1;
        else
            continuousActionsOut[1] = -1;

        if (Input.GetKey(KeyCode.F3))
            continuousActionsOut[2] = 1;
        else
            continuousActionsOut[2] = -1;

        //base.Heuristic(actionsOut);
    }
}
