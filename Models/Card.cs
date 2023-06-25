using System.ComponentModel.DataAnnotations;

namespace GameServer.Models;

public enum CardType
{
    Straight,
    Left,
    Right,
    All,
}

public class Card
{
    public int Id { get; set; }
    [Required] public string Name { get; set; }

    [Required]
    [EnumDataType(typeof(CardType))]
    public CardType Type { get; set; }

    [Required] public int Manacost { get; set; }
    public int Hp { get; set; }
    public int Damage { get; set; }
    public string ImageUrl { get; set; }
}