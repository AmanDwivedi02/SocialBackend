using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialBackend.Controllers;
using SocialBackend.Data;
using SocialBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialBackendUnitTests
{
    [TestClass]
    public class TodoTests
    {

        public static readonly DbContextOptions<SocialBackendContext> options = new DbContextOptionsBuilder<SocialBackendContext>()
            .UseInMemoryDatabase(databaseName: "testDatabase")
            .Options;

        public static readonly IList<string> usernames = new List<string> { "userA", "userB", "userC", null };
        public static readonly IList<string> emails = new List<string> { "userA@user.com", "userB@user.com", "userC@user.com", null };
        public static readonly IList<string> hashedPasswords = new List<string> { "0AQfVI32kJ7bXQAJL9Xq1sHIVSU5mYJ/pBJuhp+4bPa6aV2M", "dHG8b7jneWJKaH33jqcKJEcxVa8pSLiTMo255WgDEbLvSJRq", "nUZNT1jL+uLIoileVsnXXTmOzb5pgnhZJeQyXmDubZhfdBZx", null };
        public static readonly IList<string> passwords = new List<string> { "abcd", "blah", "string", null };
        public static readonly IList<string> authTokens = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null };
        public static IList<User> users = new List<User>();

        public static readonly IList<string> tasks = new List<string> { "taskA", "taskB", "taskC", null };
        public static readonly IList<DateTime> dueDates = new List<DateTime> { DateTime.Now, DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), DateTime.Now.AddDays(4) };
        public static readonly IList<bool> completes = new List<bool> { false, true, false, false };
        public static IList<Todo> todos = new List<Todo>();

        public static int i = 999;

        public class FakeCookieService : ICookieService
        {
            public string getCookieValue(HttpContext context)
            {
                if (i == 999)
                {
                    return "";
                }
                else
                {
                    return authTokens[i];
                }
            }
            public void setCookie(HttpContext context, string authToken)
            {
                authTokens[i] = authToken;
                return;
            }
        }

        [TestInitialize]
        public void SetupDb()
        {
            using (var context = new SocialBackendContext(options))
            {
                User userA = new User()
                {
                    username = usernames[0],
                    emailAddress = emails[0],
                    password = hashedPasswords[0],
                    authToken = authTokens[0]
                };
                users.Add(userA);

                User userB = new User()
                {
                    username = usernames[1],
                    emailAddress = emails[1],
                    password = hashedPasswords[1],
                    authToken = authTokens[1]
                };
                users.Add(userB);

                context.User.Add(userA);
                context.User.Add(userB);
                context.SaveChanges();

                Todo todoA = new Todo()
                {
                    task = tasks[0],
                    dueDate = dueDates[0],
                    complete = completes[0],
                    user = context.User.Where(u => u.username == users[0].username).AsNoTracking().FirstOrDefault()

                };
                todos.Add(todoA);

                Todo todoB = new Todo()
                {
                    task = tasks[1],
                    dueDate = dueDates[1],
                    complete = completes[1],
                    user = context.User.Where(u => u.username == users[1].username).AsNoTracking().FirstOrDefault()

                };
                todos.Add(todoB);

                context.Todo.Add(todoA);
                context.Todo.Add(todoB);
                context.SaveChanges();
            }
        }

        [TestCleanup]
        public void ClearDb()
        {
            using (var context = new SocialBackendContext(options))
            {
                context.User.RemoveRange(context.User);
                context.Todo.RemoveRange(context.Todo);
                context.SaveChanges();
            };
            i = 999;
        }

        [TestMethod]
        public async Task TestNonAuthorisedGet()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; //
                string username = usernames[i];
                string password = passwords[i];

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.GetTodo();

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestNoCookieGet()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 3; //
                string username = usernames[i];
                string password = passwords[i];

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.GetTodo();

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestGet()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 0; //
                string username = usernames[i];
                string password = passwords[i];

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.GetTodo() as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                var okObject = result as OkObjectResult;
                var todo = okObject.Value as List<Todo>;

                Assert.IsNotNull(todo);
                Assert.IsTrue(todo.Count == 1);
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestNullCookiePut()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; // task does not currently exists in db
                Todo updatedTodo = new Todo()
                {
                    id = (await context.Todo.AsNoTracking().FirstAsync()).id,
                    task = tasks[i],
                    complete = completes[i],
                    dueDate = dueDates[i]
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.UpdateTask((await context.Todo.AsNoTracking().FirstAsync()).id, updatedTodo) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestEditOtherUsersTask()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 0; //
                IQueryable<Todo> _todo = from t in context.Todo
                                         orderby t.id ascending
                                         select t;
                var dbTodo = await _todo.AsNoTracking().FirstOrDefaultAsync();
                Todo updatedTodo = new Todo()
                {
                    id = dbTodo.id + 1,
                    task = tasks[i],
                    complete = completes[i],
                    dueDate = dueDates[i]
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.UpdateTask(dbTodo.id + 1, updatedTodo) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestEditTodo()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 0; //
                IQueryable<Todo> _todo = from t in context.Todo
                                         orderby t.id ascending
                                         select t;
                var dbTodo = await _todo.AsNoTracking().FirstOrDefaultAsync();
                Todo updatedTodo = new Todo()
                {
                    id = dbTodo.id,
                    task = tasks[i],
                    complete = !completes[i],
                    dueDate = dueDates[i]
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.UpdateTask(dbTodo.id, updatedTodo) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NoContentResult));

                var updatedDbTodo = await _todo.AsNoTracking().FirstOrDefaultAsync();

                Assert.AreEqual(dbTodo.id, updatedDbTodo.id);
                Assert.AreEqual(dbTodo.task, updatedDbTodo.task);
                Assert.AreEqual(dbTodo.dueDate, updatedDbTodo.dueDate);
                Assert.AreNotEqual(dbTodo.complete, updatedDbTodo.complete);

                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestNullCookiePost()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; // task and user does not currently exists in db
                Todo newTodo = new Todo()
                {
                    task = tasks[i],
                    complete = completes[i],
                    dueDate = dueDates[i]
                };
                i = 3; //for null cookie

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.PostTodo(newTodo) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());

            }
        }

        [TestMethod]
        public async Task TestNonExistantUserPost()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; // task and user does not currently exists in db
                Todo newTodo = new Todo()
                {
                    task = tasks[i],
                    complete = completes[i],
                    dueDate = dueDates[i]
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.PostTodo(newTodo) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(BadRequestResult));
                Assert.AreEqual(2, context.Todo.Count());

            }
        }

        [TestMethod]
        public async Task TestPost()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; //task 2 doesn't esists in db
                Todo newTodo = new Todo()
                {
                    task = tasks[i],
                    complete = completes[i],
                    dueDate = dueDates[i]
                };
                i = 1; // user 1 exists

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.PostTodo(newTodo) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(CreatedResult));

                IQueryable<Todo> _todo = from t in context.Todo
                                         orderby t.id descending
                                         select t;
                var newestTodo = await _todo.AsNoTracking().FirstOrDefaultAsync();

                Assert.AreEqual(newTodo.task, newestTodo.task);
                Assert.AreEqual(newTodo.complete, newestTodo.complete);
                Assert.AreEqual(newTodo.dueDate, newestTodo.dueDate);

                Assert.AreEqual(newTodo.user.emailAddress, users[i].emailAddress);
                Assert.AreEqual(newTodo.user.username, users[i].username);
                Assert.AreEqual(newTodo.user.password, users[i].password);
                Assert.AreEqual(newTodo.user.online, users[i].online);

                Assert.AreEqual(3, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestNullCookieDelete()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 3; //for null cookie

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.DeleteTodo((await context.Todo.AsNoTracking().FirstOrDefaultAsync()).id) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());

            }
        }

        [TestMethod]
        public async Task TestNonExistantDelete()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 1; //for authorised user
                IQueryable<Todo> _todo = from t in context.Todo
                                         orderby t.id descending
                                         select t;
                var dbTodo = await _todo.AsNoTracking().FirstOrDefaultAsync(); // last TodoItem in db

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.DeleteTodo(dbTodo.id + 1) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestDeleteOtherUsersTask()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 0; //for authorised user
                IQueryable<Todo> _todo = from t in context.Todo
                                         orderby t.id descending
                                         select t;
                var dbTodo = await _todo.AsNoTracking().FirstOrDefaultAsync(); // todoItem belonging to different user

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.DeleteTodo(dbTodo.id) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestDeleteNonDbAuthToken()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; //for non existant user with a valid authtoken
                IQueryable<Todo> _todo = from t in context.Todo
                                         orderby t.id descending
                                         select t;
                var dbTodo = await _todo.AsNoTracking().FirstOrDefaultAsync();

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.DeleteTodo(dbTodo.id) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
                Assert.AreEqual(2, context.Todo.Count());
            }
        }

        [TestMethod]
        public async Task TestDelete()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 0; // exisiting user
                var dbTodo = await context.Todo.AsNoTracking().Where(t => t.user.username == usernames[i]).FirstOrDefaultAsync(); // existing todo

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.DeleteTodo(dbTodo.id) as IActionResult;

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                Assert.AreEqual(1, context.Todo.Count());
                Assert.AreEqual(2, context.User.Count());

                var okObject = result as OkObjectResult;
                var deletedItem = okObject.Value as Todo;

                Assert.AreEqual(dbTodo.id, deletedItem.id);
                Assert.AreEqual(dbTodo.task, deletedItem.task);
                Assert.AreEqual(dbTodo.complete, deletedItem.complete);
                Assert.AreEqual(dbTodo.dueDate, deletedItem.dueDate);
            }
        }
    }
}
