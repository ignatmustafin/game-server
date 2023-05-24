using GameServer.Postgres;
using GameServer.Postgres.Models;
using Microsoft.AspNetCore.Mvc;
using System.Timers;
using Timer = GameServer.Services.Timer;


namespace GameServer.Controllers
{
    [Route("lobby")]
    public class LobbyController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly SocketServer.SocketServer _socketServer;

        public LobbyController(AppDbContext dbContext, SocketServer.SocketServer socketServer)
        {
            _dbContext = dbContext;
            _socketServer = socketServer;
        }

        [HttpGet("/lobby/create-lobby")]
        public IActionResult CreateLobby()
        {
            var lobby = new Lobby()
            {
                Id = Guid.NewGuid(),
                JoinedUsers = 1,
                LoadedUsers = 0,
                TimerLeft = 60
            };

            _dbContext.Lobby.Add(lobby);
            _dbContext.SaveChanges();

            return Json(lobby);
        }

        public class LobbyIdRequest
        {
            public Guid LobbyId { get; set; }
        }

        [HttpPost("/lobby/join-lobby")]
        public IActionResult JoinLobby([FromBody] Lobby request)
        {
            Console.WriteLine($"request: {request}");
            Guid lobbyId = request.Id;
            Console.WriteLine($"get lobbyId value: {lobbyId}");
            var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);
            Console.WriteLine($"lobby found: {lobby}");

            if (lobby != null)
            {
                lobby.JoinedUsers++;
                _dbContext.SaveChanges();
                if (lobby.JoinedUsers == 2)
                {
                    _socketServer.SendToAllClients("all_users_joined_lobby", new {message = "Lobby is full!"});
                }

                return Json(new {success = true});
            }

            return NotFound();
        }

        [HttpPost("/lobby/scene-loaded")]
        public IActionResult SceneLoaded([FromBody] LobbyIdRequest request)
        {
            Console.WriteLine($"request: {request}");
            Guid lobbyId = request.LobbyId;
            Console.WriteLine($"get lobbyId value: {lobbyId}");
            var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);
            Console.WriteLine($"lobby found: {lobby}");

            if (lobby != null)
            {
                lobby.LoadedUsers++;
                _dbContext.SaveChanges();
                if (lobby.LoadedUsers == 2)
                {
                    Timer.StartTimer(lobby.Id, 1000, OnTimerTick);
                }

                return Json(new {success = true});
            }

            return NotFound();
        }

        [HttpPost("/lobby/end-turn")]
        public IActionResult TurnEnded([FromBody] LobbyIdRequest request)
        {
            Guid lobbyId = request.LobbyId;
            var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);

            if (lobby != null)
            {
                lobby.EndedTurnUsers++;
                _dbContext.SaveChanges();
                if (lobby.EndedTurnUsers == 2)
                {
                    StartBattle(lobby.Id);
                }

                return Json(lobby);
            }

            return NotFound();
        }

        private void StartBattle(Guid lobbyId)
        {
            Timer.StopTimer(lobbyId);
            var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);
            _socketServer.SendToAllClients("start_battle", new {message = "START BATTLE"});
            lobby.EndedTurnUsers = 0;
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            var currentTimer = (System.Timers.Timer)sender;
            var timerData = Timer.GetTimers.Values.FirstOrDefault(td => td.Timer == currentTimer);
            var timerId = Timer.GetTimers.FirstOrDefault(x => x.Value == timerData).Key;
            TimeSpan ticks = DateTime.Now - timerData.StartTime;
            Console.WriteLine($"Timer with ID {timerId} has ticked {ticks.TotalSeconds} times");
            if (ticks.TotalSeconds >= 60)
            {
                StartBattle(timerId);
            }
            else
            {
                _socketServer.SendToAllClients("time_left", new {timer = 60 - Math.Round(ticks.TotalSeconds)});
            }
        }
    }
}