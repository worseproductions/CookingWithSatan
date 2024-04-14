using Postgrest.Attributes;
using Postgrest.Models;

namespace CookingWithSatan.scripts.dto;

[System.ComponentModel.DataAnnotations.Schema.Table("leaderboard")]
public class Leaderboard : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("score")]
    public int Score { get; set; }
}