using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();
        string _team = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}, TEAM: {_team}) has assumed the wrong client ID ({_clientIdCheck})!");
        Server.clients[_fromClient].SendIntoGame(_username, _team);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
            _inputs[i] = _packet.ReadBool();
        Quaternion _rotation = _packet.ReadQuaternion();
        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void PickupItem(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.PickupItem();
    }
}
