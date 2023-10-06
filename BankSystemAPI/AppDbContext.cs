using BankSystemAPI.Models;
using Microsoft.EntityFrameworkCore;



namespace BankSystemAPI
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> option)
            : base(option)
        { }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseSqlServer("Data Source=(local);Initial Catalog=EFCoreBankSystem1; Integrated Security=true; TrustServerCertificate=True");
        }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
