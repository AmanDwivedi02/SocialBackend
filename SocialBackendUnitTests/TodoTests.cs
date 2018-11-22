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
                    user = users[0]

                };
                todos.Add(todoA);

                Todo todoB = new Todo()
                {
                    task = tasks[1],
                    dueDate = dueDates[1],
                    complete = completes[1],
                    user = users[1]

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
                i = 2; // user does not currently exists in db
                string username = usernames[i];
                string password = passwords[i];

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.GetTodo();

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
            }
        }

        [TestMethod]
        public async Task TestNoCookieGet()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 3; // user does not currently exists in db
                string username = usernames[i];
                string password = passwords[i];

                //When
                ICookieService fakeCookie = new FakeCookieService();
                TodoesController todoController = new TodoesController(context, fakeCookie);
                var result = await todoController.GetTodo();

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
            }
        }

        [TestMethod]
        public async Task TestGet()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 0; // user does not currently exists in db
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
            }
        }
    }
}
