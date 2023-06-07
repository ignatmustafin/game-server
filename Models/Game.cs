using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Models;

[Index(nameof(Link), IsUnique = true)]
public class Game
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    
    [JsonPropertyName("Link")]
    [Required]
    public Guid Link { get; set; }
    
    [JsonPropertyName("Players")]
    public ICollection<Player> Players { get; } = new List<Player>();

    [JsonPropertyName("IsFinished")] public bool IsFinished { get; set; } = false;
}