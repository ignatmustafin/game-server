using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Models;

[Index(nameof(Email), IsUnique = true)]
public class Player
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    
    [JsonPropertyName("Name")]
    [Required]
    public string Name { get; set; }
    
    [JsonPropertyName("Email")]
    [Required]
    public string Email { get; set; }
    
    [JsonPropertyName("Password")]
    [Required]
    public string Password { get; set; }
}