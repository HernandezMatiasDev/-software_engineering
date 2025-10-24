using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;


namespace SuMejorPeso.Controllers
{
    /// <summary>
    /// Controlador encargado de las operaciones CRUD (Alta, Baja y Modificación)
    /// de las sedes (branches) del gimnasio.
    /// </summary>
    [Authorize(Roles = "SuperUser")]
    public class BranchesController : Controller
    {
        private readonly GymContext _context;

        public BranchesController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Branch/
        // Muestra la lista completa de sedes ACTIVAS
        // ============================================================
        public async Task<IActionResult> Index()
        {
            // Filtrar solo a las sedes activas (active == true)
            var branch = await _context.Branch // <-- CORREGIDO: Usa tu DbSet singular
                .Where(b => b.active) // <-- CORREGIDO: Filtro agregado
                .ToListAsync();

            return View(branch);
        }

        // ============================================================
        // GET: /Branch/Inactive
        // Muestra la lista de sedes INACTIVAS
        // ============================================================
        public async Task<IActionResult> Inactive()
        {
            // Filtrar solo a las sedes inactivas (active == false)
            var branch = await _context.Branch // <-- CORREGIDO: Usa tu DbSet singular
                .Where(b => !b.active) // <-- CORREGIDO: Filtro agregado
                .ToListAsync();
            
            return View(branch); 
        }
        // ============================================================
        // GET: /Branch/Details/5
        // Muestra información detallada de una sede
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branch.FirstOrDefaultAsync(b => b.id == id); // <-- Tu código estaba bien

            if (branch == null) return NotFound();

            return View(branch);
        }

        // ============================================================
        // GET: /Branch/Create
        // Muestra el formulario para crear una nueva sede
        // ============================================================
        public IActionResult Create()
        {
            return View();
        }

        // ============================================================
        // POST: /Branch/Create
        // Recibe los datos del formulario y crea la sede en la BD
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {
            if (!ModelState.IsValid)
            {
                return View(branch);
            }

            // --- CORRECCIÓN LÓGICA ---
            // Validación por 'name' (nombre) en lugar de 'address' (dirección).
            var existingBranch = await _context.Branch
                .FirstOrDefaultAsync(b => b.name.ToLower() == branch.name.ToLower()); 

            if (existingBranch != null)
            {
                if (existingBranch.active)
                {
                    // Error: Ya existe una sede ACTIVA con ese nombre.
                    ModelState.AddModelError(nameof(branch.name), "Ya existe una sede ACTIVA con ese nombre.");
                    return View(branch);
                }
                else
                {
                    // Sede inactiva: Redirigimos a la confirmación de reactivación.
                    return RedirectToAction("ReactivateConfirmation", new { id = existingBranch.id });
                }
            }
            
            branch.active = true;
            _context.Branch.Add(branch); // <-- Tu código estaba bien
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /Branch/Edit/5
        // Muestra el formulario para editar una sede existente
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branch.FindAsync(id); // <-- Tu código estaba bien
            if (branch == null) return NotFound();

            return View(branch);
        }

        // ============================================================
        // POST: /Branch/Edit/5
        // Recibe los cambios del formulario y actualiza la sede
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Branch branch)
        {
            if (id != branch.id) return NotFound();

            if (!ModelState.IsValid)
                return View(branch);

            try
            {
                _context.Update(branch);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // <-- CORRECCIÓN LÓGICA: Renombrar método auxiliar
                if (!BranchExists(branch.id)) 
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
        // GET: /Branch/Delete/5
        // Muestra confirmación antes de eliminar (desactivar) una sede
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branch
                .FirstOrDefaultAsync(m => m.id == id); // <-- Tu código estaba bien

            if (branch == null) return NotFound();

            return View(branch);
        }

        // ============================================================
        // POST: /Branch/Delete/5
        // Ejecuta la eliminación LÓGICA (Soft Delete)
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branch.FindAsync(id); // <-- Tu código estaba bien
            
            if (branch != null)
            {
                branch.active = false; 
                _context.Update(branch);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        // ============================================================
        // GET: /Branch/ReactivateConfirmation/5
        // Muestra el formulario para confirmar reactivación
        // ============================================================
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branch.FindAsync(id); // <-- Tu código estaba bien

            if (branch == null || branch.active) // No debe ser activo ni nulo
                return NotFound();

            return View("ReactivateConfirmation", branch);
        }
        
        // ============================================================
        // POST: /Branch/Reactivate
        // Ejecuta la reactivación de la sede
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var branch = await _context.Branch.FindAsync(id); // <-- Tu código estaba bien
            
            if (branch == null) return NotFound();

            branch.active = true;
            _context.Update(branch);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = branch.id });
        }

        // ============================================================
        // MÉTODO AUXILIAR (privado)
        // ============================================================
        // <-- CORRECCIÓN LÓGICA: Renombrar método auxiliar
        private bool BranchExists(int id) 
        {
            return _context.Branch.Any(e => e.id == id); // <-- Tu código estaba bien
        }
    }
}