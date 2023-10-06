using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankSystemAPI.Models
{
    public class Account
    {
        [Key]
        public int AccountNumber { get; set; }

        [Required]
        [MaxLength(255)]
        public string AccountHolderName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        // Foreign Key property
        [JsonIgnore]
        public User User { get; set; }

        // Navigation property to represent the relationship with the User entity
        [ForeignKey("User")]
        public int UserId { get; set; }
        [JsonIgnore]
        public List<Transaction> transaction { get; set; }
    }
}
