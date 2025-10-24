using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using SuMejorPeso.Helpers; // <-- CAMBIO: Añadido para el PasswordHelper

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

// Activar MVC + Razor Runtime Compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// --- CAMBIO: Añadir servicio de Autenticación por Cookies ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // 1. Definir dónde está la página de Login
        options.LoginPath = "/Account/Login";
        
        // 2. (Opcional) Página de acceso denegado (si un rol no tiene permiso)
        options.AccessDeniedPath = "/Home/AccessDenied"; 
        
        // 3. (Opcional) Tiempo de expiración de la sesión
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });
// --- FIN DEL CAMBIO ---

var app = builder.Build();

// ============================================================
// --- CAMBIO: INICIO DE SEEDING DEL SUPERUSUARIO ---
// ============================================================
// Llamamos a nuestra función de seeding ANTES de correr la app
await SeedSuperUserAsync(app);
// ============================================================


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

// --- CAMBIO: Añadir Autenticación (¡El orden importa!) ---
// Debe ir DESPUÉS de UseRouting y ANTES de UseAuthorization
app.UseAuthentication(); // Primero identifica al usuario (lee la cookie)
app.UseAuthorization();  // Luego revisa sus permisos (ej: [Authorize])
// --- FIN DEL CAMBIO ---

// --- CAMBIO: Ruta por defecto apunta al Login ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
// --- FIN DEL CAMBIO ---

app.Run();


// ============================================================
// --- CAMBIO: AÑADIDA FUNCIÓN AUXILIAR PARA EL SEEDING ---
// ============================================================

/// <summary>
/// Revisa la base de datos al arrancar y crea el SuperUsuario
/// (admin/admin) si no existe ninguno.
/// </summary>
async Task SeedSuperUserAsync(WebApplication app)
{
    // 1. Obtenemos los "servicios" (como el DbContext)
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<GymContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            // 2. Revisamos si ya existe un SuperUsuario
            if (!await context.User.AnyAsync(u => u.Role == UserRole.SuperUser))
            {
                logger.LogWarning("No se encontró SuperUsuario. Creando usuario 'admin'...");

                // 3. Si no existe, creamos uno
                var (hash, salt) = PasswordHelper.HashPassword("admin");

                var superUser = new User
                {
                    userName = "admin",
                    email = "admin@gym.com",
                    name = "Administrador",
                    lastName = "Principal",
                    passwordHash = hash,
                    salt = salt,
                    Role = UserRole.SuperUser,
                    active = true,
                    lastAccess = DateTime.UtcNow,
                    branchId = null // El SuperUser no tiene sede
                };

                context.User.Add(superUser);
                await context.SaveChangesAsync();
                
                logger.LogInformation("Usuario 'admin' creado exitosamente.");
            }
            else
            {
                logger.LogInformation("El SuperUsuario ya existe. Se omite el seeding.");
            }
        }
        catch (Exception ex)
        {
            // Manejo de error (ej: si la BD no está creada)
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocurrió un error durante el seeding del SuperUsuario.");
        }
    }
}