using Postgrest.Attributes;
using Postgrest.Models;
// ReSharper disable ExplicitCallerInfoArgument

namespace CookingWithSatan.scripts.dto;

[Table("leaderboard")]
public class Leaderboard : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Reference(typeof(User))]
    public User User { get; set; }
    
    [Column("viewers")]
    public int Viewers { get; set; }
    
    [Column("uptime")]
    public int Uptime { get; set; }
    
    [Column("subs")]
    public int Subs { get; set; }
}