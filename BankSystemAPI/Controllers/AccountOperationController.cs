using BankSystemAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public IActionResult AddAccount( string accountHolderName, decimal currentBalance)
        {
            try
            {
                var newAccount = new Account
                {
                    AccountHolderName = accountHolderName,
                    Balance = currentBalance,
               
                };

                dbContext.Accounts.Add(newAccount);
                dbContext.SaveChanges();

                return Ok("Account added successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred while adding the account: " + e.Message);
            }
        }
        [Authorize]
        [HttpGet("view-accounts")]
        public ActionResult<List<Account>> ViewAccountsForUser()
        {
            try
            {
                // Get UserId from the claims
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    // UserId claim not found
                    return BadRequest("UserId claim not found.");
                }

                int userId = int.Parse(userIdClaim.Value);

                var userAccounts = dbContext.Accounts
                    .Where(account => account.UserId == userId)
                    .ToList();

                if (userAccounts.Any())
                {
                    return Ok(userAccounts);
                }
                else
                {
                    return NotFound("No accounts found for this user.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred while fetching user accounts: " + e.Message);
            }
        }
        [Authorize]
        [HttpPut("withdraw")]
        public ActionResult Withdraw( int accountNumber, decimal withdrawalAmount)
        {
            try
            {
                // Get UserId from the claims
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    // UserId claim not found
                    return BadRequest("UserId claim not found.");
                }

                int userId = int.Parse(userIdClaim.Value);
                var account = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber && a.UserId == userId);

                if (account != null)
                {
                    decimal currentBalance = account.Balance;

                    if (currentBalance >= withdrawalAmount)
                    {
                        account.Balance -= withdrawalAmount;
                        dbContext.SaveChanges();

                        RecordTransaction("Withdrawal", withdrawalAmount, accountNumber, null);

                        return Ok("Withdrawal successful!");
                    }
                    else
                    {
                        return BadRequest("Insufficient funds in the selected account.");
                    }
                }
                else
                {
                    return NotFound("The specified account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred: " + e.Message);
            }
        }
        [Authorize]
        [HttpPut("deposit")]
        public ActionResult Deposit( int accountNumber, decimal depositAmount)
        {
            try
            {
                // Get UserId from the claims
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    // UserId claim not found
                    return BadRequest("UserId claim not found.");
                }

                int userId = int.Parse(userIdClaim.Value);
                var account = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber && a.UserId == userId);

                if (account != null)
                {
                    account.Balance += depositAmount;
                    dbContext.SaveChanges();

                    RecordTransaction("Deposit", depositAmount, null, accountNumber);

                    return Ok("Deposit successful!");
                }
                else
                {
                    return NotFound("The specified account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred: " + e.Message);
            }
        }
        [Authorize]
        [HttpPut("transfer")]
        public ActionResult Transfer(int userId, int sourceAccountNumber, int targetAccountNumber, decimal transferAmount)
        {
            try
            {
                var sourceAccount = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == sourceAccountNumber && a.UserId == userId);

                if (sourceAccount != null)
                {
                    if (sourceAccount.Balance >= transferAmount)
                    {
                        sourceAccount.Balance -= transferAmount;

                        var targetAccount = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == targetAccountNumber);

                        if (targetAccount != null)
                        {
                            targetAccount.Balance += transferAmount;
                            dbContext.SaveChanges();

                            RecordTransaction("Transfer", transferAmount, sourceAccountNumber, targetAccountNumber);

                            return Ok("Transfer successful!");
                        }
                        else
                        {
                            return NotFound("Target account not found.");
                        }
                    }
                    else
                    {
                        return BadRequest("Insufficient funds in the source account.");
                    }
                }
                else
                {
                    return NotFound("The specified source account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred: " + e.Message);
            }
        }
        [Authorize]
        [HttpPost("RecordTransaction")]
        private ActionResult RecordTransaction(string transactionType, decimal amount, int? sourceAccountNumber, int? targetAccountNumber)
        {
            try
            {
                if (!sourceAccountNumber.HasValue && !targetAccountNumber.HasValue)
                {
                    return BadRequest("Both sourceAccountNumber and targetAccountNumber cannot be null.");
                }

                var newww = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == (sourceAccountNumber ?? targetAccountNumber))?.AccountNumber;
                var transaction = new Transaction
                {
                    Timestamp = DateTime.Now,
                    Type = transactionType,
                    Amount = amount,
                    SrcAccNO = sourceAccountNumber ?? newww,
                    TargetAccNO = targetAccountNumber ?? null
                };

                if (sourceAccountNumber.HasValue || targetAccountNumber.HasValue)
                {
                    var account = dbContext.Accounts.FirstOrDefault(a => a.AccountNumber == (sourceAccountNumber ?? targetAccountNumber));

                    if (account != null)
                    {
                        transaction.account = account;
                    }
                    else
                    {
                        return BadRequest("Invalid source or target account number.");
                    }
                }

                dbContext.Transactions.Add(transaction);
                dbContext.SaveChanges();

                return Ok("Transaction recorded successfully.");
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred while recording the transaction: " + e.Message);
            }
        }
        [Authorize]
        [HttpGet("transaction-history/{period}")]
        public ActionResult<List<Transaction>> ViewTransactionHistory( string period)
        {
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            DateTime startDate;

            switch (period.ToLower())
            {
                case "last transaction":
                    startDate = minSqlDate;
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
                    return BadRequest("Invalid period. Showing all transactions.");
            }

            try
            {
                // Get UserId from the claims
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    // UserId claim not found
                    return BadRequest("UserId claim not found.");
                }

                int userId = int.Parse(userIdClaim.Value);
                var transactions = dbContext.Transactions
                    .Where(t => (t.SrcAccNO.HasValue && dbContext.Accounts.Any(a => a.UserId == userId && a.AccountNumber == t.SrcAccNO.Value))
                                || (t.TargetAccNO.HasValue && dbContext.Accounts.Any(a => a.UserId == userId && a.AccountNumber == t.TargetAccNO.Value))
                                && t.Timestamp >= startDate)
                    .OrderByDescending(t => t.Timestamp)
                    .ToList();

                if (transactions.Any())
                {
                    return Ok(transactions);
                }
                else
                {
                    return NotFound("No transaction history found.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred: " + e.Message);
            }
        }
        [Authorize]
        [HttpDelete("delete-account")]
        public ActionResult DeleteUserAccount( int accountNumberToDelete, string email, string password)
        {
            try
            {
                // Get UserId from the claims
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    // UserId claim not found
                    return BadRequest("UserId claim not found.");
                }

                int userId = int.Parse(userIdClaim.Value);
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
                                dbContext.Accounts.Remove(new Account { UserId = userId, AccountNumber = accountNumberToDelete });
                                dbContext.SaveChanges();
                                transaction.Commit();

                                return Ok("Account deleted successfully.");
                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                return StatusCode(500, "An error occurred while deleting the account: " + e.Message);
                            }
                        }
                    }
                    else
                    {
                        return BadRequest("Invalid email or password. Deletion failed.");
                    }
                }
                else
                {
                    return NotFound("The specified account does not belong to you.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred: " + e.Message);
            }
        }

    }
}





            
            




    

