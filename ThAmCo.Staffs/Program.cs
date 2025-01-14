using Microsoft.EntityFrameworkCore;
using ThAmCo.Staffs.Data;
using ThAmCo.Customers.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure CustomerDbContext
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDbConnection")));

// Configure StaffDbContext
builder.Services.AddDbContext<StaffDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StaffDbConnection")));

// Add Authentication
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=StaffsHome}/{action=Index}/{id?}");

app.Run();
