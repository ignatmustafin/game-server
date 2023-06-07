using System.ComponentModel.DataAnnotations;

namespace GameServer.Models;

public class PlayerCard
{
    public int Id { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    public Player Player { get; set; }
    
    [Required]
    public int CardId { get; set; }
    public Card Card { get; set; }
}