using BankSystemAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountOperationController : ControllerBase
    {
        public static AppDbContext dbContext;

        public AccountOperationController(AppDbContext DB)
        {
            dbContext = DB;
        }
        [HttpPost("add-account")]
        public void AddAccount(int userId, string accountHolderName, decimal currentBalance)
        {
            try
            {
                // Create a new Account object
                var newAccount = new Account
                {
                    AccountHolderName = accountHolderName,
                    Balance = currentBalance,
                    UserId = userId // Associate the account with the specified user
                };

                // Add the new account to the DbContext
                dbContext.Accounts.Add(newAccount);

                // Save changes to the database
                dbContext.SaveChanges();

                Console.WriteLine("Account added successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }
        [HttpGet("view-accounts/{userId}")]
        public void ViewAccountsForUser(int userId)
        {

            try
            {
                // Query the accounts for the user using Entity Framework Core
                var userAccounts = dbContext.Accounts
                    .Where(account => account.UserId == userId)
                    .ToList();

                if (userAccounts.Any())
                {

                    Console.WriteLine("Accounts for the current user:");


                    foreach (var account in userAccounts)
                    {

                        Console.WriteLine($"Account Number: {account.AccountNumber}");
                        Console.WriteLine($"Account Holder: {account.AccountHolderName}");
                        Console.WriteLine($"Current Balance: {account.Balance} OMR");

                    }


                }
                else
                {
                    Console.WriteLine("No accounts found for this user.");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);

            }
        }
        [HttpPut("withdraw")]
        public void Withdraw(int userId, int accountNumber, decimal withdrawalAmount)
        {

            try
            {
                // Check if the specified account belongs to the current user
                var account = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber && a.UserId == userId);

                if (account != null)
                {
                    decimal currentBalance = account.Balance;

                    if (currentBalance >= withdrawalAmount)
                    {
                        // Update the balance with the new amount after withdrawal
                        account.Balance -= withdrawalAmount;
                        dbContext.SaveChanges();

                        Console.WriteLine("Withdrawal successful!");
                        RecordTransaction("Withdrawal", withdrawalAmount, accountNumber, null);
                    }
                    else
                    {
                        Console.WriteLine("Insufficient funds in the selected account.");
                    }
                }
                else
                {
                    Console.WriteLine("The specified account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }
        [HttpPut("deposit")]
        public void Deposit(int userId, int accountNumber, decimal depositAmount)
        {

            try
            {
                // Check if the specified account belongs to the current user
                var account = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber && a.UserId == userId);

                if (account != null)
                {
                    // Update the balance with the new amount after deposit
                    account.Balance += depositAmount;
                    dbContext.SaveChanges();

                    Console.WriteLine("Deposit successful!");
                    RecordTransaction("Deposit", depositAmount, null, accountNumber);
                }
                else
                {
                    Console.WriteLine("The specified account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }



        }
        [HttpPut("transfer")]
        public void Transfer(int userId, int sourceAccountNumber, int targetAccountNumber, decimal transferAmount)
        {

            try
            {
                // Check if the source account belongs to the current user
                var sourceAccount = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == sourceAccountNumber && a.UserId == userId);

                if (sourceAccount != null)
                {
                    if (sourceAccount.Balance >= transferAmount)
                    {
                        // Update the source account balance with the new amount after transfer
                        sourceAccount.Balance -= transferAmount;

                        // Check if the target account belongs to the user
                        var targetAccount = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == targetAccountNumber);

                        if (targetAccount != null)
                        {
                            // Transfer to the target account
                            targetAccount.Balance += transferAmount;
                            dbContext.SaveChanges();

                            Console.WriteLine("Transfer successful!");
                            RecordTransaction("Transfer", transferAmount, sourceAccountNumber, targetAccountNumber);
                        }
                        else
                        {
                            Console.WriteLine("Target account not found.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Insufficient funds in the source account.");
                    }
                }
                else
                {
                    Console.WriteLine("The specified source account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }

        [HttpPost("RecordTransaction")]
        private void RecordTransaction(string transactionType, decimal amount, int? sourceAccountNumber, int? targetAccountNumber)
        {

            try
            {
                // Ensure that both sourceAccountNumber and targetAccountNumber are not null
                if (!sourceAccountNumber.HasValue && !targetAccountNumber.HasValue)
                {
                    Console.WriteLine("Both sourceAccountNumber and targetAccountNumber cannot be null.");
                    return;
                }

                // Create a new transaction
                int? newww = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == (sourceAccountNumber ?? targetAccountNumber))?.AccountNumber;
                var transaction = new Transaction
                {

                    Timestamp = DateTime.Now,
                    Type = transactionType,
                    Amount = amount,
                    SrcAccNO = sourceAccountNumber ?? newww, // Set to AccountNumber if sourceAccountNumber is null
                    TargetAccNO = targetAccountNumber ?? null // Set to null if targetAccountNumber is null
                };


                //If sourceAccountNumber is provided, set the AccountId accordingly
                if (sourceAccountNumber.HasValue || targetAccountNumber.HasValue)
                {
                    var account = dbContext.Accounts
                        .FirstOrDefault(a => a.AccountNumber == (sourceAccountNumber ?? targetAccountNumber));

                    if (account != null)
                    {
                        // The account exists, you can proceed to create the transaction.
                        transaction.account = account;

                        // Rest of your transaction recording logic
                    }
                    else
                    {
                        Console.WriteLine("Invalid source or target account number.");
                    }
                }

                dbContext.Transactions.Add(transaction);
                dbContext.SaveChanges();

                Console.WriteLine("Transaction recorded successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while recording the transaction: " + e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + e.InnerException.Message);
                    Console.WriteLine("Inner Exception Stack Trace: " + e.InnerException.StackTrace);
                }
            }
        }
        [HttpGet("transaction-history/{userId}/{period}")]
        public void ViewTransactionHistory(int userId, string period)
        {
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            DateTime startDate;


            switch (period.ToLower())
            {
                case "last transaction":
                    startDate = minSqlDate; // Set to minimum date
                    break;
                case "last day":
                    startDate = DateTime.Now.AddDays(-1);
                    break;
                case "last 5 days":
                    startDate = DateTime.Now.AddDays(-5);
                    break;
                case "last 1 month":
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case "last 2 months":
                    startDate = DateTime.Now.AddMonths(-2);
                    break;
                default:
                    Console.WriteLine("Invalid period. Showing all transactions.");
                    startDate = minSqlDate; // Set to minimum date
                    break;
            }

           
                try
                {
                    // Define the LINQ query to fetch transaction history for the user's accounts within the specified period
                    var transactions = dbContext.Transactions
                        .Where(t => (t.SrcAccNO.HasValue && dbContext.Accounts.Any(a => a.UserId == userId && a.AccountNumber == t.SrcAccNO.Value))
                                    || (t.TargetAccNO.HasValue && dbContext.Accounts.Any(a => a.UserId == userId && a.AccountNumber == t.TargetAccNO.Value))
                                    && t.Timestamp >= startDate)
                        .OrderByDescending(t => t.Timestamp)
                        .ToList();

                    if (transactions.Any())
                    {
                        Console.WriteLine($"Transaction History (Last {period}):");
                        foreach (var transaction in transactions)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Transaction ID: {transaction.TransId}");
                            Console.WriteLine($"Timestamp: {transaction.Timestamp}");
                            Console.WriteLine($"Type: {transaction.Type}");
                            Console.WriteLine($"Amount: {transaction.Amount} OMR");
                            Console.ResetColor();

                            if (transaction.SrcAccNO.HasValue)
                            {
                                Console.WriteLine($"Source Account: {transaction.SrcAccNO}");
                            }

                            if (transaction.TargetAccNO.HasValue)
                            {
                                Console.WriteLine($"Target Account: {transaction.TargetAccNO}");
                            }

                            Console.WriteLine("---------------------------");
                        }
                      
                    }
                    else
                    {
                        Console.WriteLine("No transaction history found.");
                  
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                
                }
            }
        [HttpDelete("delete-account")]
        public void DeleteUserAccount(int userId, int accountNumberToDelete, string email, string password)
        {

            try
            {
                var accountToDelete = dbContext.Accounts
                    .FirstOrDefault(a => a.AccountNumber == accountNumberToDelete && a.UserId == userId);

                if (accountToDelete != null)
                {
                    var userToDelete = dbContext.Users
                        .FirstOrDefault(u => u.UserId == userId && u.Email == email && u.Password == password);

                    if (userToDelete != null)
                    {
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            try
                            {
                                // Remove the account from the user's accounts
                                dbContext.Accounts.Remove(new Account { UserId = userId, AccountNumber = accountNumberToDelete });

                                // Save changes to commit the transaction
                                dbContext.SaveChanges();

                                transaction.Commit();

                                Console.WriteLine("Account deleted successfully.");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("An error occurred while deleting the account: " + e.Message);
                                transaction.Rollback();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid email or password. Deletion failed.");
                    }
                }
                else
                {
                    Console.WriteLine("The specified account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }


    }
}





            
            




    

