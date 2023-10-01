using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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

        public User User { get; set; }

        // Navigation property to represent the relationship with the User entity
        [ForeignKey("User")]
        public int UserId { get; set; }

        public List<Transaction> transaction { get; set; }
    }
}
