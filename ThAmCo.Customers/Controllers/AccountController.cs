using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Customers.Data;

namespace ThAmCo.Customers.Controllers
{
    public class AccountController : Controller
    {
        private readonly CustomerDbContext _context;

        public AccountController(CustomerDbContext context)
        {
            _context = context;
        }

        // Login endpoint - initiates the authentication process
        public async Task Login(string returnUrl = "/Products/Index")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(
                Auth0Constants.AuthenticationScheme,
                authenticationProperties
            );
        }

        // Callback endpoint - handles the Auth0 response after successful authentication
        public async Task<IActionResult> Callback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(Auth0Constants.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return RedirectToAction("Error", "Home");

            var returnUrl = authenticateResult.Properties?.RedirectUri ?? "/Products/Index";

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authenticateResult.Principal,
                authenticateResult.Properties
            );

            return Redirect(returnUrl);
        }

        // Logout endpoint - handles user logout
        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(Url.Action("Index", "Home"))
                .Build();

            await HttpContext.SignOutAsync(
                Auth0Constants.AuthenticationScheme,
                authenticationProperties
            );
        }

        // Profile endpoint - displays user information and dynamic links
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);

            var model = new
            {
                Name = User.Identity.Name,
                EmailAddress = userEmail,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value,
                IsCustomerRegistered = customer != null
            };

            return View(model);
        }
    }
}
