using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Collections.Generic;
using AzureFunctions.Extensions.Swashbuckle.Attribute;

namespace UserAPI
{
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IActionResult))]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(IActionResult))]
    [ProducesResponseType((int)HttpStatusCode.Conflict, Type = typeof(IActionResult))]
    public class HttpTrigger
    {
        private readonly UserDbContext _context;

        public HttpTrigger(UserDbContext context)
        {
            _context = context;
        }

        [FunctionName("GetUsers")]
        public virtual IActionResult GetUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP GET/users trigger function processed a request.");

            var usersArray = _context.users.OrderBy(p => p.UserName).ToArray();
            return new OkObjectResult(usersArray);
        }

        [FunctionName("GetUserDetails")]
        [QueryStringParameter("id", "user id", DataType = typeof(Guid), Required = true)]
        public IActionResult GetUserDetails(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/details")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP GET/user details trigger function processed a request.");

                var userId = req.Query["id"];

                var user = _context.users.Where(x => x.UserId == Guid.Parse(userId)).FirstOrDefault();
                return new OkObjectResult(user);
            }
            catch (Exception ex)
            {
                return new BadRequestResult();
            }
        }

        [FunctionName("InsertUser")]
        public IActionResult InsertUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/insert")][RequestBodyType(typeof(User), "User model")] HttpRequest req,
            ILogger log)
        {
            try
            {

                log.LogInformation("C# HTTP POST/users trigger function processed a request.");

                var content = new StreamReader(req.Body).ReadToEndAsync().Result;

                User user = JsonConvert.DeserializeObject<User>(content);

                UserService service = new UserService(_context);

                if (service.ValidateUser(user))
                {
                    _context.users.Add(user);
                    _context.SaveChanges();
                }
                else
                {
                    return new BadRequestObjectResult(false);
                }

                return new OkObjectResult(user.UserId);
            }
            catch (Exception ex)
            {
                return new BadRequestResult();
            }
        }

        [FunctionName("UpdateUser")]
        public IActionResult UpdateUser(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/update")] [RequestBodyType(typeof(User), "User model")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP PUT/users trigger function processed a request.");

                var content = new StreamReader(req.Body).ReadToEndAsync().Result;

                var user = JsonConvert.DeserializeObject<User>(content);

                _context.users.Update(user);
                _context.SaveChanges();

                return new OkObjectResult(user.UserId);
            }
            catch (Exception ex)
            {
                return new BadRequestResult();
            }
        }

        [FunctionName("DeleteUser")]
        [QueryStringParameter("userId", "user id", DataType = typeof(Guid), Required = true)]
        public IActionResult DeleteUser(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/delete")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP PUT/users trigger function processed a request.");

            string id = req.Query["userId"];

            User user = FindUser(id);

            _context.users.Remove(user);
            _context.SaveChanges();

            return new OkObjectResult(user.UserId);
        }

        private User FindUser(string userId)
        {
            return _context.users.Where(x => x.UserId == Guid.Parse(userId)).FirstOrDefault();
        }
    }
}
