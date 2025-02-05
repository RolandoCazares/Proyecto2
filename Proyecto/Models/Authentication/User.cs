using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proyecto.Models.Authentication
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
