using FHP.Core.Models;
using FHP.Core.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// PaaS hosts (Railway, etc.) assign the listening port at runtime via $PORT.
string? port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddRazorPages();

// All persistent app data (JSON stores + Data Protection keys) lives under this
// directory. Defaults to App_Data for local dev. In production, set DATA_DIR to a
// mounted volume path (e.g. Railway) so data survives redeploys/restarts.
string dataDir = Environment.GetEnvironmentVariable("DATA_DIR")
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir);

// Persist Data Protection keys to disk so Forms-Auth-style login cookies survive
// an app restart during development, instead of invalidating every session.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDir, "keys")))
    .SetApplicationName("FHP.Web");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IUserRepository>(sp =>
    new JsonUserRepository(Path.Combine(dataDir, "users.json")));

builder.Services.AddSingleton<IUserGroupRepository>(sp =>
    new JsonUserGroupRepository(Path.Combine(dataDir, "userGroups.json")));

builder.Services.AddSingleton<IDashboardGroupRepository>(sp =>
    new JsonDashboardGroupRepository(Path.Combine(dataDir, "dashboardGroups.json")));

var app = builder.Build();

SeedDefaultData(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Railway (and most PaaS hosts) terminate TLS at the edge and forward plain HTTP
// to the container, so trust their X-Forwarded-* headers instead of redirecting
// every request — otherwise UseHttpsRedirection sees "http" and loops forever.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

// Fills in default master data and the first admin account on a brand-new data
// store (e.g. a freshly mounted, empty Railway volume). No-ops once any records
// exist, so it's safe to run on every startup.
static void SeedDefaultData(IServiceProvider services)
{
    var userGroups = services.GetRequiredService<IUserGroupRepository>();
    if (userGroups.GetAll().Count == 0)
    {
        foreach (string name in UserGroups.All)
        {
            userGroups.Add(new UserGroup
            {
                Name = name,
                Description = name,
                Status = UserStatuses.Active,
                LastUpdateBy = "system"
            });
        }
    }

    var dashboardGroups = services.GetRequiredService<IDashboardGroupRepository>();
    if (dashboardGroups.GetAll().Count == 0)
    {
        (string Name, string Description)[] defaults =
        {
            ("Admin", "Admin Dashboard"),
            ("Sales", "Sales Dashboard"),
            ("Stokist", "Stockist View"),
            ("Warehouse", "Gudang"),
        };

        foreach (var (name, description) in defaults)
        {
            dashboardGroups.Add(new DashboardGroup
            {
                Name = name,
                Description = description,
                Status = UserStatuses.Active,
                LastUpdateBy = "system"
            });
        }
    }

    var users = services.GetRequiredService<IUserRepository>();
    if (users.GetAll().Count == 0)
    {
        string? username = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
        string? email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        string? password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            users.Add(new User
            {
                FullName = Environment.GetEnvironmentVariable("ADMIN_FULLNAME") ?? "Administrator",
                Username = username,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Role = UserRoles.Admin,
                UserGroup = UserGroups.Admin,
                Status = UserStatuses.Active,
                LastUpdateBy = "system"
            });
            Console.WriteLine($"Seeded initial admin user '{username}' from ADMIN_* environment variables.");
        }
        else
        {
            Console.WriteLine(
                "No users exist yet and ADMIN_USERNAME / ADMIN_EMAIL / ADMIN_PASSWORD are not all set — " +
                "set them and redeploy to create the first admin, or run FHP.AdminSetup against this data directory.");
        }
    }
}
