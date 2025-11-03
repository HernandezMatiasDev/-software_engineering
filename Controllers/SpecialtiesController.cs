using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;

namespace SuMejorPeso.Controllers
{
    /// <summary>
    /// Controlador para las operaciones CRUD de las Especialidades de los profesores
    /// (ej: Yoga, CrossFit, Funcional).
    /// </summary>
    [Authorize(Roles = "SuperUser, Manager, Administrator")]
    public class SpecialtiesController : Controller
    {
        private readonly GymContext _context;

        public SpecialtiesController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Specialties/
        // Muestra la lista de especialidades ACTIVAS
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var specialties = await _context.Specialty
                .Where(s => s.active) 
                .ToListAsync();

            return View(specialties);
        }

        // ============================================================
        // GET: /Specialties/Inactive
        // Muestra la lista de especialidades INACTIVAS
        // ============================================================
        public async Task<IActionResult> Inactive()
        {
            var specialties = await _context.Specialty
                .Where(s => !s.active) 
                .ToListAsync();
            
            return View(specialties); 
        }

        // ============================================================
        // GET: /Specialties/Details/5
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Incluimos la lista de coaches que tienen esta especialidad
            var specialty = await _context.Specialty
                .Include(s => s.coaches) 
                .FirstOrDefaultAsync(s => s.id == id);

            if (specialty == null) return NotFound();

            return View(specialty);
        }

        // ============================================================
        // GET: /Specialties/Create
        // ============================================================
        public IActionResult Create()
        {
            return View();
        }

        // ============================================================
        // POST: /Specialties/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Specialty specialty)
        {
            if (!ModelState.IsValid)
            {
                return View(specialty);
            }

            // Validación por nombre
            var existingSpecialty = await _context.Specialty
                .FirstOrDefaultAsync(s => s.name.ToLower() == specialty.name.ToLower()); 

            if (existingSpecialty != null)
            {
                if (existingSpecialty.active)
                {
                    ModelState.AddModelError(nameof(specialty.name), "Ya existe una especialidad ACTIVA con ese nombre.");
                    return View(specialty);
                }
                else
                {
                    return RedirectToAction("ReactivateConfirmation", new { id = existingSpecialty.id });
                }
            }
            
            specialty.active = true;
            _context.Specialty.Add(specialty);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /Specialties/Edit/5
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var specialty = await _context.Specialty.FindAsync(id);
            if (specialty == null) return NotFound();

            return View(specialty);
        }

        // ============================================================
        // POST: /Specialties/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Specialty specialty)
        {
            if (id != specialty.id) return NotFound();
            
            // Removemos la validación de la lista de coaches
            ModelState.Remove(nameof(specialty.coaches));

            if (!ModelState.IsValid)
                return View(specialty);

            try
            {
                _context.Update(specialty);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpecialtyExists(specialty.id)) 
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
        // GET: /Specialties/Delete/5
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var specialty = await _context.Specialty
                .FirstOrDefaultAsync(s => s.id == id);

            if (specialty == null) return NotFound();

            return View(specialty);
        }

        // ============================================================
        // POST: /Specialties/Delete/5
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialty = await _context.Specialty.FindAsync(id);
            
            if (specialty != null)
            {
                specialty.active = false; 
                _context.Update(specialty);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        // ============================================================
        // GET: /Specialties/ReactivateConfirmation/5
        // ============================================================
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();

            var specialty = await _context.Specialty.FindAsync(id);

            if (specialty == null || specialty.active) 
                return NotFound();

            return View("ReactivateConfirmation", specialty);
        }
        
        // ============================================================
        // POST: /Specialties/Reactivate
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var specialty = await _context.Specialty.FindAsync(id);
            
            if (specialty == null) return NotFound();

            specialty.active = true;
            _context.Update(specialty);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = specialty.id });
        }

        // ============================================================
        // MÉTODO AUXILIAR (privado)
        // ============================================================
        private bool SpecialtyExists(int id) 
        {
            return _context.Specialty.Any(e => e.id == id);
        }
    }
}