using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NETCore.MailKit.Core;
using System.Text;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IdentityExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signinManager;
        private readonly IEmailService emailService;

        // Inject services that were added in Startup
        // AddIdentity calls AddSingleton, etc. internally
        public HomeController(
            UserManager<IdentityUser> mgr, 
            SignInManager<IdentityUser> signin,
            IEmailService mailService)
        {
            userManager = mgr;
            signinManager = signin;
            emailService = mailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Login Functionality
            IdentityUser user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                // Sign in
                SignInResult result = await signinManager.PasswordSignInAsync(user, password, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // Register Functionality
            IdentityUser user = new IdentityUser
            {
                UserName = username,
                Email = ""
            };      
            
            IdentityResult result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Generate Verification-Email-Token
                string code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                // Encode
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string link = Url.Action(nameof(VerifyEmail), "Home", new { userId = user.Id, code }, Request.Scheme, Request.Host.ToString());
                await emailService.SendAsync("test@test.com", "Verification Email IdentityExample", $"<a href=\"{link}\">Verify Email</a>", true);

                return RedirectToAction("EmailVerification");
            }

            return RedirectToAction("Index");
        }

        public IActionResult EmailVerification() => View();

        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            // Decode
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            // Verify with Code
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                    return View();
            }
            else
            {
                return BadRequest();
            }

            return BadRequest();
        }

        public async Task<IActionResult> LogOut()
        {
            await signinManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}
