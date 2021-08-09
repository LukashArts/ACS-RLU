using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player, string _team)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);
            _packet.Write(_team);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void SpawnBot(int _toClient, Gatherer _bot, string _team)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnBot))
        {
            _packet.Write(_bot.id);
            _packet.Write(_bot.transform.position);
            _packet.Write(_bot.transform.rotation);
            _packet.Write(_team);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            //Debug.Log(_player.transform.position + " rot: " + _player.transform.rotation);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
            //SendUDPDataToAll(_packet);
        }
    }

    public static void BotPosition(Gatherer _bot)
    {
        using (Packet _packet = new Packet((int)ServerPackets.botPosition))
        {
            _packet.Write(_bot.id);
            _packet.Write(_bot.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    public static void BotRotation(Gatherer _bot)
    {
        using (Packet _packet = new Packet((int)ServerPackets.botRotation))
        {
            _packet.Write(_bot.id);
            _packet.Write(_bot.transform.rotation);

            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);
            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHealth(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void BotHealth(Gatherer _bot)
    {
        using (Packet _packet = new Packet((int)ServerPackets.botHealth))
        {
            _packet.Write(_bot.id);
            _packet.Write(_bot.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }

    //public static void BotRespawned(Gatherer _bot)
    //{
    //    using (Packet _packet = new Packet((int)ServerPackets.botRespawned))
    //    {
    //        _packet.Write(_bot.id);
    //        SendTCPDataToAll(_packet);
    //    }
    //}

    public static void ItemPickedUp(int _playerId, string _objectName)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_playerId);
            _packet.Write(_objectName);

            SendTCPDataToAll(_packet);
        }
    }

    public static void SpawnProjectile(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ProjectilePosition(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectilePosition))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    public static void ItemPosition(int _playerId, Items _item)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemPosition))
        {
            _packet.Write(_playerId);
            _packet.Write(_item.name);
            _packet.Write(_item.transform.position);
            _packet.Write(_item.transform.rotation);

            SendUDPDataToAll(_packet);
        }
    }

    public static void ProjectileExploded(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectileExploded))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);

            SendTCPDataToAll(_packet);
        }
    }

    public static void BluePoint(int blue_score)
    {
        using(Packet _packet = new Packet((int)ServerPackets.bluePoint))
        {
            _packet.Write(blue_score);
            SendTCPDataToAll(_packet);
        }
    }

    public static void RedPoint(int red_score)
    {
        using(Packet _packet = new Packet((int)ServerPackets.redPoint))
        {
            _packet.Write(red_score);
            SendTCPDataToAll(_packet);
        }
    }

    public static void Wins(string winner)
    {
        using(Packet _packet = new Packet((int)ServerPackets.winner))
        {
            _packet.Write(winner);
            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerColors(Player player, Dictionary<int, string> players)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerColors))
        {
            _packet.Write(players.Count);
            foreach (var p in players)
                _packet.Write(p.Key + "_" + p.Value);
            _packet.Write(player.id + "_" + player.team);
            SendTCPDataToAll(_packet);
        }
    }

    public static void TimeRemaining(float timeRemaining)
    {
        using (Packet _packet = new Packet((int)ServerPackets.timeRemaining))
        {
            _packet.Write(timeRemaining);
            SendTCPDataToAll(_packet);
        }
    }

    #endregion
}
