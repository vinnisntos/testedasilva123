using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Services;
using Npgsql;
using Microsoft.AspNetCore.HttpOverrides; // Adicionado para o Proxy Reverso

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Tratativa para pegar a Connection String tanto no Local quanto no Render
var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__SupabaseConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string do Supabase não informada. Verifique as Environment Variables no Render.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IReservaService, ReservaService>();

// CONFIGURAÇÃO DE SEGURANÇA
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AcessoNegado"; // Crie essa página depois se quiser
        options.ExpireTimeSpan = TimeSpan.FromHours(30);
        // Garante que o cookie funcione mesmo por trás do proxy do Render
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// DEFINIÇÃO DA POLÍTICA DE ADMIN
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/App");
    // Aqui a trava: Só entra na pasta Admin quem tiver a Role "Admin"
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
});

// --- CONFIGURAÇÃO PARA O RENDER (PROXY REVERSO) ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // O Render tem IPs dinâmicos, então limpamos isso para o ASP.NET aceitar o cabeçalho
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// ESSA LINHA PRECISA SER A PRIMEIRA DO PIPELINE
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();