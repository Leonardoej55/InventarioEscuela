using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.Services;
using InventarioEscuela.Services.Reportes;
using InventarioEscuela.Services.Reportes.Pdf;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(opciones =>
{
    opciones.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (_, _) =>
            "Revise el formato de este campo: debe indicar una fecha, un número o una opción válida.");

    opciones.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        _ => "Ingrese un número válido.");

    opciones.Filters.Add(new ResponseCacheAttribute
    {
        NoStore = true,
        Location = ResponseCacheLocation.None
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CadenaSQL"));
});

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IReporteConsultaService, ReporteConsultaService>();
builder.Services.AddScoped<IReportePdfService, ReportePdfService>();
builder.Services.AddScoped<IReporteCsvService, ReporteCsvService>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<RelojSistema>();

builder.Services.AddOptions<OpcionesReportes>()
    .Bind(builder.Configuration.GetSection(OpcionesReportes.Seccion))
    .Validate(
        opciones => TimeZoneInfo.TryFindSystemTimeZoneById(
            opciones.ZonaHoraria, out _),
        "La zona horaria de los reportes no es válida.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Login/IniciarSesion";
        opciones.AccessDeniedPath = "/Login/AccesoDenegado";
        opciones.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        opciones.SlidingExpiration = true;
    });

var app = builder.Build();

QuestPDF.Settings.License = LicenseType.Community;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=IniciarSesion}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }