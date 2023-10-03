using BankSystemAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistrationController : ControllerBase
    {
        public static AppDbContext dbContext;

        public UserRegistrationController(AppDbContext DB)
        {
            dbContext = DB;
        }
        [HttpPost]
        public ActionResult RegisterUser(string name, string email, string password)
        {
            try
            {
                if (IsPasswordValid(password))
                {
                    var newUser = new User
                    {
                        Name = name,
                        Email = email,
                        Password = password
                    };

                    dbContext.Users.Add(newUser);
                    dbContext.SaveChanges();
                    return Ok("Registration successful!"); // 200 OK
                }
                else
                {                 
                    return BadRequest("Password is invalid."); // 400 Bad Request
                }
            }
            catch (Exception ex)
            {             
                return StatusCode(500, $"Internal Server Error {ex.Message}"); // 500 Internal Server Error
            }
        }

       

        private bool IsPasswordValid(string password)
        {
            // Define the regular expression pattern
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";

            // Create a regex object
            Regex regex = new Regex(pattern);

            // Use regex.IsMatch to check if the password matches the pattern
            return regex.IsMatch(password);
        }

    }

}

