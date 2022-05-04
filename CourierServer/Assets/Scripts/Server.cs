using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static int NumberOfBots = 2;
    public static Dictionary<Vector3, int> spawn_points_blue = new Dictionary<Vector3, int>();
    public static Dictionary<Vector3, int> spawn_points_red = new Dictionary<Vector3, int>();
    public static Dictionary<int, string> player_team = new Dictionary<int, string>();
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public static Dictionary<int, Gatherer> bots = new Dictionary<int, Gatherer>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    /// <summary>Starts the server.</summary>
    /// <param name="_maxPlayers">The maximum players that can be connected simultaneously.</param>
    /// <param name="_port">The port to start the server on.</param>
    public static void Start(int _maxPlayers, int _port)
    {
        Log.CreateFile();
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on port {Port}.");
        Log.Write($"Server started on port {Port}.\nMaxPlayers: {MaxPlayers}.\n\n");
    }

    /// <summary>Handles new TCP connections.</summary>
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
                return;

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                    return;

                if (clients[_clientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    // Ensures that the client is not being impersonated by another by sending a false clientID
                    clients[_clientId].udp.HandleData(_packet);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="_packet">The packet to send.</param>
    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    /// <summary>Initializes all necessary server data.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
            clients.Add(i, new Client(i));

        InitSpawnPoints();

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            { (int)ClientPackets.pickupItem, ServerHandle.PickupItem },
        };
        Debug.Log("Initialized packets.");

        // create bots
        for (int i = 1; i <= NumberOfBots; i++)
        {
            var team = "";
            var position = Vector3.zero;
            if (i % 2 == 0)
            {
                team = "red";
                var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_red);
                var keys = temp_dict.Keys;
                foreach (var key in keys)
                {
                    if (Server.spawn_points_red[key] == 0)
                    {
                        position = key;
                        var bot = NetworkManager.instance.InstantiateBot(position);
                        bot.team = team;
                        bot.model.material.color = Color.red;
                        Server.spawn_points_red[key] = 100 + bot.id;
                        bots.Add(bot.id, bot);
                        break;
                    }
                }
            }
            else
            {
                team = "blue";
                var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_blue);
                var keys = temp_dict.Keys;
                foreach (var key in keys)
                {
                    if (Server.spawn_points_blue[key] == 0)
                    {
                        position = key;
                        var bot = NetworkManager.instance.InstantiateBot(position);
                        bot.team = team;
                        bot.model.material.color = Color.blue;
                        Server.spawn_points_blue[key] = 100 + bot.id;
                        bots.Add(bot.id, bot);
                        break;
                    }
                }
            }
        }
        Debug.Log($"Spawned {NumberOfBots} bots.");
        Log.Write($"Spawned {NumberOfBots} bots.");
    }

    private static void InitSpawnPoints()
    {
        spawn_points_red.Add(new Vector3(-45, 2, 24), 0);
        spawn_points_red.Add(new Vector3(-45, 2, 28), 0);
        spawn_points_red.Add(new Vector3(-45, 2, 32), 0);
        spawn_points_red.Add(new Vector3(-45, 2, 36), 0);
        spawn_points_red.Add(new Vector3(-45, 2, 40), 0);

        spawn_points_blue.Add(new Vector3(126, 2, -24), 0);
        spawn_points_blue.Add(new Vector3(126, 2, -28), 0);
        spawn_points_blue.Add(new Vector3(126, 2, -32), 0);
        spawn_points_blue.Add(new Vector3(126, 2, -36), 0);
        spawn_points_blue.Add(new Vector3(126, 2, -40), 0);
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
