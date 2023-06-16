using GameServer.DTO.Game;
using GameServer.Services.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GameServer.Endpoints;

public static class LobbyEndpoints
{
    public record ApiError(string Error);

    public static void ConfigureLobbyEndpoints(this WebApplication app)
    {
        app.MapPost("/game/create-game", CreateGame).WithName("create-game")
            .Accepts<GameDto.CreateGameRequest>("application/json")
            .Produces<GameDto.CreateGameResponse>(201).Produces<ApiError>(400);
        app.MapPost("/game/join-game", JoinGame).WithName("join-game")
            .Accepts<GameDto.JoinGameRequest>("application/json")
            .Produces<GameDto.JoinGameResponse>().Produces<ApiError>(400);
        app.MapPost("/game/loaded-game", LoadGame).WithName("loaded-game")
            .Accepts<GameDto.IsLoadedRequest>("application/json")
            .Produces<GameDto.IsLoadedResponse>().Produces<ApiError>(400); 
        app.MapPost("/game/card-thrown", ThrowCard).WithName("card-thrown")
            .Accepts<GameDto.CardThrownRequest>("application/json")
            .Produces<GameDto.CardThrownResponse>().Produces<ApiError>(400);
        app.MapPost("/game/turn-ended", EndTurn).WithName("turn-ended")
            .Accepts<GameDto.EndTurnRequest>("application/json")
            .Produces<GameDto.EndTurnResponse>().Produces<ApiError>(400);
    }

    private async static Task<IResult> CreateGame(IGameService gameService,
        [FromBody] GameDto.CreateGameRequest body)
    {
        try
        {
            GameDto.CreateGameResponse signUpResponse = await gameService.CreateLobby(body);
            return Results.Ok(signUpResponse);
        }
        catch (Exception e)
        {
            return Results.BadRequest(new ApiError(e.Message));
        }
    }

    private async static Task<IResult> JoinGame(IGameService gameService,
        [FromBody] GameDto.JoinGameRequest body)
    {
        try
        {
            GameDto.JoinGameResponse response = await gameService.JoinGame(body);
            return Results.Ok(response);
        }
        catch (Exception e)
        {
            return Results.BadRequest(new ApiError(e.Message));
        }
    }

    private async static Task<IResult> LoadGame(IGameService gameService, [FromBody] GameDto.IsLoadedRequest body)
    {
        try
        {
            GameDto.IsLoadedResponse response = await gameService.LoadGame(body);
            return Results.Ok(response);
        }
        catch (Exception e)
        {
            return Results.BadRequest(new ApiError(e.Message));
        }
    }

    private async static Task<IResult> ThrowCard(IGameService gameService, [FromBody] GameDto.CardThrownRequest body)
    {
        try
        {
            GameDto.CardThrownResponse response = await gameService.CardThrown(body);
            return Results.Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.BadRequest(new ApiError(e.Message));
        }
    }
    
    private async static Task<IResult> EndTurn(IGameService gameService, [FromBody] GameDto.EndTurnRequest body)
    {
        try
        {
            GameDto.EndTurnResponse response = await gameService.EndTurn(body);
            return Results.Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.BadRequest(new ApiError(e.Message));
        }
    }

    // [HttpGet]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // [Route("create-lobby")]
    // public IActionResult CreateLobby()
    // {
    //     var lobby = new Lobby()
    //     {
    //         Id = Guid.NewGuid(),
    //         JoinedUsers = 1,
    //         LoadedUsers = 0,
    //         TimerLeft = 60
    //     };
    //
    //     _dbContext.Lobby.Add(lobby);
    //     _dbContext.SaveChanges();
    //
    //     return Json(lobby);
    // }

    // public class LobbyIdRequest
    // {
    //     public Guid LobbyId { get; set; }
    // }
    //
    // [HttpPost("/lobby/join-lobby")]
    // public IActionResult JoinLobby([FromBody] Lobby request)
    // {
    //     Console.WriteLine($"request: {request}");
    //     Guid lobbyId = request.Id;
    //     Console.WriteLine($"get lobbyId value: {lobbyId}");
    //     var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);
    //     Console.WriteLine($"lobby found: {lobby}");
    //
    //     if (lobby != null)
    //     {
    //         lobby.JoinedUsers++;
    //         _dbContext.SaveChanges();
    //         if (lobby.JoinedUsers == 2)
    //         {
    //             _socketServer.SendToAllClients("all_users_joined_lobby", new {message = "Lobby is full!"});
    //         }
    //
    //         return Json(new {success = true});
    //     }
    //
    //     return NotFound();
    // }
    //
    // [HttpPost("/lobby/scene-loaded")]
    // public IActionResult SceneLoaded([FromBody] LobbyIdRequest request)
    // {
    //     Console.WriteLine($"request: {request}");
    //     Guid lobbyId = request.LobbyId;
    //     Console.WriteLine($"get lobbyId value: {lobbyId}");
    //     var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);
    //     Console.WriteLine($"lobby found: {lobby}");
    //
    //     if (lobby != null)
    //     {
    //         lobby.LoadedUsers++;
    //         _dbContext.SaveChanges();
    //         if (lobby.LoadedUsers == 2)
    //         {
    //             Timer.StartTimer(lobby.Id, 1000, OnTimerTick);
    //         }
    //
    //         return Json(new {success = true});
    //     }
    //
    //     return NotFound();
    // }
    //
    // [HttpPost("/lobby/end-turn")]
    // public IActionResult TurnEnded([FromBody] LobbyIdRequest request)
    // {
    //     Guid lobbyId = request.LobbyId;
    //     var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);
    //
    //     if (lobby != null)
    //     {
    //         lobby.EndedTurnUsers++;
    //         _dbContext.SaveChanges();
    //         if (lobby.EndedTurnUsers == 2)
    //         {
    //             StartBattle(lobby.Id);
    //         }
    //
    //         return Json(lobby);
    //     }
    //
    //     return NotFound();
    // }
    //
    // private void StartBattle(Guid lobbyId)
    // {
    //     Timer.StopTimer(lobbyId);
    //     var lobby = _dbContext.Lobby.FirstOrDefault(l => l.Id == lobbyId);
    //     _socketServer.SendToAllClients("start_battle", new {message = "START BATTLE"});
    //     lobby.EndedTurnUsers = 0;
    // }
    //
    // private void OnTimerTick(object sender, ElapsedEventArgs e)
    // {
    //     var currentTimer = (System.Timers.Timer) sender;
    //     var timerData = Timer.GetTimers.Values.FirstOrDefault(td => td.Timer == currentTimer);
    //     var timerId = Timer.GetTimers.FirstOrDefault(x => x.Value == timerData).Key;
    //     TimeSpan ticks = DateTime.Now - timerData.StartTime;
    //     Console.WriteLine($"Timer with ID {timerId} has ticked {ticks.TotalSeconds} times");
    //     if (ticks.TotalSeconds >= 60)
    //     {
    //         StartBattle(timerId);
    //     }
    //     else
    //     {
    //         _socketServer.SendToAllClients("time_left", new {timer = 60 - Math.Round(ticks.TotalSeconds)});
    //     }
    // }
}