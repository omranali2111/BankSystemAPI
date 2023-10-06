using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankSystemAPI.Models
{
    public class User
    {

        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }
        [JsonIgnore]
        public List<Account> Accounts { get; set; }
    }
}
