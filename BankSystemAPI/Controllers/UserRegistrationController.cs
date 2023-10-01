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

        public void RegisterUser(string name, string email, string password)
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

                    Console.WriteLine("Registration successful!");
                }
                else
                {
                    Console.WriteLine("Password is invalid.");
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
        public bool UserLogin(string email, string password)
        {
            
                var user = dbContext.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    Console.WriteLine("Login successful!");
                    return true; // User exists, login successful
                }
                else
                {
                    Console.WriteLine("Login failed. Check your email and password.");
                    return false; // Email or password is incorrect
                }
            
        }



    }

}

