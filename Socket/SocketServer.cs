using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

namespace GameServer.SocketServer;

public class SocketServer
{
    private SocketIOServer _socketServer;
    private Dictionary<SocketIOSocket, int> _clientSocketIds;

    public SocketServer()
    {
        // Создание и настройка экземпляра сервера Socket.IO
        _socketServer = new SocketIOServer(new SocketIOServerOption(3000));
        _clientSocketIds = new Dictionary<SocketIOSocket, int>();

        _socketServer.OnConnection((SocketIOSocket socket) =>
        {
            Console.WriteLine($"Clients connected! {socket.Server.ClientsCounts}");

            // var id = socket.GetHashCode();
            // _clientSocketIds.Add(socket, id);

            socket.Emit("connect");

            socket.On("set_user_id", (JToken[] data) =>
            {
                var id = data[0].Value<int>();
                _clientSocketIds.Add(socket, id);
                socket.Emit("socket_id_saved");
                Console.WriteLine($"LENGTH! {_clientSocketIds.Count}");
            });

            socket.On(SocketIOEvent.DISCONNECT, () =>
            {
                _clientSocketIds.Remove(socket);
                Console.WriteLine($"Client disconnected! {_clientSocketIds.Count}");
            });
        });

        // Запуск сервера Socket.IO
        _socketServer.Start();
        Console.WriteLine($"Server started {_socketServer.Option.Port}");
    }

    public void SendToAllClients(JToken eventName, params object[] data)
    {
        Console.WriteLine("HERE IN FUNC EMIT");
        _socketServer.Emit(eventName, data);
    }

    public void SendToClientsInList(int[] socketIdList, string eventName, params object[] data)
    {
        foreach (var socketId in socketIdList)
        {
            var client = _clientSocketIds.FirstOrDefault(x => x.Value == socketId).Key;
            Console.WriteLine(client);
            if (client != null)
            {
                Console.WriteLine("EMITTED");
                client.Emit(eventName, data);
            }
        }
    }
    
    public void SendToClient(int socketId, string eventName, params object[] data)
    {
        Console.WriteLine($"EVENT SENDED TO {socketId}");
        var client = _clientSocketIds.FirstOrDefault(x => x.Value == socketId).Key;
        if (client != null)
        {
            client.Emit(eventName, data);
        }
    }
}