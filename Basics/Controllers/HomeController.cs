using Basics.CustomPolicyProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Basics.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        // Injection In Constructor. Useful for Class-Wide used services
        public HomeController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        [Authorize(Policy = "Claim.DoB")]
        public IActionResult SecretPolicy()
        {
            return View("Secret");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SecretRole()
        {
            return View("Secret");
        }


        [SecurityLevel(5)]
        public IActionResult SecurityLevel5()
        {
            return View("Secret");
        }

        [SecurityLevel(10)]
        public IActionResult SecurityLevel10()
        {
            return View("Secret");
        }

        // Specifically allow this method to bypass global Authorization-Restrictions
        [AllowAnonymous]
        public IActionResult Authenticate()
        {
            var grandmaClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "Bob"),
                new Claim(ClaimTypes.Email, "bob@email.com"),
                new Claim(ClaimTypes.DateOfBirth, "01/01/2000"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(DynamicPolicies.SecurityLevel, "7"),
                new Claim("Grandma.Says", "He a gud boy")
            };

            var licenseClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "Bob K Foo"),
                new Claim("DrivingLicense", "A+")
            };

            var grandmaIdentity = new ClaimsIdentity(grandmaClaims, "Grandma Identity");
            var licenseIdentity = new ClaimsIdentity(licenseClaims, "Government");

            var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity, licenseIdentity });

            HttpContext.SignInAsync(userPrincipal);

            return RedirectToAction("Index");
        }

        // Injection in Method. Useful for services only used in a few methods
        public async Task<IActionResult> DoStuff(
            [FromServices] IAuthorizationService _authorizationService
            )
        {

            var builder = new AuthorizationPolicyBuilder("Schema");
            var customPolicy = builder.RequireClaim("Hello").Build();


            // Result = Authorize(User, Policy)
            AuthorizationResult result = await _authorizationService.AuthorizeAsync(HttpContext.User, "Claim.DoB"); // Added in Startup
            AuthorizationResult result2 = await _authorizationService.AuthorizeAsync(HttpContext.User, customPolicy); // Newly Created (Custom) Policy (Fails because user does not have this claim)

            if (result.Succeeded)
            {
                return View("Index");
            }

            return View("Index");
        }
    }
}
