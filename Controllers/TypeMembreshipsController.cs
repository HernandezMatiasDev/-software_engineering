using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;

namespace SuMejorPeso.Controllers
{
    /// <summary>
    /// Controlador para las operaciones CRUD (Alta, Baja y Modificación)
    /// de los Tipos de Membresía (ej: Plan Oro, Plan Plata).
    /// </summary>
    /// 
    [Authorize(Roles = "SuperUser, Manager, Administrator")]
    public class TypeMembreshipsController : Controller
    {
        private readonly GymContext _context;

        public TypeMembreshipsController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /TypeMembreships/
        // Muestra la lista de tipos de membresía ACTIVOS
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var types = await _context.TypeMembreship
                .Where(t => t.active) 
                .ToListAsync();

            return View(types);
        }

        // ============================================================
        // GET: /TypeMembreships/Inactive
        // Muestra la lista de tipos de membresía INACTIVOS
        // ============================================================
        public async Task<IActionResult> Inactive()
        {
            var types = await _context.TypeMembreship
                .Where(t => !t.active) 
                .ToListAsync();
            
            return View(types); 
        }

        // ============================================================
        // GET: /TypeMembreships/Details/5
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var typeMembreship = await _context.TypeMembreship
                .FirstOrDefaultAsync(t => t.id == id);

            if (typeMembreship == null) return NotFound();

            return View(typeMembreship);
        }

        // ============================================================
        // GET: /TypeMembreships/Create
        // ============================================================
        public IActionResult Create()
        {
            return View();
        }

        // ============================================================
        // POST: /TypeMembreships/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TypeMembreship typeMembreship)
        {
            if (!ModelState.IsValid)
            {
                return View(typeMembreship);
            }

            // Validación por nombre
            var existingType = await _context.TypeMembreship
                .FirstOrDefaultAsync(t => t.name.ToLower() == typeMembreship.name.ToLower()); 

            if (existingType != null)
            {
                if (existingType.active)
                {
                    ModelState.AddModelError(nameof(typeMembreship.name), "Ya existe un tipo de membresía ACTIVO con ese nombre.");
                    return View(typeMembreship);
                }
                else
                {
                    return RedirectToAction("ReactivateConfirmation", new { id = existingType.id });
                }
            }
            
            typeMembreship.active = true;
            _context.TypeMembreship.Add(typeMembreship);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /TypeMembreships/Edit/5
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var typeMembreship = await _context.TypeMembreship.FindAsync(id);
            if (typeMembreship == null) return NotFound();

            return View(typeMembreship);
        }

        // ============================================================
        // POST: /TypeMembreships/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TypeMembreship typeMembreship)
        {
            if (id != typeMembreship.id) return NotFound();

            if (!ModelState.IsValid)
                return View(typeMembreship);

            try
            {
                _context.Update(typeMembreship);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeMembreshipExists(typeMembreship.id)) 
                {
                    return NotFound();
                }
                else
                {
                    throw; 
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /TypeMembreships/Delete/5
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var typeMembreship = await _context.TypeMembreship
                .FirstOrDefaultAsync(m => m.id == id);

            if (typeMembreship == null) return NotFound();

            return View(typeMembreship);
        }

        // ============================================================
        // POST: /TypeMembreships/Delete/5
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var typeMembreship = await _context.TypeMembreship.FindAsync(id);
            
            if (typeMembreship != null)
            {
                typeMembreship.active = false; 
                _context.Update(typeMembreship);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        // ============================================================
        // GET: /TypeMembreships/ReactivateConfirmation/5
        // ============================================================
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();

            var typeMembreship = await _context.TypeMembreship.FindAsync(id);

            if (typeMembreship == null || typeMembreship.active) 
                return NotFound();

            return View("ReactivateConfirmation", typeMembreship);
        }
        
        // ============================================================
        // POST: /TypeMembreships/Reactivate
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var typeMembreship = await _context.TypeMembreship.FindAsync(id);
            
            if (typeMembreship == null) return NotFound();

            typeMembreship.active = true;
            _context.Update(typeMembreship);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = typeMembreship.id });
        }

        // ============================================================
        // MÉTODO AUXILIAR (privado)
        // ============================================================
        private bool TypeMembreshipExists(int id) 
        {
            return _context.TypeMembreship.Any(e => e.id == id);
        }
    }
}