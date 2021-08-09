using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        string _team = _packet.ReadString();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, _team);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
            _player.transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
            _player.transform.localRotation = _rotation;
    }

    public static void SpawnBot(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        string _team = _packet.ReadString();

        GameManager.instance.SpawnBot(_id, _position, _rotation, _team);
    }

    public static void BotPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        if (GameManager.bots.TryGetValue(_id, out BotManager _bot))
            _bot.transform.position = _position;
    }

    public static void BotRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.bots.TryGetValue(_id, out BotManager _bot))
            _bot.transform.localRotation = _rotation;
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_health);
    }

    public static void BotHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        if (GameManager.bots.ContainsKey(_id))
            GameManager.bots[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _objectName = _packet.ReadString();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            // get object by name
            var carriedObject = GameObject.Find(_objectName);
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            if (_player.Destination != null)
            {
                carriedObject.transform.rotation = _player.Destination.rotation;
                carriedObject.transform.position = _player.Destination.position;
            }
        }
    }

    public static void SpawnProjectile(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        GameManager.instance.SpawnProjectile(_projectileId, _position);
    }

    public static void ProjectilePosition(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.projectiles.ContainsKey(_projectileId))
            GameManager.projectiles[_projectileId].transform.position = _position;
    }

    public static void ProjectileExploded(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.projectiles.ContainsKey(_projectileId))
            GameManager.projectiles[_projectileId].Explode(_position);
    }

    public static void ItemPosition(Packet _packet)
    {
        int _playerId = _packet.ReadInt();
        string _objectName = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.items.ContainsKey(_objectName))
        {
            GameManager.items[_objectName].transform.position = _position;

            var carriedObject = GameObject.Find(_objectName);
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            carriedObject.transform.position = _position;
            carriedObject.transform.rotation = _rotation;
        }
    }

    public static void BluePoint(Packet _packet)
    {
        int _blueScore = _packet.ReadInt();
        GameManager.instance.SetBlueScore(_blueScore);
    }

    public static void RedPoint(Packet _packet)
    {
        int _redScore = _packet.ReadInt();
        GameManager.instance.SetRedScore(_redScore);
    }

    public static void Winner(Packet _packet)
    {
        string winner = _packet.ReadString();
        GameManager.instance.SetWinner(winner);
    }

    public static void PlayerColors(Packet _packet)
    {
        var count = _packet.ReadInt();
        for (int i = 0; i < count; i++)
        {
            var player = _packet.ReadString().Split('_');
            var _id = int.Parse(player[0]);
            var team = player[1];
            if (GameManager.players.ContainsKey(_id))
                GameManager.players[_id].SetColor(team);
        }
    }

    public static void TimeRemaining(Packet _packet)
    {
        float timeRemaining = _packet.ReadFloat();
        GameManager.instance.Timer.text = timeRemaining.ToString("##");
    }
}
