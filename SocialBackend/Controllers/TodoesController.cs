using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialBackend.Data;
using SocialBackend.Models;

namespace SocialBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoesController : ControllerBase
    {
        private readonly SocialBackendContext _context;
        private readonly ICookieService _cookieService;

        public TodoesController(SocialBackendContext context, ICookieService cookieService)
        {
            _context = context;
            _cookieService = cookieService;
        }

        // GET: api/Todoes
        [HttpGet]
        public async Task<IActionResult> GetTodo()
        {
            if (string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            }
            if (!await checkAuthorisation(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            }
            return Ok(await _context.Todo.Where(t => t.user.authToken == _cookieService.getCookieValue(HttpContext)).ToListAsync());
        }

        // GET: api/Todoes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo([FromRoute] int id)
        {
            if (string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            } else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var todo = await _context.Todo.FindAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            _context.Entry(todo).Reference(t => t.user).Load();

            var localUser = await _context.User.Where(u => u.authToken == _cookieService.getCookieValue(HttpContext)).FirstOrDefaultAsync();
            if (localUser == null)
            {
                return Unauthorized();
            } else
            {
                if (localUser.authToken != todo.user.authToken)
                {
                    return Unauthorized();
                }
            }

            todo.user.username = null;
            todo.user.password = null;
            todo.user.emailAddress = null;
            todo.user.authToken = null;

            return Ok(todo);
        }

        // PUT: api/Todoes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodo([FromRoute] int id, [FromBody] Todo todo)
        {
            if (string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            } else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != todo.id)
            {
                return BadRequest();
            }

            todo.user = await _context.User.FindAsync(todo.user.id);
            if (todo.user == null)
            {
                return NotFound();
            }

            var localUser = await _context.User.Where(u => u.authToken == _cookieService.getCookieValue(HttpContext)).FirstOrDefaultAsync();
            if (localUser == null)
            {
                return Unauthorized();
            }
            else
            {
                if (localUser.authToken != todo.user.authToken)
                {
                    return Unauthorized();
                }
            }

            _context.Entry(todo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoExists(id))
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

        // POST: api/Todoes
        [HttpPost]
        public async Task<IActionResult> PostTodo([FromBody] Todo todo)
        {
            if (string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            } else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            todo.user = await _context.User.FindAsync(todo.user.id);
            if (todo.user == null)
            {
                return NotFound();
            }
            var localUser = await _context.User.Where(u => u.authToken == _cookieService.getCookieValue(HttpContext)).FirstOrDefaultAsync();
            if (localUser == null)
            {
                return Unauthorized();
            }
            else
            {
                if (localUser.authToken != todo.user.authToken)
                {
                    return Unauthorized();
                }
            }
            _context.Todo.Add(todo);
            await _context.SaveChangesAsync();

            return Created("GetTodo", new { id = todo.id });
        }

        // DELETE: api/Todoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo([FromRoute] int id)
        {
            if (string.IsNullOrEmpty(_cookieService.getCookieValue(HttpContext)))
            {
                return Unauthorized();
            } else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var todo = await _context.Todo.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.Entry(todo).Reference(t => t.user).Load();

            var localUser = await _context.User.Where(u => u.authToken == _cookieService.getCookieValue(HttpContext)).FirstOrDefaultAsync();
            if (localUser == null)
            {
                return Unauthorized();
            }
            else
            {
                if (localUser.authToken != todo.user.authToken)
                {
                    return Unauthorized();
                }
            }

            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();

            return Ok(todo);
        }

        private bool TodoExists(int id)
        {
            return _context.Todo.Any(e => e.id == id);
        }

        private async Task<bool> checkAuthorisation(string cookieValue)
        {
            return await _context.User.Where(u => u.authToken == cookieValue).AnyAsync();
        }
    }
}