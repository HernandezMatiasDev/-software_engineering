using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Necesario para leer el ID del usuario

namespace SuMejorPeso.Controllers
{
    [Authorize(Roles = "member")] // <-- SOLO el rol 'member' puede entrar aquí
    public class MyProfileController : Controller
    {
        private readonly GymContext _context;

        public MyProfileController(GymContext context)
        {
            _context = context;
        }

        // GET: /MyProfile/
        public async Task<IActionResult> Index()
        {
            // 1. Obtener el ID del User que está logueado
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                // Si no podemos obtener el ID, lo mandamos al login
                return RedirectToAction("Logout", "Account");
            }

            // 2. Buscar el perfil de Miembro (Member) que está vinculado a este User
            // Usamos la propiedad 'userId' que heredaste de Person
            var memberProfile = await _context.Member
                .Include(m => m.license)     // Cargar la licencia
                .Include(m => m.membership)  // Cargar la membresía
                    .ThenInclude(mem => mem.type) // Cargar el *tipo* de membresía
                .FirstOrDefaultAsync(m => m.userId == currentUserId);

            // 3. Comprobar si el perfil existe
            if (memberProfile == null)
            {
                // El User tiene rol 'member' pero aún no ha comprado un plan.
                // Lo mandamos a una vista especial.
                return View("NoProfile");
            }

            // 4. Mostrar la vista "Index" con los datos de su perfil
            return View(memberProfile);
        }
    }
}