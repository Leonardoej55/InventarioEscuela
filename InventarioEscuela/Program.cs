using InventarioEscuela.Data;
using InventarioEscuela.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// MVC + desactivar caché en las respuestas
// =========================================
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(
        new ResponseCacheAttribute
        {
            NoStore = true,
            Location = ResponseCacheLocation.None
        });
});

// =========================================
// BASE DE DATOS
// =========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CadenaSQL")
    );
});

// =========================================
// SERVICIOS
// =========================================
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// =========================================
// AUTENTICACIÓN POR COOKIES
// =========================================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Login/IniciarSesion";

        opciones.ExpireTimeSpan = TimeSpan.FromMinutes(20);

        opciones.SlidingExpiration = true;
    });

var app = builder.Build();
QuestPDF.Settings.License = LicenseType.Community;

// =========================================
// MANEJO DE ERRORES
// =========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// =========================================
// PIPELINE HTTP
// =========================================
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// =========================================
// RUTA PRINCIPAL
// =========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=IniciarSesion}/{id?}")
    .WithStaticAssets();

app.Run();