using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialBackend.Models;

namespace SocialBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SocialBackendContext _context;

        public UsersController(SocialBackendContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> checkLoggedIn()
        {
            if (Request.Cookies["cookie"] != null)
            {
                if (await checkAuthorisation(Request.Cookies["cookie"]))
                {
                    return Ok();
                } else
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
            if (Request.Cookies["cookie"] == null)
            {
                return Unauthorized();
            }
            else if (!await checkAuthorisation(Request.Cookies["cookie"]))
            {
                return Unauthorized();
            }
            else if(!ModelState.IsValid)
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
        public async Task<IActionResult> PutUser([FromRoute] int id, [FromBody] User user)
        {
            if (Request.Cookies["cookie"] == null)
            {
                return Unauthorized();
            }
            else if (!await checkAuthorisation(Request.Cookies["cookie"]))
            {
                return Unauthorized();
            }
            else if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.id)
            {
                return BadRequest();
            }

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
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            user.authToken = Guid.NewGuid().ToString();

            user.password = saltedHashedPassword(user.password);

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddHours(1);
            Response.Cookies.Append("cookie", user.authToken, options);

            return CreatedAtAction("GetUser", new { id = user.id });
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

            CookieOptions options = new CookieOptions();

            options.Expires = DateTime.Now.AddHours(1);

            Response.Cookies.Append("cookie", localUser.authToken, options);

            return Ok();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (Request.Cookies["cookie"] == null)
            {
                return Unauthorized();
            }
            else if (!await checkAuthorisation(Request.Cookies["cookie"]))
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
            var localUser = await _context.User.Where(u => u.authToken == cookieValue).FirstOrDefaultAsync();
            if (localUser == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}