using Microsoft.AspNetCore.SignalR;

namespace GameServer.Socket
{
    public class SocketServerHub : Hub
    {
        private Dictionary<string, int> _clientSocketIds = new Dictionary<string, int>();

        public void SetUserId(int id)
        {
            var connectionId = Context.ConnectionId;
            _clientSocketIds[connectionId] = id;
            Clients.Client(connectionId).SendAsync("socket_id_saved");
            Console.WriteLine($"LENGTH! {_clientSocketIds.Count}");
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected! {_clientSocketIds.Count}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            _clientSocketIds.Remove(connectionId);
            Console.WriteLine($"Client disconnected! {_clientSocketIds.Count}");
            return base.OnDisconnectedAsync(exception);
        }

        public void SendToAllClients(string eventName, params object[] data)
        {
            Console.WriteLine($"EVENT {eventName} SENDED TO ALL CLIENTS");
            Clients.All.SendAsync(eventName, data);
        }

        public void SendToClientsInList(int[] socketIdList, string eventName, params object[] data)
        {
            foreach (var socketId in socketIdList)
            {
                var client = _clientSocketIds.FirstOrDefault(x => x.Value == socketId);
                if (client.Key != null)
                {
                    Console.WriteLine($"EVENT {eventName} SENDED TO Clients in list with id {socketId}");
                    Clients.Client(client.Key).SendAsync(eventName, data);
                }
            }
        }

        public void SendToClient(int socketId, string eventName, params object[] data)
        {
            var client = _clientSocketIds.FirstOrDefault(x => x.Value == socketId);
            if (client.Key != null)
            {
                Console.WriteLine($"EVENT {eventName} SENDED TO Client with id {socketId}");
                Clients.Client(client.Key).SendAsync(eventName, data);
            }
        }
    }
}
