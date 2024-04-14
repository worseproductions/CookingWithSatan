using Postgrest.Attributes;
using Postgrest.Models;

namespace CookingWithSatan.scripts.dto;

[Table("livechat")]
public class Livechat : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("message")]
    public string Message { get; set; }
}