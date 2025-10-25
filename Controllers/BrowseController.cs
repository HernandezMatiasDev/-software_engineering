using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization; // Para permitir acceso

namespace SuMejorPeso.Controllers
{
    // Permitir acceso a usuarios logueados (defaults y members)
    [Authorize(Roles = "defaults, member")]
    public class BrowseController : Controller
    {
        private readonly GymContext _context;

        public BrowseController(GymContext context)
        {
            _context = context;
        }

        // GET: /Browse/Branches
        // Muestra la lista de Sedes activas
        public async Task<IActionResult> Branches()
        {
            var branches = await _context.Branch
                .Where(b => b.active)
                .OrderBy(b => b.name)
                .ToListAsync();
            return View(branches);
        }

        // GET: /Browse/Classes/{branchId}
        // Muestra las clases activas para una Sede específica
        public async Task<IActionResult> Classes(int? id) // Recibe el ID de la Sede
        {
            if (id == null)
            {
                // Si no hay ID, redirigir a la selección de sedes
                return RedirectToAction(nameof(Branches));
            }

            var branch = await _context.Branch.FindAsync(id);
            if (branch == null || !branch.active)
            {
                return NotFound("Sede no encontrada o inactiva.");
            }

            // En Controllers/BrowseController.cs -> Classes(id)
            var classes = await _context.Classrooms
                .Include(c => c.assignment)
                .Include(c => c.coaches)
                .Include(c => c.members)
                .Where(c => c.branchId == id && c.active)
                .OrderBy(c => c.name)
                .ToListAsync();

            ViewBag.BranchName = branch.name; // Pasa el nombre de la sede a la vista
            return View(classes);
        }
    }
}