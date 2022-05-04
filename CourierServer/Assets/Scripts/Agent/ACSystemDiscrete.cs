using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System;
using TMPro;
using System.IO;

public class ACSystemDiscrete : Agent
{
    public static ACSystemDiscrete agent;
    //public TextMeshPro AgentBotState;

    private Player player;
    private Gatherer bot;

    const int positive = 0;
    const int neutral = 1;
    const int negative = 2;

    private void Start()
    {
        // create static instance and connect with other scripts
        agent = this;
        player = agent.GetComponentInParent<Player>();
        bot = agent.GetComponentInParent<Gatherer>();

        InvokeRepeating("SaveAgentPercentage", 1f, 1f);
    }

    string directory = "LogGame/perc/";
    string version = "v0.72_2v2.txt";
    string filepath = "";

    private void SaveAgentPercentage()
    {
        if (player != null)
        {
            filepath = string.Concat(directory, "agent_perc_", player.id, "_", player.team, "_player_", version);
            File.AppendAllText(filepath, player.AgentStatePerc.text + "\n");
        }
        else if (bot != null)
        {
            filepath = string.Concat(directory, "agent_perc_", bot.id, "_", bot.team , "_bot_", version);
            File.AppendAllText(filepath, bot.AgentStatePerc.text + "\n");
        }
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
        }

        // monitor all items
        foreach (var i in Items.items)
        {
            var item = i.Value;
            if ((player != null && player.team == item.team) || (bot != null && bot.team == item.team))
            {
                // 3 floats
                sensor.AddObservation(item.transform.localPosition);
                // 3 floats
                sensor.AddObservation(item.transform.localRotation);
            }
        }

        // monitor overall score
        // 1 int
        sensor.AddObservation(PointArea.BlueNumberOfObjects);
        // 1 int
        sensor.AddObservation(PointArea.RedNumberOfObjects);

        base.CollectObservations(sensor);
    }

    private List<int> states = new List<int>();
    private int current_action = -1;
    public override void OnActionReceived(ActionBuffers actions)
    {
        //  TRUE - player is cheating or bot
        //  NEUTRAL - unknown
        //  FALSE - player is human

        // if action is
        //      = 1  ---- player is cheating
        //      = -1 ---- player is human
        //      = 0  ---- neutral

        var action = actions.DiscreteActions[0];

        // take last 3000 actions
        // calculate percentage of cheating
        states.Add(action);

        if (states.Count > 3000)
        {
            var remove_number = states.Count - 3000;
            states.RemoveRange(0, remove_number);
        }
        var cheater_count = states.Where(x => x == positive).Count();
        double perc_cheater = Math.Round(((double)cheater_count / (double)states.Count()) * 100, 2);

        // set REWARDS
        //  +1 reward if agent chose correctly
        //  0  reward if agent didn't take action (neutral)
        //  -1 reward if agent chose wrongly
        if (player != null)
        {
            //Debug.Log(player.id + " action: " + action);
            player.AgentStatePerc.text = perc_cheater.ToString();
            switch (action)
            {
                case positive:
                    player.AgentState.text = "True";
                    SetReward(-1f);
                    break;
                case neutral:
                    player.AgentState.text = "Neutral";
                    SetReward(-0.01f);
                    break;
                case negative:
                    player.AgentState.text = "False";
                    SetReward(1f);
                    break;
            }
        }

        if (bot != null)
        {
            //Debug.Log(bot.id + " action: " + action);
            bot.AgentStatePerc.text = perc_cheater.ToString();
            switch (action)
            {
                case positive:
                    bot.AgentState.text = "True";
                    SetReward(1f);
                    break;
                case neutral:
                    bot.AgentState.text = "Neutral";
                    SetReward(-0.01f);
                    break;
                case negative:
                    bot.AgentState.text = "False";
                    SetReward(-1f);
                    break;
            }
        }

        base.OnActionReceived(actions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.F1))
            discreteActionsOut[0] = 0;
        else if (Input.GetKey(KeyCode.F2))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.F3))
            discreteActionsOut[0] = 2;
        base.Heuristic(actionsOut);
    }
}
