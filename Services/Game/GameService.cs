using System.Collections;
using System.Runtime.InteropServices.JavaScript;
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
    public Card Field1 { get; set; }
    public Card Field2 { get; set; }
    public Card Field3 { get; set; }
    public Card Field4 { get; set; }
}

public class EnemyData
{
    public string Name { get; set; }
    public int Hp { get; set; }
    public int Mana { get; set; }
    public int CardsInHandCount { get; set; }
    public Card Field1 { get; set; }
    public Card Field2 { get; set; }
    public Card Field3 { get; set; }
    public Card Field4 { get; set; }
}

public class GameService : IGameService
{
    private readonly AppDbContext _db;
    private readonly SocketServer.SocketServer _socketService;

    public record GameData(PlayerData PlayerData, EnemyData EnemyData);

    public GameService(AppDbContext db, SocketServer.SocketServer socketServer)
    {
        _db = db;
        _socketService = socketServer;
    }

    public async Task<GameDto.CreateGameResponse> CreateLobby(GameDto.CreateGameRequest createGameRequest)
    {
        Models.Game game = new Models.Game
        {
            Link = Guid.NewGuid()
        };

        var newGame = await _db.Game.AddAsync(game);
        await _db.SaveChangesAsync();

        Player player = new Player
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

        Player player = new Player
        {
            GameId = game.Id,
            UserId = joinGameRequest.UserId
        };

        var newPlayer = await _db.AddAsync(player);
        await _db.SaveChangesAsync();

        int[] playerListIds = game.Players.Select(p => p.UserId).ToArray();

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
        var randomCards = _db.Card.ToList()
            .OrderBy(c => Guid.NewGuid())
            .Take(3);

        foreach (var c in randomCards)
        {
            PlayerCard playerCard = new PlayerCard
            {
                CardId = c.Id,
                PlayerId = player.Id
            };
            await _db.PlayerCard.AddAsync(playerCard);
        }

        await _db.SaveChangesAsync();

        var game = await CheckAllPlayersLoaded(player.GameId);

        if (game != null)
        {
            SetGameData(game);
        }

        return new GameDto.IsLoadedResponse();
    }

    public async Task<Models.Game> CheckAllPlayersLoaded(int gameId)
    {
        var game = await _db.Game
            .Include(g => g.Players)
            .ThenInclude(p => p.User).Include(g => g.Players)
            .ThenInclude(p => p.Cards)
            .ThenInclude(pc => pc.Card)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game != null && game.Players.Count == 2)
        {
            return game;
        }

        return null;
    }

    public async Task<GameDto.CardThrownResponse> CardThrown(GameDto.CardThrownRequest cardThrownRequest)
    {
        Console.WriteLine("IN FUNC CARD THROWN");
        var player = await _db.Player
            .Include(p => p.Cards
                .Where(pc =>
                    pc.CardIn == CardIn.Hand && pc.PlayerId == cardThrownRequest.PlayerId &&
                    pc.CardId == cardThrownRequest.CardId))
            .FirstOrDefaultAsync(p => p.Id == cardThrownRequest.PlayerId);

        foreach (var playerCard in player.Cards)
        {
            player.Field1CardId = playerCard.CardId;
            playerCard.CardIn = cardThrownRequest.Field;
        }

        await _db.SaveChangesAsync();

        var game = await _db.Game
            .Include(g => g.Players)
            .ThenInclude(p => p.User)
            .Include(g => g.Players)
            .ThenInclude(p => p.Cards)
            .ThenInclude(pc => pc.Card)
            .FirstOrDefaultAsync(g => g.Id == player.GameId);

        if (game == null)
        {
            throw new Exception("game not found");
        }

        SetGameData(game);

        return new GameDto.CardThrownResponse();
    }

    public void SetGameData(Models.Game game)
    {
        int playerIndex = 0;
        foreach (var currentPlayer in game.Players)
        {
            Console.WriteLine(
                $"DATA: {currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1 && pc.DeletedAt == null)?.Card}");
            foreach (var pc in currentPlayer.Cards)
            {
                Console.WriteLine($"player card id: {pc} with type: {pc.CardIn}");
            }

            var playerData = new PlayerData
            {
                Name = currentPlayer.User.Name,
                Hp = currentPlayer.Hp,
                Mana = currentPlayer.Mana,
                CardsInHand = currentPlayer.Cards.Where(pc => pc.CardIn == CardIn.Hand && pc.DeletedAt == null)
                    .Select(pc => pc.Card).ToList(),
                Field1 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1 && pc.DeletedAt == null)
                    ?.Card,
                Field2 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2 && pc.DeletedAt == null)
                    ?.Card,
                Field3 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3 && pc.DeletedAt == null)
                    ?.Card,
                Field4 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4 && pc.DeletedAt == null)
                    ?.Card
            };

            var enemyPlayer = playerIndex == 0
                ? game.Players.ElementAt(playerIndex + 1)
                : game.Players.ElementAt(playerIndex - 1);
            var enemyData = new EnemyData
            {
                Name = enemyPlayer.User.Name,
                Hp = enemyPlayer.Hp,
                Mana = enemyPlayer.Mana,
                CardsInHandCount = enemyPlayer.Cards.Count(pc => pc.CardIn == CardIn.Hand && pc.DeletedAt == null),
                Field1 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1 && pc.DeletedAt == null)
                    ?.Card,
                Field2 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2 && pc.DeletedAt == null)
                    ?.Card,
                Field3 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3 && pc.DeletedAt == null)
                    ?.Card,
                Field4 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4 && pc.DeletedAt == null)
                    ?.Card
            };

            _socketService.SendToClient(currentPlayer.UserId, "update_game_data",
                new GameData(playerData, enemyData));
            playerIndex++;
        }
    }

    public async Task<GameDto.EndTurnResponse> EndTurn(GameDto.EndTurnRequest endTurnRequest)
    {
        Console.WriteLine(endTurnRequest.PlayerId);
        var player = await _db.Player.FirstOrDefaultAsync(p => p.Id == endTurnRequest.PlayerId);
        if (player == null)
        {
            throw new Exception("Player not found");
        }

        player.TurnEnded = true;
        await _db.SaveChangesAsync();
        Console.WriteLine(player.Id);
        var game = await AllPlayersEndedTurn(player.GameId);
        await StartBattle(game);

        return new GameDto.EndTurnResponse();
    }

    private async Task<Models.Game> AllPlayersEndedTurn(int gameId)
    {
        var game = await _db.Game
            .Include(g => g.Players.Where(p => p.TurnEnded == true))
            .ThenInclude(p => p.User).Include(g => g.Players)
            .ThenInclude(p => p.Cards)
            .ThenInclude(pc => pc.Card)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game != null && game.Players.Count == 2)
        {
            int[] playerListIds = game.Players.Select(p => p.UserId).ToArray();
            _socketService.SendToClientsInList(playerListIds, "turn_ended");
            return game;
        }

        return null;
    }

    private async Task StartBattle(Models.Game game)
    {
        var player1Fields = new Card?[4];
        var player2Fields = new Card?[4];

        Console.WriteLine(game.Players.Count);
        var player1 = game.Players.ElementAt(0);
        var p1field1Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1);
        player1Fields[0] = p1field1Card?.Card;

        var p1field2Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2);
        player1Fields[1] = p1field2Card?.Card;

        var p1field3Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3);
        player1Fields[2] = p1field3Card?.Card;

        var p1field4Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4);
        player1Fields[3] = p1field4Card?.Card;


        var player2 = game.Players.ElementAt(1);
        var p2field1Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1);
        player2Fields[0] = p2field1Card?.Card;

        var p2field2Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2);
        player2Fields[1] = p2field2Card?.Card;

        var p2field3Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3);
        player2Fields[2] = p2field3Card?.Card;

        var p2field4Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4);
        player2Fields[3] = p2field4Card?.Card;

        Console.WriteLine(
            $"player 1 fields: {player1Fields[0]}, {player1Fields[1]}, {player1Fields[2]}, {player1Fields[3]}");
        Console.WriteLine(
            $"player 2 fields: {player2Fields[0]}, {player2Fields[1]}, {player2Fields[2]}, {player2Fields[3]}");

        for (var i = 0; i < 4; i++)
        {
            var p1CurrentCard = player1Fields[i];
            var p2CurrentCard = player2Fields[i];

            if (p1CurrentCard != null)
            {
                if (p1CurrentCard.Type == CardType.Straight)
                {
                    var fieldsToAttack = player2Fields[i];

                    if (fieldsToAttack == null)
                    {
                        Console.WriteLine(player2.Hp - p1CurrentCard.Damage);
                        player2.Hp -= p1CurrentCard.Damage;
                        await _db.SaveChangesAsync();
                    }
                }

                if (p1CurrentCard.Type == CardType.Left)
                {
                    var fieldsToAttack =
                        i == 0
                            ? new Card[] {player2Fields[i] ?? null}
                            : new Card[] {player2Fields[i] ?? null, player2Fields[i - 1] ?? null};

                    foreach (var field in fieldsToAttack)
                    {
                        if (field == null)
                        {
                            player2.Hp -= p1CurrentCard.Damage;
                            await _db.SaveChangesAsync();
                        }
                    }
                }

                if (p1CurrentCard.Type == CardType.Right)
                {
                    var fieldsToAttack =
                        i == 3
                            ? new Card[] {player2Fields[i] ?? null}
                            : new Card[] {player2Fields[i] ?? null, player2Fields[i + 1] ?? null};

                    foreach (var field in fieldsToAttack)
                    {
                        if (field == null)
                        {
                            player2.Hp -= p1CurrentCard.Damage;
                            await _db.SaveChangesAsync();
                        }
                    }
                }

                if (p1CurrentCard.Type == CardType.All)
                {
                    var fieldsToAttack =
                        i == 0
                            ? new Card[] {player2Fields[i] ?? null, player2Fields[i + 1] ?? null}
                            : i == 3
                                ? new Card[] {player2Fields[i] ?? null, player2Fields[i - 1] ?? null}
                                : new Card[]
                                {
                                    player2Fields[i] ?? null, player2Fields[i + 1] ?? null, player2Fields[i - 1] ?? null
                                };

                    foreach (var field in fieldsToAttack)
                    {
                        if (field == null)
                        {
                            player2.Hp -= p1CurrentCard.Damage;
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            
            
            // socket send info

            if (p2CurrentCard != null)
            {
                if (p2CurrentCard.Type == CardType.Straight)
                {
                    var fieldsToAttack = player1Fields[i];

                    if (fieldsToAttack == null)
                    {
                        player1.Hp -= p2CurrentCard.Damage;
                        await _db.SaveChangesAsync();
                    }
                }

                if (p2CurrentCard.Type == CardType.Left)
                {
                    var fieldsToAttack =
                        i == 0
                            ? new Card[] {player1Fields[i] ?? null}
                            : new Card[] {player1Fields[i] ?? null, player1Fields[i - 1] ?? null};

                    foreach (var field in fieldsToAttack)
                    {
                        if (field == null)
                        {
                            player1.Hp -= p2CurrentCard.Damage;
                            await _db.SaveChangesAsync();
                        }
                    }
                }

                if (p2CurrentCard.Type == CardType.Right)
                {
                    var fieldsToAttack =
                        i == 3
                            ? new Card[] {player1Fields[i] ?? null}
                            : new Card[] {player1Fields[i] ?? null, player1Fields[i + 1] ?? null};

                    foreach (var field in fieldsToAttack)
                    {
                        if (field == null)
                        {
                            player1.Hp -= p2CurrentCard.Damage;
                            await _db.SaveChangesAsync();
                        }
                    }
                }

                if (p2CurrentCard.Type == CardType.All)
                {
                    var fieldsToAttack =
                        i == 0
                            ? new Card[] {player1Fields[i] ?? null, player1Fields[i + 1] ?? null}
                            : i == 3
                                ? new Card[] {player1Fields[i] ?? null, player1Fields[i - 1] ?? null}
                                : new Card[]
                                {
                                    player1Fields[i] ?? null, player1Fields[i + 1] ?? null, player1Fields[i - 1] ?? null
                                };

                    foreach (var field in fieldsToAttack)
                    {
                        if (field == null)
                        {
                            player1.Hp -= p2CurrentCard.Damage;
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }
}