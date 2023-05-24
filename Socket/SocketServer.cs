using System;
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

    public SocketServer()
    {
        // Создание и настройка экземпляра сервера Socket.IO
        _socketServer = new SocketIOServer(new SocketIOServerOption(3000));

        _socketServer.OnConnection((SocketIOSocket socket) =>
        {
            Console.WriteLine($"Client connected! {_socketServer.ClientsCounts}");
            
            socket.Emit("connect", new {time = 15});

            socket.On(SocketIOEvent.DISCONNECT, () => { Console.WriteLine("Client disconnected!"); });
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
}