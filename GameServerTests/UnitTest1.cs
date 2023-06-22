using System.Collections;
using System.Diagnostics;
using EngineIOSharp.Common.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Client;
using System.Net.Http;
using System.Text;
using GameServer.DTO.Game;
using GameServer.Services.Game;
using NLog;
using System.Threading.Tasks;
using GameServer.Models;
using Microsoft.AspNetCore.SignalR.Client;


namespace TestProject1;

public class Tests
{
    private class Qwe
    {
        private int GameId;
    }
    private record testRequest(int PlayerId, int CardId, string field);

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private HubConnection connection1;
    private HubConnection connection2;
    private HttpClient _client = new();
    private string connection1Id;

    private Guid link;
    private int gameId;
    private int player1Id;
    private int player2Id;
    private string test;
    private string qwe;
    private string asd;

    private GameDto.GameData player1GameData;
    private GameDto.GameData player2GameData;

    private GameDto.PlayerData player1Data;
    private GameDto.EnemyData player1EnemyData;
    private GameDto.PlayerData player2Data;
    private GameDto.EnemyData player2EnemyData;

    [SetUp]
    public void Setup()
    {
    }

    [Test, Order(1)]
    [Timeout(5000)]
    public async Task Test1()
    {
        connection1 = new HubConnectionBuilder()
            .WithUrl("http://localhost:5157/socket")
            .Build();

        await connection1.StartAsync();
        
        Console.WriteLine(connection1.State == HubConnectionState.Connected);

        TaskCompletionSource<bool> socket1EventReceived = new TaskCompletionSource<bool>(false);

        connection1.On<string>("socket_id_saved", (connectionId) =>
        {
            Console.WriteLine($"Socket 1 user id saved {connectionId}");
            connection1Id = connectionId;
            socket1EventReceived.SetResult(true);
        });

        await connection1.InvokeAsync("SetUserId", 1);


        Assert.IsTrue(socket1EventReceived.Task.Result);
    }

    [Test, Order(2)]
    [Timeout(5000)]
    public async Task Test2()
    {
        Console.WriteLine(connection1Id);
        connection2 = new HubConnectionBuilder()
            .WithUrl("http://localhost:5157/socket")
            .Build();
        
        await connection2.StartAsync();
        
        TaskCompletionSource<bool> socket1EventReceived = new TaskCompletionSource<bool>(false);

        connection2.On<string>("socket_id_saved", (connectionId) =>
        {
            Console.WriteLine($"Socket 2 user id saved {connectionId}");
            socket1EventReceived.SetResult(true);
        });
        
        await connection2.InvokeAsync("SetUserId", 3);
        
        Assert.IsTrue(socket1EventReceived.Task.Result);
    }

    

    [Test, Order(3)]
    [Timeout(5000)]
    public async Task CreateGame()
    {
        var json = JsonConvert.SerializeObject(new GameDto.CreateGameRequest(1));
        var body = new StringContent(json, Encoding.UTF8, "application/json");
    
        var response = await _client.PostAsync("http://localhost:5157/game/create-game", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.CreateGameResponse>(responseBody);
        
        var test = new Guid(responseObject.Link.ToString());
        link = test;
        player1Id = responseObject.PlayerId;
        gameId = responseObject.GameId;
        Assert.IsNotNull(responseObject);
    }
    
    [Test, Order(4)]
    [Timeout(5000)]
    public async Task JoinGame()
    {
        TaskCompletionSource<bool> socket1EventReceived = new TaskCompletionSource<bool>(false);
        TaskCompletionSource<bool> socket2EventReceived = new TaskCompletionSource<bool>(false);
        
        connection1.On<string>("all_users_joined_lobby", (data) =>
        {
            Console.WriteLine($"HERE DATA CON1 {data}");
            test = data;
            socket1EventReceived.SetResult(true);
        });
    
        connection2.On<string>("all_users_joined_lobby", (data) =>
        {
            Console.WriteLine($"HERE DATA CON2 {data}");
            socket2EventReceived.SetResult(true);
        });
        
        var json = JsonConvert.SerializeObject(new GameDto.JoinGameRequest(link, 3));
        var body = new StringContent(json, Encoding.UTF8, "application/json");
    
        var response = await _client.PostAsync("http://localhost:5157/game/join-game", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.JoinGameResponse>(responseBody);
        
        
        Assert.IsNotNull(responseObject);
        player2Id = responseObject.PlayerId;
        
        await Task.WhenAll(socket1EventReceived.Task, socket2EventReceived.Task);
        Assert.IsTrue(socket1EventReceived.Task.Result);
        Assert.IsTrue(socket2EventReceived.Task.Result);
    }
    //
    [Test, Order(5)]
    public async Task LoadPlayer1()
    {
        Console.WriteLine(test);
        var json = JsonConvert.SerializeObject(new GameDto.IsLoadedRequest(player1Id, gameId));
        var body = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("http://localhost:5157/game/loaded-game", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.IsLoadedResponse>(responseBody);
        
        Assert.IsTrue(responseObject.Success);
    }
    
    [Test, Order(6)]
    [Timeout(5000)]
    public async Task LoadPlayer2()
    {
        TaskCompletionSource<bool> socket1EventReceived = new TaskCompletionSource<bool>(false);
        TaskCompletionSource<bool> socket2EventReceived = new TaskCompletionSource<bool>(false);
        
        connection1.On<string>("update_game_data",  (data) =>
        {
            var responseObject = JsonConvert.DeserializeObject<GameDto.GameData>(data);
            if (responseObject != null)
            {
                player1GameData = responseObject;
            }
            socket1EventReceived.SetResult(true);
        });
    
        connection2.On<string>("update_game_data", (data) =>
        {
            var responseObject = JsonConvert.DeserializeObject<GameDto.GameData>(data);
            if (responseObject != null)
            {
                player2GameData = responseObject;
            }
            socket2EventReceived.SetResult(true);
        });
        
        
        var json = JsonConvert.SerializeObject(new GameDto.IsLoadedRequest(player2Id, gameId));
        var body = new StringContent(json, Encoding.UTF8, "application/json");
    
        var response = await _client.PostAsync("http://localhost:5157/game/loaded-game", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.IsLoadedResponse>(responseBody);
        
        Assert.IsTrue(responseObject.Success);
        await Task.WhenAll(socket1EventReceived.Task, socket2EventReceived.Task);
        Assert.IsTrue(socket1EventReceived.Task.Result);
        Assert.IsTrue(socket2EventReceived.Task.Result);
    }
    
    [Test, Order(7)]
    public void CheckPlayer1GameData()
    {
        player1Data = player1GameData.PlayerData;
        player1EnemyData = player1GameData.EnemyData;
        Assert.That(player1Data.Name, Is.EqualTo("Ignat"));
        Assert.That(player1Data.Hp, Is.EqualTo(30));
        Assert.That(player1Data.Mana, Is.EqualTo(1));
        Assert.That(player1Data.CardsInHand.Count, Is.EqualTo(3));
        
        Assert.That(player1EnemyData.Name, Is.EqualTo("Sasha"));
        Assert.That(player1EnemyData.Hp, Is.EqualTo(30));
        Assert.That(player1EnemyData.Mana, Is.EqualTo(1));
        Assert.That(player1EnemyData.CardsInHand.Count, Is.EqualTo(3));
    }
    
    [Test, Order(8)]
    public void CheckPlayer2GameData()
    {
        player2Data = player2GameData.PlayerData;
        player2EnemyData = player2GameData.EnemyData;
        Assert.That(player2Data.Name, Is.EqualTo("Sasha"));
        Assert.That(player2Data.Hp, Is.EqualTo(30));
        Assert.That(player2Data.Mana, Is.EqualTo(1));
        Assert.That(player2Data.CardsInHand.Count, Is.EqualTo(3));
        
        Assert.That(player2EnemyData.Name, Is.EqualTo("Ignat"));
        Assert.That(player2EnemyData.Hp, Is.EqualTo(30));
        Assert.That(player2EnemyData.Mana, Is.EqualTo(1));
        Assert.That(player2EnemyData.CardsInHand.Count, Is.EqualTo(3));
    }
    
    [Test, Order(9)]
    [Timeout(5000)]
    public async Task ThrowCardByPlayer1()
    {
        TaskCompletionSource<bool> socket1EventReceived = new TaskCompletionSource<bool>(false);
        TaskCompletionSource<bool> socket2EventReceived = new TaskCompletionSource<bool>(false);
        
        connection1.On<string>("update_game_data",  (data) =>
        {
            var responseObject = JsonConvert.DeserializeObject<GameDto.GameData>(data);
            if (responseObject != null)
            {
                player1GameData = responseObject;
            }
            socket1EventReceived.SetResult(true);
        });
    
        connection2.On<string>("update_game_data", (data) =>
        {
            var responseObject = JsonConvert.DeserializeObject<GameDto.GameData>(data);
            if (responseObject != null)
            {
                player2GameData = responseObject;
            }
            socket2EventReceived.SetResult(true);
        });
        
        
        var json = JsonConvert.SerializeObject(new GameDto.CardThrownRequest(player1Id, player1Data.CardsInHand.ElementAt(0).Id, CardIn.Field1));
        var body = new StringContent(json, Encoding.UTF8, "application/json");
    
        var response = await _client.PostAsync("http://localhost:5157/game/card-thrown", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.CardThrownResponse>(responseBody);
    
        Assert.IsTrue(responseObject.Success);
        await Task.WhenAll(socket1EventReceived.Task, socket2EventReceived.Task);
        Assert.IsTrue(socket1EventReceived.Task.Result);
        Assert.IsTrue(socket2EventReceived.Task.Result);
        
    }
    
    [Test, Order(10)]
    [Timeout(5000)]
    public void CheckPlayerDataAfterCardThrown()
    {
        player1Data = player1GameData.PlayerData;
        player1EnemyData = player1GameData.EnemyData;
        player2Data = player2GameData.PlayerData;
        player2EnemyData = player2GameData.EnemyData;
        
        Assert.That(player1Data.CardsInHand.Count, Is.EqualTo(2));
        Assert.That(player1Data.Field1, Is.TypeOf<PlayerCard>());
        
        Assert.That(player2Data.CardsInHand.Count, Is.EqualTo(3));
        Assert.That(player2Data.Field1, Is.Null);
    
        Assert.That(player1EnemyData.CardsInHand.Count, Is.EqualTo(3));
        Assert.That(player1EnemyData.Field1, Is.Null);
        
        Assert.That(player2EnemyData.CardsInHand.Count, Is.EqualTo(2));
        Assert.That(player2EnemyData.Field1, Is.TypeOf<PlayerCard>());
    }
    
    [Test, Order(11)]
    [Timeout(5000)]
    public async Task Player1EndTurn()
    {
        var json = JsonConvert.SerializeObject(new GameDto.EndTurnRequest(player1Id));
        var body = new StringContent(json, Encoding.UTF8, "application/json");
    
        var response = await _client.PostAsync("http://localhost:5157/game/turn-ended", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.EndTurnResponse>(responseBody);
    
        Assert.IsTrue(responseObject.Success);
    }
    
    [Test, Order(12)]
    [Timeout(5000)]
    public async Task Player2EndTurn()
    {
        TaskCompletionSource<bool> socket1EventReceived = new TaskCompletionSource<bool>(false);
        TaskCompletionSource<bool> socket2EventReceived = new TaskCompletionSource<bool>(false);
        
        connection1.On("turn_ended",  () =>
        {
            socket1EventReceived.SetResult(true);
        });
    
        connection2.On("turn_ended", () =>
        {
            socket2EventReceived.SetResult(true);
        });
        
        
        var json = JsonConvert.SerializeObject(new GameDto.EndTurnRequest(player2Id));
        var body = new StringContent(json, Encoding.UTF8, "application/json");
    
        var response = await _client.PostAsync("http://localhost:5157/game/turn-ended", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.EndTurnResponse>(responseBody);
    
        Assert.IsTrue(responseObject.Success);
        await Task.WhenAll(socket1EventReceived.Task, socket2EventReceived.Task);
        Assert.IsTrue(socket1EventReceived.Task.Result);
        Assert.IsTrue(socket2EventReceived.Task.Result);
    }
    
    [Test, Order(13)]
    [Timeout(5000)]
    public async Task DisposeConnections()
    {
        await connection1.DisposeAsync();
        await Task.Delay(500);
        await connection2.DisposeAsync();
    }
}