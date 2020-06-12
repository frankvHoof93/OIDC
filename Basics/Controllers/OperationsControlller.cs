using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basics.Controllers
{
    public class OperationsControlller : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public OperationsControlller(IAuthorizationService authorization)
        {
            _authorizationService = authorization;
        }

        public async Task<IActionResult> Open()
        {
            // Get resource from DB
            CookieJar jar = new CookieJar()
            {
                Name = "Ben&Jerrys"
            };
            // Check authorization
            AuthorizationResult result = await _authorizationService.AuthorizeAsync(HttpContext.User, jar, CookieJarAuthRequirements.Open);
            return View();
        }
    }

    public class CookieJarAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, CookieJar>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, CookieJar resource)
        {
            if (requirement.Name == CookieJarOperations.Look)
            {
                if (context.User.Identity.IsAuthenticated)
                    context.Succeed(requirement);
            }
            else if (requirement.Name == CookieJarOperations.ComeNear)
            {
                if (context.User.HasClaim("Friend", "Good"))
                    context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public static class CookieJarAuthRequirements
    {
        public static OperationAuthorizationRequirement Open = new OperationAuthorizationRequirement
        {
            Name = CookieJarOperations.Open
        };
    }

    public static class CookieJarOperations
    {
        public static string Open = "Open";
        public static string TakeCookie = "TakeCookie";
        public static string Look = "Look";
        public static string ComeNear = "ComeNear";
    }

    public class CookieJar
    {
        public string Name { get; set; }
    }
}
