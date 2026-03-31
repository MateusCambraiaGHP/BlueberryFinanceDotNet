using BlueberryFinance.Web.Clients;
using BlueberryFinance.Web.Components;
using BlueberryFinance.Web.Data;
using BlueberryFinance.Web.Services;
using BlueberryFinance.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// ── EF Core + ASP.NET Identity ────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = false;
    opt.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/login";
    opt.LogoutPath = "/account/logout";
    opt.AccessDeniedPath = "/access-denied";
    opt.SlidingExpiration = true;
    opt.ExpireTimeSpan = TimeSpan.FromDays(1);
});

// ── Authorization ──────────────────────────────────────────────────────────────
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("UserOrAdmin", p => p.RequireRole("Admin", "User"));
});

// ── Blazor / MudBlazor ────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

// ── Token service (scoped — reads api_jwt cookie per Blazor circuit) ──────────
builder.Services.AddScoped<ITokenService, TokenService>();

// ── API HTTP clients ──────────────────────────────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7003/";

builder.Services.AddTransient<ApiDelegatingHandler>();

// AuthClient: no delegating handler — used to obtain the JWT (no token yet)
builder.Services.AddHttpClient<AuthClient>(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<TransactionClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<BankAccountClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<CategoryClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<StoreClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<FixedExpenseClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<CurrencyClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<ChatClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<AgentApprovalClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<ReportClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

builder.Services.AddHttpClient<CsvImportClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiDelegatingHandler>();

var app = builder.Build();

// ── Migrate & seed Identity ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = app.Configuration["DEFAULT_ADMIN_EMAIL"] ?? "mateus@hotmail.com";
    var adminPassword = app.Configuration["DEFAULT_ADMIN_PASSWORD"] ?? "ChangeMe123!";

    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(admin, adminPassword);
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// ── Middleware pipeline ────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseDeveloperExceptionPage();
app.UseAntiforgery();

// ── Account endpoints ─────────────────────────────────────────────────────────
app.MapPost("/account/login", async (HttpContext context, SignInManager<IdentityUser> signIn, AuthClient authClient) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var result = await signIn.PasswordSignInAsync(email, password,
        isPersistent: false, lockoutOnFailure: false);

    if (result.Succeeded)
    {
        var apiToken = await authClient.LoginAsync(email, password);
        if (apiToken is not null)
        {
            context.Response.Cookies.Append("api_jwt", apiToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !app.Environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(apiToken.ExpiresAt - DateTime.UtcNow)
            });
        }

        var redirect = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
        return Results.Redirect(redirect);
    }

    var encodedReturn = Uri.EscapeDataString(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    return Results.Redirect($"/login?error=1&returnUrl={encodedReturn}");
}).DisableAntiforgery();

app.MapGet("/account/logout", async (HttpContext context, SignInManager<IdentityUser> signIn) =>
{
    await signIn.SignOutAsync();
    context.Response.Cookies.Delete("api_jwt");
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
