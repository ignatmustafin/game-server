using System.Collections;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameServer.DTO.Game;
using GameServer.Models;
using GameServer.Postgres;
using GameServer.Services.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Services.Game;

public class GameService : IGameService
{
    private readonly AppDbContext _db;
    private readonly SocketServerHub _socketService;

    public class TestObject
    {
        public int GameId;
    }

    private static readonly object LoadGameLock = new object(); // Объект блокировки

    // public record DamageToPlayer(int Field, Player AttackingPlayer, PlayerCard AttackingCard, Player PlayerUnderAttack);

    public record CardAttack(int Field, Player AttackingPlayer, PlayerCard AttackingCard, Player PlayerUnderAttack,
        PlayerCard[]
            CardUnderAttack);

    public record CardIsDead(int Field, Player PlayerUnderAttack);

    public GameService(AppDbContext db, SocketServerHub socketServer)
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

        GameDto.CreateGameResponse response =
            new GameDto.CreateGameResponse(newGame.Entity.Link, newPlayer.Entity.Id, newGame.Entity.Id);
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
            await _socketService.SendToClientsInList(playerListIds, "all_users_joined_lobby",
                new TestObject() {GameId = game.Id});
        }

        GameDto.JoinGameResponse response = new GameDto.JoinGameResponse(newPlayer.Entity.Id, game.Id);
        return response;
    }

    public async Task<GameDto.IsLoadedResponse> LoadGame(GameDto.IsLoadedRequest isLoadedRequest)
    {
        var player = _db.Player.FirstOrDefault(p => p.Id == isLoadedRequest.PlayerId);

        if (player == null)
        {
            throw new Exception("Player not found");
        }

        lock (LoadGameLock)
        {
            player.IsLoaded = true;

            _db.SaveChanges();

            var game = _db.Game
                .Include(g => g.Players)
                .ThenInclude(p => p.User).Include(g => g.Players)
                .ThenInclude(p => p.Cards)
                .ThenInclude(pc => pc.Card)
                .FirstOrDefault(g => g.Id == isLoadedRequest.GameId);

            var allPlayersLoaded = CheckAllPlayersLoaded(game);

            var randomCards = GetRandomCards(game, 3);

            foreach (var c in randomCards)
            {
                PlayerCard playerCard = new PlayerCard
                {
                    CardId = c.Id,
                    PlayerId = player.Id,
                    Manacost = c.Manacost,
                    Hp = c.Hp,
                    Damage = c.Damage,
                    Name = c.Name,
                    Type = c.Type
                };
                _db.PlayerCard.Add(playerCard);
            }

            _db.SaveChanges(); // Сохранение изменений перед коммитом транзакции


            if (allPlayersLoaded)
            {
                SetGameData(game);
            }
        }

        return new GameDto.IsLoadedResponse();
    }

    private bool CheckAllPlayersLoaded(Models.Game game)
    {
        Console.WriteLine($"LOADED PLAYERS COUNT {game.Players.Count(p => p.IsLoaded)}");
        return game.Players.Count(p => p.IsLoaded) == 2;
    }

    public async Task<GameDto.CardThrownResponse> CardThrown(GameDto.CardThrownRequest cardThrownRequest)
    {
        Console.WriteLine("IN FUNC CARD THROWN");
        var player = await _db.Player
            .Include(p => p.Cards)
            .FirstOrDefaultAsync(p => p.Id == cardThrownRequest.PlayerId);

        if (player == null)
        {
            throw new Exception("Player not found");
        }

        Console.WriteLine(
            $"QWE TEST HERE CARD ID {cardThrownRequest.CardId} {cardThrownRequest.PlayerId} {cardThrownRequest.Field}");

        var playerCard = player.Cards.FirstOrDefault(pc => pc.Id == cardThrownRequest.CardId);

        Console.WriteLine($"NEXT LOG {player.Cards.Count}");

        if (playerCard == null)
        {
            throw new Exception("Card not found");
        }

        playerCard.CardIn = cardThrownRequest.Field;


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

        await SetGameData(game);

        return new GameDto.CardThrownResponse();
    }

    private async Task SetGameData(Models.Game game)
    {
        int playerIndex = 0;
        foreach (var currentPlayer in game.Players)
        {
            Console.WriteLine(
                $"Cards in hand count for player {currentPlayer.Id} -- {currentPlayer.Cards.Where(pc => pc.CardIn == CardIn.Hand && pc.IsDead == false).ToList().Count}");
            foreach (var pc in currentPlayer.Cards)
            {
                Console.WriteLine($"player card id: {pc} with type: {pc.CardIn}");
            }


            var playerData = new GameDto.PlayerData
            {
                Name = currentPlayer.User.Name,
                Hp = currentPlayer.Hp,
                Mana = currentPlayer.Mana,
                CardsInHand = currentPlayer.Cards.Where(pc => pc.CardIn == CardIn.Hand && pc.IsDead == false).ToList(),
                Field1 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1 && pc.IsDead == false),
                Field2 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2 && pc.IsDead == false),
                Field3 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3 && pc.IsDead == false),
                Field4 = currentPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4 && pc.IsDead == false)
            };

            var enemyPlayer = playerIndex == 0
                ? game.Players.ElementAt(playerIndex + 1)
                : game.Players.ElementAt(playerIndex - 1);
            var enemyData = new GameDto.EnemyData
            {
                Name = enemyPlayer.User.Name,
                Hp = enemyPlayer.Hp,
                Mana = enemyPlayer.Mana,
                CardsInHand = enemyPlayer.Cards.Where(pc => pc.CardIn == CardIn.Hand && pc.IsDead == false)
                    .Select(pc => new GameDto.EnemyCardType() {Type = pc.Type}).ToList(),
                Field1 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1 && pc.IsDead == false),
                Field2 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2 && pc.IsDead == false),
                Field3 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3 && pc.IsDead == false),
                Field4 = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4 && pc.IsDead == false)
            };

            var gameData = new GameDto.GameData(playerData, enemyData);

            await _socketService.SendToClient(currentPlayer.UserId, "update_game_data",
                gameData);
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
        var game = await AllPlayersEndedTurn(player.GameId);

        // if (game != null)
        // {
        //     await StartBattle(game);
        // }

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
            await _socketService.SendToClientsInList(playerListIds, "turn_ended");
            return game;
        }

        return null;
    }

    private async Task StartBattle(Models.Game game)
    {
        var player1Fields = new PlayerCard?[4];
        var player2Fields = new PlayerCard?[4];

        Console.WriteLine($"game: {game}");
        // Console.WriteLine(game.Players.Count);
        var player1 = game.Players.ElementAt(0);
        var p1Field1Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1);
        player1Fields[0] = p1Field1Card;

        var p1Field2Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2);
        player1Fields[1] = p1Field2Card;

        var p1Field3Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3);
        player1Fields[2] = p1Field3Card;

        var p1Field4Card = player1.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4);
        player1Fields[3] = p1Field4Card;


        var player2 = game.Players.ElementAt(1);
        var p2Field1Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field1);
        player2Fields[0] = p2Field1Card;

        var p2Field2Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field2);
        player2Fields[1] = p2Field2Card;

        var p2Field3Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field3);
        player2Fields[2] = p2Field3Card;

        var p2Field4Card = player2.Cards.FirstOrDefault(pc => pc.CardIn == CardIn.Field4);
        player2Fields[3] = p2Field4Card;

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
                switch (p1CurrentCard.Type)
                {
                    case CardType.Straight:
                    {
                        var fieldsToAttack = new PlayerCard[] {player2Fields[i] ?? null};

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player2.Hp -= p1CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p1CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player1, p1CurrentCard, player2, fieldsToAttack));

                        break;
                    }
                    case CardType.Left:
                    {
                        var fieldsToAttack =
                            i == 0
                                ? new PlayerCard[] {player2Fields[i] ?? null}
                                : new PlayerCard[] {player2Fields[i] ?? null, player2Fields[i - 1] ?? null};

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player2.Hp -= p1CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p1CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player1, p1CurrentCard, player2, fieldsToAttack));

                        break;
                    }
                    case CardType.Right:
                    {
                        var fieldsToAttack =
                            i == 3
                                ? new PlayerCard[] {player2Fields[i] ?? null}
                                : new PlayerCard[] {player2Fields[i] ?? null, player2Fields[i + 1] ?? null};

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player2.Hp -= p1CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p1CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player1, p1CurrentCard, player2, fieldsToAttack));

                        break;
                    }
                    case CardType.All:
                    {
                        var fieldsToAttack =
                            i == 0
                                ? new PlayerCard[] {player2Fields[i] ?? null, player2Fields[i + 1] ?? null}
                                : i == 3
                                    ? new PlayerCard[] {player2Fields[i] ?? null, player2Fields[i - 1] ?? null}
                                    : new PlayerCard[]
                                    {
                                        player2Fields[i] ?? null, player2Fields[i + 1] ?? null,
                                        player2Fields[i - 1] ?? null
                                    };

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player2.Hp -= p1CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p1CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player1, p1CurrentCard, player2, fieldsToAttack));

                        break;
                    }
                }
            }

            if (player2.Hp < 1)
            {
                await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(), "player_win",
                    player1.Id);
                game.IsFinished = true;
                break;
            }


            // socket send info

            if (p2CurrentCard != null)
            {
                switch (p2CurrentCard.Type)
                {
                    case CardType.Straight:
                    {
                        var fieldsToAttack = new PlayerCard[] {player1Fields[i]};

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player1.Hp -= p2CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p2CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player2, p2CurrentCard, player1, fieldsToAttack));

                        break;
                    }
                    case CardType.Left:
                    {
                        var fieldsToAttack =
                            i == 0
                                ? new PlayerCard[] {player1Fields[i] ?? null}
                                : new PlayerCard[] {player1Fields[i] ?? null, player1Fields[i - 1] ?? null};

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player1.Hp -= p2CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p2CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player2, p2CurrentCard, player1, fieldsToAttack));

                        break;
                    }
                    case CardType.Right:
                    {
                        var fieldsToAttack =
                            i == 3
                                ? new PlayerCard[] {player1Fields[i] ?? null}
                                : new PlayerCard[] {player1Fields[i] ?? null, player1Fields[i + 1] ?? null};

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player1.Hp -= p2CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p2CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player2, p2CurrentCard, player1, fieldsToAttack));

                        break;
                    }
                    case CardType.All:
                    {
                        var fieldsToAttack =
                            i == 0
                                ? new PlayerCard[] {player1Fields[i] ?? null, player1Fields[i + 1] ?? null}
                                : i == 3
                                    ? new PlayerCard[] {player1Fields[i] ?? null, player1Fields[i - 1] ?? null}
                                    : new PlayerCard[]
                                    {
                                        player1Fields[i] ?? null, player1Fields[i + 1] ?? null,
                                        player1Fields[i - 1] ?? null
                                    };

                        foreach (var field in fieldsToAttack)
                        {
                            if (field == null)
                            {
                                player1.Hp -= p2CurrentCard.Damage;
                            }
                            else
                            {
                                field.Hp -= p2CurrentCard.Damage;
                            }
                        }

                        await _db.SaveChangesAsync();
                        await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_attack",
                            new CardAttack(i, player2, p2CurrentCard, player1, fieldsToAttack));

                        break;
                    }
                }
            }

            for (var j = 0; j < 4; j++)
            {
                var player1Card = player1Fields[j];
                var player2Card = player2Fields[j];

                if (player1Card.Hp <= 0 && j <= i)
                {
                    Console.WriteLine("CARD IS DEAD EVENT P1");
                    await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                        "card_is_dead", new CardIsDead(j, player2));
                }

                if (player2Card.Hp <= 0 && j <= i)
                {
                    Console.WriteLine("CARD IS DEAD EVENT P2");
                    await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                        "card_is_dead", new CardIsDead(j, player1));
                }
            }

            if (player1.Hp < 1)
            {
                await _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(), "player_win",
                    player2.Id);
                game.IsFinished = true;
                break;
            }

            await SetGameData(game);
        }

        if (game.IsFinished == false)
        {
            foreach (var player in game.Players)
            {
                player.TurnEnded = false;

                var randomCards = GetRandomCards(game, 1);

                foreach (var c in randomCards)
                {
                    var playerCard = new PlayerCard
                    {
                        CardId = c.Id,
                        PlayerId = player.Id,
                        Manacost = c.Manacost,
                        Hp = c.Hp,
                        Damage = c.Damage,
                        Name = c.Name,
                        Type = c.Type
                    };
                    await _db.PlayerCard.AddAsync(playerCard);
                }
            }

            await _db.SaveChangesAsync();
            // _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(), "turn_start");
            SetGameData(game);
        }
        else
        {
            await _db.SaveChangesAsync();
        }
    }

    private IEnumerable<Card> GetRandomCards(Models.Game game, int cardsCount)
    {
        var excludedCardIds = game.Players.SelectMany(p => p.Cards.Select(pc => pc.CardId)).ToArray();
        var filteredCards = _db.Card.Where(c => !excludedCardIds.Contains(c.Id)).ToList();

        Console.WriteLine($"Excluded CARDS ARRAY: {excludedCardIds.Length}");
        foreach (var test in excludedCardIds)
        {
            Console.WriteLine($"Excluded CARD Id: {test}");
        }


        Console.WriteLine(filteredCards.Count);

        foreach (var test in filteredCards)
        {
            Console.WriteLine($"Included CARD Id: {test.Id}");
        }

        return filteredCards
            .OrderBy(c => Guid.NewGuid())
            .Take(cardsCount);
    }
}