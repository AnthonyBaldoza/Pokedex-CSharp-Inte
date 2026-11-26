using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pokedex.Data;
using Pokedex.Middlewares;
using Pokedex.Repositories;
using Pokedex.Services;

// ============================================================
// PROGRAM.CS — Entry point ng application
// Dito nag-start ang lahat
//
// May dalawang bahagi:
// 1. BUILDER PHASE — I-register ang lahat ng services sa DI container
// 2. APP PHASE — I-configure ang middleware pipeline
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// PHASE 1: REGISTER SERVICES (Dependency Injection Container)
// ============================================================

// I-register ang Controllers
builder.Services.AddControllers();

// Swagger — para sa API documentation at testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server Database — kinukuha ang connection string mula sa appsettings
builder.Services.AddDbContext<PokedexDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// IDENTITY (LOGIN / CREATE ACCOUNT)
//
// - AddIdentityCore: user accounts + password hashing (PBKDF2, automatic)
// - AddEntityFrameworkStores: dito sine-save ang users (PokedexDbContext)
// - AddIdentityCookies: login session gamit ang HttpOnly cookie
//   (walang token sa JavaScript/localStorage)
// ============================================================
builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // Pwede ang space sa username (hal. "Anthony Baldoza"). Sinadya kong WALA ang '@' at '+'
        // para hindi malito ang login (ang '@' ay ginagamit para malaman kung email ang tina-type).
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._ ";

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        

        // 5 maling password = locked ng 5 minuto
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<PokedexDbContext>()
    .AddSignInManager();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "pokedex.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest   // local: http://localhost okay
        : CookieSecurePolicy.Always;         // production: https lang
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    // API ito: mag-return ng 401/403 imbes na mag-redirect sa login page
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

// ============================================================
// DEPENDENCY INJECTION REGISTRATIONS
//
// AddScoped = bagong instance per HTTP request
// Ito ang tamang lifetime para sa Database operations
//
// ORDER NG REGISTRATION:
// 1. Repository (nag-a-access ng database)
// 2. Service (gumagamit ng Repository)
// Controller ay automatic na registered ng AddControllers()
// ============================================================
builder.Services.AddScoped<IPokemonRepository, PokemonRepository>();
builder.Services.AddScoped<PokemonService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// WALA NANG CORS: ang frontend (wwwroot/index.html) ay galing sa SAME app,
// kaya same-origin — hindi kailangan ang CORS. Hindi rin ligtas ang AllowAnyOrigin
// kapag cookies ang gamit.

// ============================================================
// PHASE 2: BUILD THE APP
// ============================================================
var app = builder.Build();

// ============================================================
// MIDDLEWARE PIPELINE — Ang bawat HTTP request ay dumadaan dito
// ORDER IS IMPORTANT! Ang unang nakalagay ay unang tatawagan
// ============================================================

// 1. GLOBAL EXCEPTION HANDLER — Dapat UNANG middleware
//    Para mahuli ang lahat ng errors kahit saan sa pipeline
app.UseGlobalExceptionHandler();

// 2. Swagger — available lang sa Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pokédex API v1");
        options.RoutePrefix = "swagger";
    });
}

// 3. HTTPS Redirection — redirect HTTP to HTTPS
app.UseHttpsRedirection();

app.UseDefaultFiles();
// 4. Static Files — para ma-serve ang wwwroot/index.html
app.UseStaticFiles();

// 5. AUTHENTICATION then AUTHORIZATION — bago ang Controllers
//    UseAuthentication = "sino ka?" (binabasa ang cookie)
//    UseAuthorization  = "pwede ka ba rito?" ([Authorize])
app.UseAuthentication();
app.UseAuthorization();

// 6. Map Controllers — iru-route ang requests sa tamang Controller
app.MapControllers();

// ============================================================
// SEED DATABASE — I-apply ang migrations at mag-add ng initial data
// Ginagawa ito PAGKATAPOS ng app.Build() pero BAGO mag-run
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PokedexDbContext>();

    // Auto-apply pending migrations (kasama na ang Identity tables pag na-deploy)
    context.Database.Migrate();

    // Mag-seed ng initial data
    SeedData.Initialize(context);
}

// ============================================================
// START THE APPLICATION
// ============================================================
app.Run();