using System.ComponentModel.DataAnnotations;

namespace GameServer.Models;

public enum CardIn
{
    Hand,
    Field1,
    Field2,
    Field3,
    Field4,
}

public class PlayerCard
{
    public int Id { get; set; }
    [Required]
    public int PlayerId { get; set; }
    public Player Player { get; set; }
    [Required]
    public int CardId { get; set; }
    public Card Card { get; set; }
    [Required] 
    [EnumDataType(typeof(CardIn))]
    public CardIn CardIn { get; set; }

    public bool IsDead { get; set; }
    public int Manacost { get; set; }
    public int Hp { get; set; }
    public int Damage { get; set; }
    public string Name { get; set; }
    [EnumDataType(typeof(CardType))] public CardType Type { get; set; }
}