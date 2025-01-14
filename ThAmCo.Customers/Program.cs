using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ThAmCo.Customers.Data;
using ThAmCo.Customers.Middleware;
using ThAmCo.Products.Data;
using ThAmCo.Customers.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDbConnection")));

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDbConnection")));


// Configure JWT Bearer Authentication for API Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var auth0Domain = builder.Configuration["Auth0:Domain"];
    options.Authority = $"https://{auth0Domain}";
    options.Audience = builder.Configuration["Auth0:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = $"https://{auth0Domain}",
        ValidAudience = builder.Configuration["Auth0:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Auth0 Universal Login Authentication
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    var auth0Config = builder.Configuration.GetSection("Auth0");
    options.Domain = auth0Config["Domain"];
    options.ClientId = auth0Config["ClientId"];
    options.CallbackPath = new PathString("/callback");
    options.Scope = "openid profile email";
});

builder.Services.AddHostedService<ProductSyncService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Authentication and Authorization
app.UseAuthentication();

app.UseMiddleware<RedirectMiddleware>();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

