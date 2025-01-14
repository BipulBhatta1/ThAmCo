using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Customers.Data;

namespace ThAmCo.Customers.Middleware
{
    public class RedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RedirectMiddleware> _logger;

        public RedirectMiddleware(RequestDelegate next, ILogger<RedirectMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            _logger.LogInformation($"Handling request for {context.Request.Path}");

            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var userEmail = context.User.Identity.Name;

                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

                    // Redirect /Customers/Register if the user is already registered
                    if (context.Request.Path == "/Customers/Register")
                    {
                        _logger.LogInformation("Checking registration status for user.");
                        var isRegistered = dbContext.Customers.Any(c => c.Email == userEmail);
                        if (isRegistered)
                        {
                            _logger.LogInformation("User is registered. Redirecting to /Products/Index.");
                            context.Response.Redirect("/Products/Index");
                            return;
                        }
                    }

                    // Redirect / (home/index) to /Products/Index for authenticated users
                    if (context.Request.Path == "/")
                    {
                        _logger.LogInformation("Authenticated user accessing /. Redirecting to /Products/Index.");
                        context.Response.Redirect("/Products/Index");
                        return;
                    }
                }
            }

            // Pass the request to the next middleware in the pipeline
            await _next(context);
        }
    }
}
