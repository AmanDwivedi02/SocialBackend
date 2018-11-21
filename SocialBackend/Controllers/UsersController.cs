using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialBackend.Data;
using SocialBackend.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SocialBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SocialBackendContext _context;
        private readonly ICookieService _cookieService;

        public UsersController(SocialBackendContext context, ICookieService cookieService)
        {
            _context = context;
            _cookieService = cookieService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> checkLoggedIn()
        {
            if (!string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                if (await checkAuthorisation(_cookieService.getCookieValue(HttpContext)))
                {
                    return Ok();
                }
                else
                {
                    return Unauthorized();
                }
            }
            return Unauthorized();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            if (string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            }
            else if (!await checkAuthorisation(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            }
            else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] User user)
        {
            if (_cookieService.getCookieValue(HttpContext) == "")
            {
                return Unauthorized();
            }
            else if (!await checkAuthorisation(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            }
            else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.id)
            {
                return BadRequest();
            }
            else if (user.emailAddress == null && user.emailAddress == "")
            {
                return BadRequest();
            }
            else if (string.IsNullOrEmpty(user.username) || string.IsNullOrWhiteSpace(user.username))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(user.password) || string.IsNullOrWhiteSpace(user.password))
            {
                User localUser = await _context.User.AsNoTracking().Where(u => u.id == user.id).FirstOrDefaultAsync();
                user.password = localUser.password;
            }
            else
            {
                user.password = saltedHashedPassword(user.password);
            }

            user.authToken = (await _context.User.AsNoTracking().Where(u => u.id == user.id).FirstOrDefaultAsync()).authToken;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else if (user.emailAddress == null || user.emailAddress == "" || user.username == null || user.username == "" || user.password == null || user.password == "")
            {
                return BadRequest();
            }

            user.authToken = Guid.NewGuid().ToString();

            user.password = saltedHashedPassword(user.password);

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            _cookieService.setCookie(HttpContext, user.authToken);

            return Created("GetUser", new { id = user.id });
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // var authToken = await _context.User.Where(u => u.username == user.username && u.password == user.password).Select(u => u.authToken).FirstOrDefaultAsync();
            var localUser = await _context.User.Where(u => u.username == user.username).FirstOrDefaultAsync();

            if (localUser == null)
            {
                return NotFound();
            }

            if (!passCheck(user.password, localUser.password))
            {
                return NotFound();
            }

            _cookieService.setCookie(HttpContext, localUser.authToken);

            return Ok();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            }
            else if (!await checkAuthorisation(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            }

            else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.authToken != _cookieService.getCookieValue(HttpContext))
            {
                return Unauthorized();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.id == id);
        }

        private string saltedHashedPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;
        }

        private bool passCheck(string password, string savedPasswordHash)
        {
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }

        private async Task<bool> checkAuthorisation(string cookieValue)
        {
            return await _context.User.Where(u => u.authToken == cookieValue).AnyAsync();
        }

    }
}