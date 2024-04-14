using Postgrest.Attributes;
using Postgrest.Models;
// ReSharper disable ExplicitCallerInfoArgument

namespace CookingWithSatan.scripts.dto;

[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; }
}