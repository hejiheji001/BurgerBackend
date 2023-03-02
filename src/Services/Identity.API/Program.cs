var builder = WebApplication.CreateBuilder();

builder.AddCustomConfiguration();
builder.AddCustomSerilog();
builder.AddCustomMvc();
builder.AddCustomDatabase();
builder.AddCustomIdentity();
builder.AddCustomIdentityServer();
builder.AddCustomAuthentication();
builder.AddCustomHealthChecks();
builder.AddCustomApplicationServices();

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

var pathBase = builder.Configuration["PATH_BASE"];
if (!string.IsNullOrEmpty(pathBase)) app.UsePathBase(pathBase);
app.UseStaticFiles();

// This cookie policy fixes login issues with Chrome 80+ using HHTP
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

app.UseRouting();

app.UseAuthentication();

app.UseIdentityServer();

app.UseAuthorization();

app.MapDefaultControllerRoute();

app.MapHealthChecks("/hc", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});
try
{
    app.Logger.LogInformation("Seeding database ({ApplicationName})...", Identity.API.Program.AppName);

    // Apply database migration automatically. Note that this approach is not
    // recommended for production scenarios. Consider generating SQL scripts from
    // migrations instead.
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.EnsureSeedData(scope, app.Configuration, app.Logger);
    }

    app.Logger.LogInformation("Starting web host ({ApplicationName})...", Identity.API.Program.AppName);
    app.Run();

    return 0;
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Host terminated unexpectedly ({ApplicationName})...", Identity.API.Program.AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables();

    var config = builder.Build();
    return builder.Build();
}

namespace Identity.API
{
    public class Program
    {
        public static string AppName => "Identity.API";
    }

    public static class ProgramExtensions
    {
        public static void AddCustomConfiguration(this WebApplicationBuilder builder)
        {
            builder.Configuration.AddConfiguration(GetConfiguration()).Build();
        }

        public static void AddCustomSerilog(this WebApplicationBuilder builder)
        {
            var seqServerUrl = builder.Configuration["SeqServerUrl"];
            var logstashUrl = builder.Configuration["LogstashgUrl"];

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", Program.AppName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://localhost:8080" : logstashUrl, null)
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
        }

        public static void AddCustomMvc(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            builder.Services.AddRazorPages();
        }


        public static void AddCustomDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(builder.Configuration["ConnectionString"]));
        }

        public static void AddCustomIdentity(this WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddDefaultTokenProviders();
        }


        public static void AddCustomIdentityServer(this WebApplicationBuilder builder)
        {
            var identityServerBuilder = builder.Services.AddIdentityServer(options =>
                {
                    options.IssuerUri = "null";
                    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddInMemoryIdentityResources(Config.GetResources())
                .AddInMemoryApiScopes(Config.GetApiScopes())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients(builder.Configuration))
                .AddAspNetIdentity<ApplicationUser>();

            // not recommended for production - you need to store your key material somewhere secure
            identityServerBuilder.AddDeveloperSigningCredential();
        }

        public static void AddCustomAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("TenantPolicy", policy => policy.RequireRole("Tenant"));
                options.AddPolicy("HostPolicy", policy => policy.RequireRole("Host"));
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("DashboardPolicy", policy => policy.RequireAssertion(x => (
                    x.User.IsInRole("Host") || x.User.IsInRole("Tenant")
                )));

            });


        }

        public static void AddCustomHealthChecks(this WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(builder.Configuration["ConnectionString"],
                    name: "IdentityDB-check",
                    tags: new[] { "IdentityDB" });
        }

        public static void AddCustomApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IProfileService, ProfileService>();
            builder.Services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
            builder.Services.AddTransient<IRedirectService, RedirectService>();
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}