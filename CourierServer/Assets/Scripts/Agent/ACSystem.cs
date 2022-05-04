using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System;

public class ACSystem : Agent
{
    public static ACSystem agent;

    private Player player;
    private Gatherer bot;

    private void Start()
    {
        // create static instance and connect with other scripts
        agent = this;
        player = agent.GetComponent<Player>();
        bot = agent.GetComponent<Gatherer>();
    }

    public override void OnEpisodeBegin()
    {
        // shuffle positions of players and bots
        base.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // monitor this player only
        if (player != null)
        {
            sensor.AddObservation(player.transform.localPosition);
            sensor.AddObservation(player.transform.localRotation.y);
            sensor.AddObservation(player.controller.velocity);
            sensor.AddObservation(player.isPicked);
        }
        else if (bot != null)
        {
            sensor.AddObservation(bot.transform.localPosition);
            sensor.AddObservation(bot.transform.localRotation.y);
            sensor.AddObservation(bot.navMeshAgent.velocity);
            sensor.AddObservation(bot.isPicked);
            //Debug.Log(bot.transform.localPosition);
            //Debug.Log(bot.transform.localRotation.y);
            //Debug.Log(bot.navMeshAgent.velocity);
            //Debug.Log(bot.isPicked);
        }

        // monitor all items
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

    private static List<bool> states = new List<bool>();
    public override void OnActionReceived(ActionBuffers actions)
    {
        //  TRUE - player is cheating or bot
        //  NEUTRAL - unknown
        //  FALSE - player is human

        // if action is
        //      = 1  ---- player is cheating
        //      = -1 ---- player is human
        //      = 0  ---- neutral

        var action = actions.ContinuousActions[0];

        // take last 3000 actions
        // calculate percentage of cheating
        if (action > 0)
            states.Add(true);
        else if (action < 0)
            states.Add(false);

        if (states.Count > 1000)
        {
            var remove_number = states.Count - 1000;
            states.RemoveRange(0, remove_number);
        }
        var true_ones = states.Where(x => x == true).Count();
        double perc = Math.Round(((double)true_ones / (double)states.Count()) * 100, 2);

        // set REWARDS
        //  +1 reward if agent chose correctly
        //  0  reward if agent didn't take action (neutral)
        //  -1 reward if agent chose wrongly
        if (player != null)
        {
            player.AgentStatePerc.text = perc.ToString();
            if (action > 0)
            {
                player.AgentState.text = "True";
                SetReward(-1f);
            }
            else if (action == 0)
                player.AgentState.text = "Neutral";
            else if (action < 0)
            {
                player.AgentState.text = "False";
                SetReward(1f);
            }
        }

        if (bot != null)
        {
            bot.AgentStatePerc.text = perc.ToString();
            if (action > 0)
            {
                bot.AgentState.text = "True";
                SetReward(1f);
            }
            else if (action == 0)
                bot.AgentState.text = "Neutral";
            else if (action < 0)
            {
                bot.AgentState.text = "False";
                SetReward(-1f);
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
        base.Heuristic(actionsOut);
    }
}
