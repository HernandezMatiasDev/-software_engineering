using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using SuMejorPeso.Helpers;
using SuMejorPeso.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SuMejorPeso.Controllers
{
    public class AccountController : Controller
    {
        private readonly GymContext _context;

        public AccountController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // VISTA DE LOGIN (GET)
        // ============================================================
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ============================================================
        // VISTA DE REGISTRO (GET)
        // ============================================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ============================================================
        // ACCIÓN DE LOGOUT (POST)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ============================================================
        // ACCIÓN DE LOGIN (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Buscar al usuario por nombre de usuario
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.userName.ToLower() == model.UserName.ToLower());

            // 2. Validar que el usuario exista y esté activo
            if (user == null || !user.active)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }

            // 3. Verificar la contraseña usando el Helper
            bool isValidPassword = PasswordHelper.VerifyPassword(model.Password, user.passwordHash, user.salt);

            if (isValidPassword)
            {
                // 4. Actualizar el último acceso
                user.lastAccess = DateTime.UtcNow;
                _context.Update(user);
                await _context.SaveChangesAsync();

                // 5. Crear la "Identidad" (el ticket) del usuario
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                    new Claim(ClaimTypes.Name, user.userName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("BranchId", user.branchId?.ToString() ?? "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 6. Iniciar la sesión del usuario (crear la cookie)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // 7. Redirigir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        // ============================================================
        // ACCIÓN DE REGISTRO (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // --- Validación de Duplicados (Email y UserName) ---
            var normalizedUserName = model.UserName.ToLower();
            var normalizedEmail = model.Email.ToLower();

            var existingUser = await _context.User
                .FirstOrDefaultAsync(u => 
                    u.userName.ToLower() == normalizedUserName || 
                    u.email.ToLower() == normalizedEmail); 

            if (existingUser != null)
            {
                if (existingUser.userName.ToLower() == normalizedUserName)
                    ModelState.AddModelError(nameof(model.UserName), "Ese nombre de usuario ya está en uso.");
                if (existingUser.email.ToLower() == normalizedEmail)
                    ModelState.AddModelError(nameof(model.Email), "Ese email ya está en uso.");
                return View(model);
            }

            // --- Creación del Usuario ---
            
            // 1. Hashear la contraseña
            var (hash, salt) = PasswordHelper.HashPassword(model.Password);

            // 2. Crear la nueva entidad User
            var newUser = new User
            {
                userName = model.UserName,
                email = model.Email,
                name = model.Name,
                lastName = model.LastName,
                passwordHash = hash,
                salt = salt,
                Role = UserRole.member, // <-- REQUISITO: Rol 'member' automático
                active = true,
                lastAccess = DateTime.UtcNow,
                branchId = null // Los miembros no tienen sede asignada
            };

            // 3. Guardar en la BD
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            // 4. Iniciar sesión automáticamente
            var loginModel = new LoginViewModel { UserName = model.UserName, Password = model.Password };
            return await Login(loginModel, returnUrl: "/Home/Index");
        }
    }
}