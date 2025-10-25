using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;

namespace SuMejorPeso.Controllers
{
    /// <summary>
    /// Controlador para las operaciones CRUD de las Actividades del gimnasio
    /// (ej: Clases, Rutinas).
    /// </summary>
    [Authorize(Roles = "SuperUser")]
    public class AssignmentsController : Controller
    {
        private readonly GymContext _context;

        public AssignmentsController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /assignment/
        // Muestra la lista de actividades ACTIVAS
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var activities = await _context.Assignment // <-- Usa el DbSet 'assignment'
                .Where(a => a.active) 
                .ToListAsync();

            return View(activities);
        }

        // ============================================================
        // GET: /assignment/Inactive
        // Muestra la lista de actividades INACTIVAS
        // ============================================================
        public async Task<IActionResult> Inactive()
        {
            var activities = await _context.Assignment
                .Where(a => !a.active) 
                .ToListAsync();
            
            return View(activities); 
        }

        // ============================================================
        // GET: /assignment/Details/5
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignment
                .FirstOrDefaultAsync(a => a.id == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // ============================================================
        // GET: /assignment/Create
        // ============================================================
        public IActionResult Create()
        {
            return View();
        }

        // ============================================================
        // POST: /assignment/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            if (!ModelState.IsValid)
            {
                return View(assignment);
            }

            // Validación por nombre
            var existingActivity = await _context.Assignment
                .FirstOrDefaultAsync(a => a.name.ToLower() == assignment.name.ToLower()); 

            if (existingActivity != null)
            {
                if (existingActivity.active)
                {
                    ModelState.AddModelError(nameof(assignment.name), "Ya existe una actividad ACTIVA con ese nombre.");
                    return View(assignment);
                }
                else
                {
                    return RedirectToAction("ReactivateConfirmation", new { id = existingActivity.id });
                }
            }
            
            assignment.active = true;
            _context.Assignment.Add(assignment);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /assignment/Edit/5
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignment.FindAsync(id);
            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // ============================================================
        // POST: /assignment/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Assignment assignment)
        {
            if (id != assignment.id) return NotFound();

            if (!ModelState.IsValid)
                return View(assignment);

            try
            {
                _context.Update(assignment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(assignment.id)) 
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
        // GET: /assignment/Delete/5
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignment
                .FirstOrDefaultAsync(a => a.id == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // ============================================================
        // POST: /assignment/Delete/5
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignment.FindAsync(id);
            
            if (assignment != null)
            {
                assignment.active = false; 
                _context.Update(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        // ============================================================
        // GET: /assignment/ReactivateConfirmation/5
        // ============================================================
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.Assignment.FindAsync(id);

            if (assignment == null || assignment.active) 
                return NotFound();

            return View("ReactivateConfirmation", assignment);
        }
        
        // ============================================================
        // POST: /assignment/Reactivate
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var assignment = await _context.Assignment.FindAsync(id);
            
            if (assignment == null) return NotFound();

            assignment.active = true;
            _context.Update(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = assignment.id });
        }

        // ============================================================
        // MÉTODO AUXILIAR (privado)
        // ============================================================
        private bool ActivityExists(int id) 
        {
            return _context.Assignment.Any(e => e.id == id);
        }
    }
}