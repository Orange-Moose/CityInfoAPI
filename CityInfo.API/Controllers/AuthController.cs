using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CityInfo.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Inject configuration values from appsettings.Development.json
        private readonly IConfiguration _config;   
        
            public AuthController(IConfiguration config)
            {
                _config = config ?? throw new ArgumentNullException(nameof(config));
            }

        // construct req.body object
        // we won't use it outside of this class, so we scope it to this namespace
        public class AuthRequestBody
        {
            public AuthRequestBody(string username, string password)
            {
                Username = username;
                Password = password;
            }
            public string Username { get; set; }    
            public string Password { get; set; }
        }

        // construct user entity object
        // we won't use this class outside of AuthRequestBody class, so we define it inside
        private class CityInfoUser
        {
            public CityInfoUser(int userId, string userName, string firstName, string lastName, string city)
            {
                Id = userId;
                Username = userName;
                FirstName = firstName;
                LastName = lastName;
                City = city;
            }
            
            public int Id { get; set; }
            public string Username { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }
        }

        // create validation method for user credentials
        private static CityInfoUser ValidateUserCredentials(string username, string password)
        {
            // For demo purposed don't use DB for comparing credentials. Instead we create new user and assume the credentials are valid

            return new CityInfoUser(1, username ?? "", "John", "Doe", "Vilnius");
        }


        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthRequestBody credentials)
        {
            var (username, pwd) = (credentials.Username, credentials.Password);

            //1. Validate user credentials
            var user = ValidateUserCredentials(username, pwd);
            if (user == null)
                return Unauthorized(); // Creates 401 unauthorized response

            // 2. Create a token and sign credentials
            // On production, store keys in a secure environment, like Key Vault
            //In this case we store secrets in a appsetings.Development.json
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Authentication:SecretForKey"]));
            var signinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define claims. Claims define information abuout the user identity that is signed into security token
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.Id.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("city", user.City));

            // Define jwt token
            var jwtSecuriryToken = new JwtSecurityToken(
                _config["Authentication:Issuer"],
                _config["Authentication:Audience"],
                claimsForToken,
                DateTime.Now,
                DateTime.Now.AddHours(12),
                signinCredentials
                );

            // Create jwt token
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecuriryToken);

            return Ok(tokenToReturn); // return 200 OK status and token
        }

        


    }
}
