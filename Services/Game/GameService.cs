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
    private CardIn[] _fieldsList = {CardIn.Field1, CardIn.Field2, CardIn.Field3, CardIn.Field4};


    public class TestObject
    {
        public int GameId;
    }

    private static readonly object LoadGameLock = new object(); // Объект блокировки
    private static readonly object EndTurnLock = new object(); // Объект блокировки

    // public record DamageToPlayer(int Field, Player AttackingPlayer, PlayerCard AttackingCard, Player PlayerUnderAttack);

    public record CardAttack(int Field, Player AttackingPlayer, PlayerCard AttackingCard, Player PlayerUnderAttack,
        PlayerCard[]
            CardUnderAttack);

    public record CardIsDead(CardIn Field, Player PlayerUnderAttack);

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
                Field1 = GetEnemyField(CardIn.Field1, enemyPlayer),
                Field2 = GetEnemyField(CardIn.Field2, enemyPlayer),
                Field3 = GetEnemyField(CardIn.Field3, enemyPlayer),
                Field4 = GetEnemyField(CardIn.Field4, enemyPlayer)
            };

            var gameData = new GameDto.GameData(playerData, enemyData);

            await _socketService.SendToClient(currentPlayer.UserId, "update_game_data",
                gameData);
            playerIndex++;
        }
    }

    public async Task<GameDto.EndTurnResponse> EndTurn(GameDto.EndTurnRequest endTurnRequest)
    {
        var player = await _db.Player.FirstOrDefaultAsync(p => p.Id == endTurnRequest.PlayerId);
        if (player == null)
        {
            throw new Exception("Player not found");
        }

        lock (EndTurnLock)
        {
            player.TurnEnded = true;

            _db.SaveChanges();

            var game = _db.Game
                .Include(g => g.Players)
                .ThenInclude(p => p.User).Include(g => g.Players)
                .ThenInclude(p => p.Cards)
                .ThenInclude(pc => pc.Card)
                .FirstOrDefault(g => g.Id == player.GameId);

            if (game == null)
            {
                throw new Exception("Game not found");
            }

            var allPlayersEndedTurn = AllPlayersEndedTurn(game);

            if (allPlayersEndedTurn)
            {
                foreach (var p in game.Players)
                {
                    foreach (var pc in p.Cards)
                    {
                        pc.SideState = SideState.Front;
                    }
                }

                _db.SaveChanges();
                SetGameData(game);

                StartBattle(game);
            }
        }

        return new GameDto.EndTurnResponse();
    }

    private bool AllPlayersEndedTurn(Models.Game game)
    {
        Console.WriteLine($"ENDED TURN PLAYERS COUNT {game.Players.Count(p => p.TurnEnded)}");
        return game.Players.Count(p => p.TurnEnded) == 2;
    }

    private async Task StartBattle(Models.Game game)
    {
        var player1 = game.Players.ElementAt(0);
        var player2 = game.Players.ElementAt(1);

        
        foreach (var field in _fieldsList)
        {
            Console.WriteLine(field);
            var player1Card = player1.Cards.FirstOrDefault(pc => !pc.IsDead && pc.CardIn == field);
            var player2Card = player2.Cards.FirstOrDefault(pc => !pc.IsDead && pc.CardIn == field);
            Console.WriteLine($"player 1 card: {player1Card}, player 2 card: {player2Card}, Player 1 id {player1.Id}");

            if (player1Card != null)
            {

                var fieldsToAttack = GetFieldsToAttack(field, player1Card, player2);
                Console.WriteLine(fieldsToAttack.Count);
                foreach (var pc in fieldsToAttack)
                {
                    Console.WriteLine($"FIELDS: {pc}");
                    if (pc == null)
                    {
                        player2.Hp -= player1Card.Damage;
                    }
                    else
                    {
                        pc.Hp -= player1Card.Damage;
                    }
                }
                
                if (player2.Hp < 1)
                {
                    _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(), "player_win",
                        player1.Id);
                    game.IsFinished = true;
                    break;
                }
            }
            
            
            if (player2Card != null)
            {
                var fieldsToAttack = GetFieldsToAttack(field, player2Card, player1);
                Console.WriteLine(fieldsToAttack.Count);
                foreach (var pc in fieldsToAttack)
                {
                    Console.WriteLine($"FIELDS P2: {pc}");
                    if (pc == null)
                    {
                        player1.Hp -= player2Card.Damage;
                    }
                    else
                    {
                        pc.Hp -= player2Card.Damage;
                    }
                }
                
                
                if (player1.Hp < 1)
                {
                    _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(), "player_win",
                        player2.Id);
                    game.IsFinished = true;
                    break;
                }
            }

            foreach (var f in _fieldsList)
            {
                var p1Card = player1.Cards.FirstOrDefault(pc => !pc.IsDead && pc.CardIn == f);
                Console.WriteLine($"CARD P1 IS GOING DEAD {p1Card != null && p1Card.Hp <= 0 && Array.IndexOf(_fieldsList, f) <= Array.IndexOf(_fieldsList, field)}");
                if (p1Card != null && p1Card.Hp <= 0 && Array.IndexOf(_fieldsList, f) <= Array.IndexOf(_fieldsList, field))
                    {
                        Console.WriteLine("CARD IS DEAD EVENT P1");
                        p1Card.IsDead = true;
                        _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                            "card_is_dead", new CardIsDead(f, player2));
                    }
                
                var p2Card = player2.Cards.FirstOrDefault(pc => !pc.IsDead && pc.CardIn == f);
                Console.WriteLine($"CARD P1 IS GOING DEAD {p2Card != null && p2Card.Hp <= 0 && Array.IndexOf(_fieldsList, f) <= Array.IndexOf(_fieldsList, field)}");

                if (p2Card != null && p2Card.Hp <= 0 && Array.IndexOf(_fieldsList, f) <= Array.IndexOf(_fieldsList, field))
                {
                    Console.WriteLine("CARD IS DEAD EVENT P2");
                    p2Card.IsDead = true;
                    _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(),
                        "card_is_dead", new CardIsDead(f, player1));
                }
            }

            _db.SaveChanges();
            SetGameData(game);

        }
        
        Console.WriteLine("AFTER CYCLE");

        

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
                    _db.PlayerCard.Add(playerCard);
                }
            }
        
            _db.SaveChanges();
            // _socketService.SendToClientsInList(game.Players.Select(p => p.UserId).ToArray(), "turn_start");
            SetGameData(game);
        }
        else
        {
            _db.SaveChanges();
        }
    }

    private ICollection<PlayerCard> GetFieldsToAttack(CardIn field, PlayerCard card, Player enemyPlayer)
    {
        switch (card.Type)
        {
            case CardType.Straight:
            {
                return new List<PlayerCard>() {enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field)};
            }

            case CardType.Left:
            {
                if (field == CardIn.Field1)
                {
                    return new List<PlayerCard>() {enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field)};
                }
                else
                {
                    return new List<PlayerCard>()
                    {
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field), 
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == _fieldsList[Array.IndexOf(_fieldsList, field) - 1])
                    };
                }
            }
            case CardType.Right:
            {
                if (field == CardIn.Field4)
                {
                    return new List<PlayerCard>() {enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field)};
                }
                else
                {
                    return new List<PlayerCard>()
                    {
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field), 
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == _fieldsList[Array.IndexOf(_fieldsList, field) + 1])
                    };
                }
            }
            case CardType.All:
            {
                if (field == CardIn.Field1)
                {
                    return new List<PlayerCard>()
                    {
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field), 
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == _fieldsList[Array.IndexOf(_fieldsList, field) + 1])
                    };
                }
                else if (field == CardIn.Field4)
                {
                    return new List<PlayerCard>()
                    {
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field), 
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == _fieldsList[Array.IndexOf(_fieldsList, field) - 1])
                    };
                }
                else
                {
                    return new List<PlayerCard>()
                    {
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field), 
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == _fieldsList[Array.IndexOf(_fieldsList, field) - 1]),
                        enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == _fieldsList[Array.IndexOf(_fieldsList, field) + 1])
                    };
                }
            }
            default:
                return null;
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

    private GameDto.CardBase GetEnemyField(CardIn field, Player enemyPlayer)
    {
        var enemyCard = enemyPlayer.Cards.FirstOrDefault(pc => pc.CardIn == field && !pc.IsDead);

        if (enemyCard == null)
        {
            return null;
        }

        if (enemyCard.SideState == SideState.Back)
        {
            return new GameDto.EnemyCardType() {Type = enemyCard.Type};
        }

        return enemyCard;
    }
}