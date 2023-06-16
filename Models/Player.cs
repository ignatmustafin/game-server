using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GameServer.Models;

public class Player
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsLoaded { get; set; }
    public bool TurnEnded { get; set; }
    public int Hp { get; set; } = 30;
    public int Mana { get; set; } = 1;
    public int? Field1CardId { get; set; }

    public int? Field2CardId { get; set; }

    public int? Field3CardId { get; set; }

    public int? Field4CardId { get; set; }
    public ICollection<PlayerCard> Cards { get; } = new List<PlayerCard>();
}