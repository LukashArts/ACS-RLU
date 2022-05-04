# Anti Cheat system by implementing RL agents in Unity

## MP Game called "Courier"
*Carry Boxes, Avoid Objects*

Blue side versus Red side. In order to win the game, you need to transfer five boxes to the other side in a certain time (185 seconds).

Players spawn at their base with packages near them.

At any time, players can be struck with objects and drop their package.
After being struck with an object or colliding with another player, package will fall from their hands.

## Build versions
The server and the client version are available in the Build folders of *CourierClient* and *CourierServer*. Unity is not needed in order to start the game. Folder *Courier* was only the test version of the game.

## Server
Inside the folder CourierServer, there is the latest build version of the server which needs to be started first. The maximum number of players is 10 and currently there are 2 bots inside the game after the server starts. In order to change the number of players, check */CourierServer/Assets/Scripts/Server.cs* where there is a static variable NumberOfBots and modify it.

When the server starts, you can see numbers and boolean value on top of each player, which shows the percentage of bot detection. More about that, you can read in the article (at the end of this readme). To move around the map, use WASD and mouse.

## Client
Inside the folder CourierClient, there is the latest build version of the client which needs to be started after the server. If the server is not started, the game will probably bug out. Many marginal cases were not fixed. Also, if there are more than 10 players, the 11th player will drop out of the map. Use WASD and mouse for movement, space for jumping and shift for faster running.

To connect to the server, all players need to be connected to the local network area.

## Starting the training
Currently, inside the server build there is a Reinforcement learning agent with the latest model *ACSystemDiscrete_v0.72_2v2*. If new training is wanted, download the Unity ML-Agents Toolkit (https://github.com/Unity-Technologies/ml-agents). The used version in this project is *ml-agents-release_18*. Put it inside the root of this project and refer to their documentation. You can use the YAML file which was used for the latest model, defined in the article (check Documentation folder inside project).

Go to the ml-agents build
```
cd %path%/ACS-RLU/ml-agents-release_18
```

Start Tensorboard
```
tensorboard --logdir results
```

Start ML-Agents training
```
mlagents-learn config/ppo/ACSystemDiscrete.yaml --run-id=ACS_RLU_v0.72 --time-scale=1
```
If not stated, time-scale is by default set to 20. If you don't want the scale of time, use --time-scale=1.

Put generated model inside the Unity (Server):
```
Assets/Prefabs/Agent
```
Set generated model inside *Behavior Paramteres/Model*

This was only tested on MS Windows 10.

## Article

*Anticheat System Based on Reinforcement Learning Agents in Unity*:
https://www.mdpi.com/2078-2489/13/4/173
