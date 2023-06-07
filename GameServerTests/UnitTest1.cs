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


namespace TestProject1;

public class Tests
{
    private record GameData(PlayerData PlayerData, EnemyData EnemyData);

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private SocketIOClient _socket1;
    private SocketIOClient _socket2;
    private HttpClient _client = new();

    private Guid link;
    private int player1Id;
    private int player2Id;

    private GameData player1GameData;
    private GameData player2GameData;

    private PlayerData player1Data;
    private EnemyData player1EnemyData;
    private PlayerData player2Data;
    private EnemyData player2EnemyData;

    [SetUp]
    public void Setup()
    {
    }

    [Test, Order(1)]
    public void Test1()
    {
        _socket1 = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, "127.0.0.1", 3000));
        _socket1.Connect();

        var connectEventReceived = new AutoResetEvent(false);
        
        _socket1.On("socket_id_saved", () =>
        {
            Console.WriteLine($"Socket 1 user id saved");
            connectEventReceived.Set();
        });

        _socket1.On("connect", () =>
        {
            _socket1.Emit("set_user_id", 1);
        });
        
        Assert.True(connectEventReceived.WaitOne(TimeSpan.FromSeconds(5)));
    }

    [Test, Order(2)]
    public void Test2()
    {
        _socket2 = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, "127.0.0.1", 3000));
        _socket2.Connect();

        var connectEventReceived = new AutoResetEvent(false);
        
        _socket2.On("socket_id_saved", () =>
        {
            Console.WriteLine($"Socket 3 user id saved");
            connectEventReceived.Set();
        });

        _socket2.On("connect", () =>
        {
            _socket2.Emit("set_user_id", 3);
        });
        
        Assert.True(connectEventReceived.WaitOne(TimeSpan.FromSeconds(5)));
    }

    [Test, Order(3)]
    public async Task CreateGame()
    {
        var json = JsonConvert.SerializeObject(new GameDto.CreateGameRequest(1));
        var body = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("http://localhost:5157/game/create-game", body);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GameDto.CreateGameResponse>(responseBody);


        Console.WriteLine(responseObject.Link);
        Assert.IsNotNull(responseObject);
        link = responseObject.Link;
        player1Id = responseObject.PlayerId;
    }
    
    [Test, Order(4)]
    [Timeout(5000)]
    public async Task JoinGame()
    {
        TaskCompletionSource<bool> socket1EventReceived = new TaskCompletionSource<bool>(false);
        TaskCompletionSource<bool> socket2EventReceived = new TaskCompletionSource<bool>(false);

        // Console.WriteLine(socket1EventReceived.Task.Result);
        _socket1.On("all_users_joined_lobby", () =>
        {
            socket1EventReceived.SetResult(true);
        });

        _socket2.On("all_users_joined_lobby", () =>
        {
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
    
    [Test, Order(5)]
    public async Task LoadPlayer1()
    {
        var json = JsonConvert.SerializeObject(new GameDto.IsLoadedRequest(player1Id));
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
        
        _socket1.On("update_game_data",  (JToken[] data) =>
        {
            player1GameData = data[0].ToObject<GameData>();
            socket1EventReceived.SetResult(true);
        });

        _socket2.On("update_game_data", (JToken[] data) =>
        {
            player2GameData = data[0].ToObject<GameData>();
            socket2EventReceived.SetResult(true);
        });
        
        
        var json = JsonConvert.SerializeObject(new GameDto.IsLoadedRequest(player2Id));
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
        Assert.That(player1Data.CardsInHand, Is.EqualTo(new List<Card>()));
        
        Assert.That(player1EnemyData.Name, Is.EqualTo("Sasha"));
        Assert.That(player1EnemyData.Hp, Is.EqualTo(30));
        Assert.That(player1EnemyData.Mana, Is.EqualTo(1));
        Assert.That(player1EnemyData.CardsInHandCount, Is.EqualTo(0));
    }
    
    [Test, Order(8)]
    public void CheckPlayer2GameData()
    {
        player2Data = player2GameData.PlayerData;
        player2EnemyData = player2GameData.EnemyData;
        Assert.That(player2Data.Name, Is.EqualTo("Sasha"));
        Assert.That(player2Data.Hp, Is.EqualTo(30));
        Assert.That(player2Data.Mana, Is.EqualTo(1));
        Assert.That(player2Data.CardsInHand, Is.EqualTo(new List<Card>()));
        
        Assert.That(player2EnemyData.Name, Is.EqualTo("Ignat"));
        Assert.That(player2EnemyData.Hp, Is.EqualTo(30));
        Assert.That(player2EnemyData.Mana, Is.EqualTo(1));
        Assert.That(player2EnemyData.CardsInHandCount, Is.EqualTo(0));
    }

}