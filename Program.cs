using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;  // ✅ Necesario para AddRazorRuntimeCompilation
using SuMejorPeso.Models;

var builder = WebApplication.CreateBuilder(args);

// ==================================
// CONFIGURACIÓN DE SERVICIOS
// ==================================
builder.Services.AddDbContext<GymContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))
    )
);

// ✅ Activar MVC + Razor Runtime Compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

// ==================================
// PIPELINE DE LA APLICACIÓN
// ==================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// ✅ Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
