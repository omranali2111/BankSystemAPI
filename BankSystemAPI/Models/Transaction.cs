using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankSystemAPI.Models
{
    public class Transaction
    {
        [Key]
        public int TransId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public int? SrcAccNO { get; set; }
        public int? TargetAccNO { get; set; }

        // Foreign key property following naming convention
        [ForeignKey("account")]
        public int AccountId { get; set; }

        // Navigation property for the Account
        public Account account { get; set; }
    }
}
