using GameServer.DTO.Game;
using GameServer.Models;
using GameServer.Postgres;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Services.Game;

public class PlayerData
{
    public string Name { get; set; }
    public int Hp { get; set; }
    public int Mana { get; set; }
    public ICollection<Card> CardsInHand { get; set; }
}

public class EnemyData
{
    public string Name { get; set; }
    public int Hp { get; set; }
    public int Mana { get; set; }
    public int CardsInHandCount { get; set; }
}

public class GameService : IGameService
{
    private readonly AppDbContext _db;
    private readonly SocketServer.SocketServer _socketService;

    private record GameData(PlayerData PlayerData, EnemyData EnemyData);

    public GameService(AppDbContext db, SocketServer.SocketServer socketServer)
    {
        _db = db;
        _socketService = socketServer;
    }

    public async Task<GameDto.CreateGameResponse> CreateLobby(GameDto.CreateGameRequest createGameRequest)
    {
        Models.Game game = new Models.Game()
        {
            Link = Guid.NewGuid()
        };

        var newGame = await _db.Game.AddAsync(game);
        await _db.SaveChangesAsync();

        Player player = new Player()
        {
            GameId = newGame.Entity.Id,
            UserId = createGameRequest.UserId,
        };

        var newPlayer = await _db.Player.AddAsync(player);
        await _db.SaveChangesAsync();

        GameDto.CreateGameResponse response = new GameDto.CreateGameResponse(newGame.Entity.Link, newPlayer.Entity.Id);
        return response;
    }

    public async Task<GameDto.JoinGameResponse> JoinGame(GameDto.JoinGameRequest joinGameRequest)
    {
        var game = await _db.Game.Include(g => g.Players).FirstOrDefaultAsync(g => g.Link == joinGameRequest.Link);
        if (game == null || game.IsFinished || game.Players.Count >= 2)
        {
            throw new Exception("Game not found");
        }

        Player player = new Player()
        {
            GameId = game.Id,
            UserId = joinGameRequest.UserId
        };

        var newPlayer = await _db.AddAsync(player);
        await _db.SaveChangesAsync();

        int[] playerListIds = game.Players.Select(player => player.UserId).ToArray();

        if (game.Players.Count >= 2)
        {
            _socketService.SendToClientsInList(playerListIds, "all_users_joined_lobby");
        }

        GameDto.JoinGameResponse response = new GameDto.JoinGameResponse(newPlayer.Entity.Id);
        return response;
    }

    public async Task<GameDto.IsLoadedResponse> LoadGame(GameDto.IsLoadedRequest isLoadedRequest)
    {
        var player = await _db.Player.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == isLoadedRequest.PlayerId);
        if (player == null)
        {
            throw new Exception("Player not found");
        }

        player.IsLoaded = true;
        await _db.SaveChangesAsync();

        var game = await CheckAllPlayersLoaded(player.GameId);


        if (game != null)
        {
            for (var i = 0; i < game.Players.Count; i++)
            {
                var currentPlayer = game.Players.ElementAt(i);
                var playerData = new PlayerData
                {
                    Name = currentPlayer.User.Name,
                    Hp = currentPlayer.Hp,
                    Mana = currentPlayer.Mana,
                    CardsInHand = currentPlayer.CardsInHand.Select(pc => pc.Card).ToList()
                };

                var enemyPlayer = i == 0 ? game.Players.ElementAt(i + 1) : game.Players.ElementAt(i - 1);
                var enemyData = new EnemyData
                {
                    Name = enemyPlayer.User.Name,
                    Hp = enemyPlayer.Hp,
                    Mana = enemyPlayer.Mana,
                    CardsInHandCount = currentPlayer.CardsInHand.Count
                };

                Console.WriteLine(
                    $"Player data for {i}: {playerData.Name}_{playerData.Hp}, Enemy data -- {enemyData.Name}_{enemyData.Hp} ");
                
                _socketService.SendToClient(currentPlayer.UserId, "update_game_data", new GameData(playerData, enemyData));
            }
        }

        return new GameDto.IsLoadedResponse();
    }

    public async Task<Models.Game> CheckAllPlayersLoaded(int gameId)
    {
        var game = await _db.Game
            .Include(g => g.Players)
                .ThenInclude(p => p.User).
            Include(g => g.Players)
                .ThenInclude(p => p.CardsInHand)
                    .ThenInclude(pc => pc.Card)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game != null && game.Players.Count == 2)
        {
            return game;
        }

        return null;
    }
}