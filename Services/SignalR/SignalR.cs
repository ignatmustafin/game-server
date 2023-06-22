using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace GameServer.Services.SignalR
{
    public class SocketServerHub : Hub
    {
        private readonly Dictionary<int, string> _clientConnectionsIds = new Dictionary<int, string>();

        public async Task SetUserId(int id)
        {
            var connectionId = Context.ConnectionId;
            _clientConnectionsIds[id] = connectionId;
            Console.WriteLine($"Saved user id {id}, with connection id: {connectionId}");
            Console.WriteLine($"Client connected! Total count {_clientConnectionsIds.Count}");
            await Clients.Client(connectionId).SendAsync("socket_id_saved", connectionId);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var clientId = _clientConnectionsIds.FirstOrDefault(item => item.Value == connectionId);
            _clientConnectionsIds.Remove(clientId.Key);
            Console.WriteLine($"Client with id  {clientId.Key} disconnected. Connections left {_clientConnectionsIds.Count}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendToAllClients(string eventName, object data = null)
        {
            if (data == null)
            {
                await Clients.All.SendAsync(eventName);
            }
            else
            {
                var json = JsonConvert.SerializeObject(data);
                await Clients.All.SendAsync(eventName, json);
            }
            Console.WriteLine($"EVENT {data} SENDED TO ALL CLIENTS");
        }

        public async Task SendToClientsInList(int[] socketIdList, string eventName, object data = null)
        {
            foreach (var socketId in socketIdList)
            {
                var client = _clientConnectionsIds.FirstOrDefault(x => x.Key == socketId);
                if (client.Value != null)
                {
                    if (data == null)
                    {
                        await Clients.Client(client.Value).SendAsync(eventName);
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(data);
                        await Clients.Client(client.Value).SendAsync(eventName, json);
                    }
                    Console.WriteLine($"EVENT {eventName} SENDED TO Clients in list with id {socketId}");
                }
            }
        }

        public async Task SendToClient(int socketId, string eventName, object data = null)
        {
            var client = _clientConnectionsIds.FirstOrDefault(x => x.Key == socketId);
            if (client.Value != null)
            {
                if (data == null)
                {
                    await Clients.Client(client.Value).SendAsync(eventName);
                }
                else
                {
                    var json = JsonConvert.SerializeObject(data);
                    await Clients.Client(client.Value).SendAsync(eventName, json);
                }
                Console.WriteLine($"EVENT {eventName} SENDED TO Client with id {socketId}");
            }
        }
    }
}
