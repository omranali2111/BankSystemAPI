using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {
        public static AppDbContext dbContext;

        public UserLoginController(AppDbContext DB)
        {
            dbContext = DB;
        }


        [HttpPost("user-login")]
        public IActionResult GenerateJwtToken(string email, string password)
        {
            // Validate user credentials
            var user = dbContext.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

           
            // Create claims for the user
            
            if (user != null)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"));

                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: "omran",
                    audience: "all",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(120),
                    signingCredentials: credentials
                );

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            else
            {
                return Unauthorized("Invalid credentials.");
            }
        }

    }
}
