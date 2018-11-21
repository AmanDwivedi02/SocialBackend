using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public class UserTests
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

                User userB= new User()
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
            }
        }

        [TestCleanup]
        public void ClearDb()
        {
            using (var context = new SocialBackendContext(options))
            {
                context.User.RemoveRange(context.User);
                context.SaveChanges();
            };
            i = 999;
        }

        [TestMethod]
        public async Task TestLoginNonExistant()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                string username = "test";
                string password = "pass";
                User userA = new User
                {
                    username = username,
                    password = password
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var result = await usersController.login(userA);

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }
        }

        [TestMethod]
        public async Task TestLogin()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 0;
                string username = usernames[i];
                string password = passwords[i];
                User user = new User
                {
                    username = username,
                    password = password
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var result = await usersController.login(user);


                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(OkResult));
            }
        }

        [TestMethod]
        public async Task TestNullCookieAuthorisation()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 3;

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var result = await usersController.checkLoggedIn();


                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
            }
        }

        [TestMethod]
        public async Task TestNonAuthorisedCookieAuthorisation()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2;

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var result = await usersController.checkLoggedIn();


                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
            }
        }

        [TestMethod]
        public async Task TestAuthorisedCookieAuthorisation()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 1;

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var result = await usersController.checkLoggedIn();


                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(OkResult));
            }
        }

        [TestMethod]
        public async Task TestLoginIncorrectPassword()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 1;
                string username = usernames[i];
                string password = "wrongPassword";
                User user = new User
                {
                    username = username,
                    password = password
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var result = await usersController.login(user);


                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }
        }

        [TestMethod]
        public async Task TestRegister()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2;
                string username = usernames[i];
                string password = passwords[i];
                string emailAddress = emails[i];
                User user = new User
                {
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var result = await usersController.RegisterUser(user);

                // Then
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(CreatedResult));
                Assert.AreEqual(emails[i], (await context.User.LastAsync()).emailAddress);
                Assert.AreEqual(usernames[i], (await context.User.LastAsync()).username);
            }
        }

        [TestMethod]
        public async Task TestEmptyFieldRegister()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                string username = null;
                string password = null;
                string emailAddress = null;
                User userA = new User
                {
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };

                username = "";
                password = null;
                emailAddress = null;
                User userB = new User
                {
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };

                username = null;
                password = "";
                emailAddress = null;
                User userC = new User
                {
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };

                username = null;
                password = null;
                emailAddress = "";
                User userD = new User
                {
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };

                username = "";
                password = "";
                emailAddress = "";
                User userE = new User
                {
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var resultA = await usersController.RegisterUser(userA);
                var resultB = await usersController.RegisterUser(userB);
                var resultC = await usersController.RegisterUser(userC);
                var resultD = await usersController.RegisterUser(userD);
                var resultE = await usersController.RegisterUser(userE);


                // Then
                Assert.IsNotNull(resultA);
                Assert.IsInstanceOfType(resultA, typeof(BadRequestResult));
                Assert.IsNotNull(resultB);
                Assert.IsInstanceOfType(resultB, typeof(BadRequestResult));
                Assert.IsNotNull(resultC);
                Assert.IsInstanceOfType(resultC, typeof(BadRequestResult));
                Assert.IsNotNull(resultD);
                Assert.IsInstanceOfType(resultD, typeof(BadRequestResult));
                Assert.IsNotNull(resultE);
                Assert.IsInstanceOfType(resultE, typeof(BadRequestResult));
            }
        }

        [TestMethod]
        public async Task TestUpdateOtherUser()
        {
            using (var context = new SocialBackendContext(options))
            {
                
                // Given
                i = 1; //
                IQueryable<User> _user = from u in context.User
                                         orderby u.id descending
                                         select u;
                var originalUser = await _user.AsNoTracking().FirstOrDefaultAsync();
                User incorrectUser = await context.User.FindAsync(originalUser.id-1);
                int userId = originalUser.id;
                string username = usernames[i];
                string password = passwords[i];
                string emailAddress = emails[i];
                // new user has id one before user we want to edit
                User userA = new User
                {
                    id = userId-1,
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var resultA = await usersController.UpdateUser(userId, userA);
                
                // Then
                Assert.IsNotNull(resultA);
                Assert.IsInstanceOfType(resultA, typeof(BadRequestResult));

                User dbUser = await context.User.FindAsync(originalUser.id);

                Assert.AreEqual(originalUser.id, dbUser.id);
                Assert.AreEqual(originalUser.username, dbUser.username);
                Assert.AreEqual(originalUser.password, dbUser.password);
                Assert.AreEqual(originalUser.emailAddress, dbUser.emailAddress);
                Assert.AreEqual(originalUser.authToken, dbUser.authToken);
                Assert.AreEqual(originalUser.online, dbUser.online);

                dbUser = await context.User.FindAsync(originalUser.id - 1);

                Assert.AreEqual(incorrectUser.id, dbUser.id);
                Assert.AreEqual(incorrectUser.username, dbUser.username);
                Assert.AreEqual(incorrectUser.password, dbUser.password);
                Assert.AreEqual(incorrectUser.emailAddress, dbUser.emailAddress);
                Assert.AreEqual(incorrectUser.authToken, dbUser.authToken);
                Assert.AreEqual(incorrectUser.online, dbUser.online);

                
            }
        }

        [TestMethod]
        public async Task TestUpdateUser()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; //
                IQueryable<User> _user = from u in context.User
                                         orderby u.id descending
                                         select u;
                var originalUser = await _user.AsNoTracking().FirstOrDefaultAsync();
                int userId = originalUser.id;
                string username = usernames[i];
                string password = passwords[i];
                string emailAddress = emails[i];
                // new user has id one before user we want to edit
                User userA = new User
                {
                    id = userId,
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };
                // i needs to be set to 1 to ensure correct 
                i = 1;

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var resultA = await usersController.UpdateUser(userId, userA);

                // Then
                Assert.IsNotNull(resultA);
                Assert.IsInstanceOfType(resultA, typeof(NoContentResult));

                User dbUser = await context.User.FindAsync(originalUser.id);

                Assert.AreEqual(originalUser.id, dbUser.id);
                Assert.AreNotEqual(originalUser.username, dbUser.username);
                Assert.AreNotEqual(originalUser.password, dbUser.password);
                Assert.AreNotEqual(originalUser.emailAddress, dbUser.emailAddress);
                Assert.AreEqual(originalUser.authToken, dbUser.authToken);
                Assert.AreEqual(originalUser.online, dbUser.online);

                userA.password = hashedPasswords[i];

                Assert.AreEqual(userA.id, dbUser.id);
                Assert.AreEqual(userA.username, dbUser.username);
                Assert.AreEqual(userA.password, dbUser.password);
                Assert.AreEqual(userA.emailAddress, dbUser.emailAddress);
                Assert.AreEqual(userA.authToken, dbUser.authToken);
                Assert.AreEqual(userA.online, dbUser.online);

                
            }
        }

        [TestMethod]
        public async Task TestUpdateNoPassword()
        {
            using (var context = new SocialBackendContext(options))
            {
                // Given
                i = 2; //
                User originalUser = await context.User.AsNoTracking().LastAsync();
                int userId = originalUser.id;
                string username = usernames[i];
                string password = "";
                string emailAddress = emails[i];
                // new user has id one before user we want to edit
                User userA = new User
                {
                    id = userId,
                    username = username,
                    password = password,
                    emailAddress = emailAddress
                };
                // i needs to be set to 1 to ensure correct 
                i = 1;

                //When
                ICookieService fakeCookie = new FakeCookieService();
                UsersController usersController = new UsersController(context, fakeCookie);
                var resultA = await usersController.UpdateUser(userId, userA);

                // Then
                Assert.IsNotNull(resultA);
                Assert.IsInstanceOfType(resultA, typeof(NoContentResult));
                userA.password = hashedPasswords[i];

                User dbUser = await context.User.FindAsync(originalUser.id);

                Assert.AreEqual(userA.id, dbUser.id);
                Assert.AreEqual(userA.username, dbUser.username);
                Assert.AreEqual(userA.password, dbUser.password);
                Assert.AreEqual(userA.emailAddress, dbUser.emailAddress);
                Assert.AreEqual(userA.authToken, dbUser.authToken);
                Assert.AreEqual(userA.online, dbUser.online);
            }
        }
    }
}
