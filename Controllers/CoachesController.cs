// En SuMejorPeso/Controllers/CoachesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;

namespace SuMejorPeso.Controllers
{
    [Authorize(Roles = "SuperUser, Manager, Administrator")]
    public class CoachesController : Controller
    {
        private readonly GymContext _context;

        public CoachesController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Coaches/
        // Muestra la lista completa de profesores ACTIVOS
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var coaches = await _context.Coach
                .Include(c => c.branch) // Carga la Sede
                .Include(c => c.user)   // Carga el Usuario
                .Where(c => c.active) 
                .ToListAsync();

            return View(coaches);
        }

        // ============================================================
        // GET: /Coaches/Inactive
        // Muestra la lista de profesores INACTIVOS
        // ============================================================
        public async Task<IActionResult> Inactive()
        {
            var coaches = await _context.Coach
                .Include(c => c.branch)
                .Include(c => c.user)
                .Where(c => !c.active) 
                .ToListAsync();
            
            return View(coaches); 
        }

        // ============================================================
        // GET: /Coaches/Details/5
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var coach = await _context.Coach
                .Include(c => c.branch)
                .Include(c => c.user)
                .Include(c => c.specialties) // Carga la lista de especialidades
                .FirstOrDefaultAsync(c => c.id == id);

            if (coach == null) return NotFound();

            return View(coach);
        }

        // ============================================================
        // GET: /Coaches/Create
        // Muestra el formulario para crear un nuevo profesor
        // ============================================================
        public async Task<IActionResult> Create()
        {
            // Cargar datos para los 3 dropdowns
            await LoadDropdownData(null);
            return View();
        }

        // ============================================================
        // POST: /Coaches/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Recibe el coach Y la lista de IDs de especialidades seleccionadas
        public async Task<IActionResult> Create(Coach coach, List<int> selectedSpecialties)
        {
            ModelState.Remove(nameof(coach.user));
            ModelState.Remove(nameof(coach.branch));
            ModelState.Remove(nameof(coach.specialties));
            ModelState.Remove(nameof(coach.classrooms));
            ModelState.Remove(nameof(coach.schedule));

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(coach.branchId);
                return View(coach);
            }
            
            // --- Validación de Duplicados (DNI y Email) ---
            var normalizedDni = coach.dni;
            var normalizedEmail = coach.email.ToLower();

            var existingCoach = await _context.Coach
                .FirstOrDefaultAsync(c => c.dni == normalizedDni || c.email.ToLower() == normalizedEmail); 

            if (existingCoach != null)
            {
                if (existingCoach.active)
                {
                    if (existingCoach.dni == normalizedDni)
                        ModelState.AddModelError(nameof(coach.dni), "Ya existe un profesor ACTIVO con ese DNI.");
                    if (existingCoach.email.ToLower() == normalizedEmail)
                        ModelState.AddModelError(nameof(coach.email), "Ya existe un profesor ACTIVO con ese email.");
                    
                    await LoadDropdownData(coach.branchId);
                    return View(coach);
                }
                else
                {
                    return RedirectToAction("ReactivateConfirmation", new { id = existingCoach.id });
                }
            }
            
            // --- Lógica de Relación M-M (Especialidades) ---
            if (selectedSpecialties != null)
            {
                // Busca las entidades 'Specialty' que coincidan con los IDs
                var specialties = await _context.Specialty
                    .Where(s => selectedSpecialties.Contains(s.id))
                    .ToListAsync();
                
                coach.specialties = specialties; // Asigna la lista de entidades
            }
            
            coach.active = true;
            _context.Coach.Add(coach);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /Coaches/Edit/5
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var coach = await _context.Coach
                .Include(c => c.specialties) // Cargar especialidades actuales
                .FirstOrDefaultAsync(c => c.id == id);
                
            if (coach == null) return NotFound();

            // Cargar datos para los dropdowns
            await LoadDropdownData(coach.branchId, coach.userId, coach.specialties.Select(s => s.id).ToList());
            return View(coach);
        }

        // ============================================================
        // POST: /Coaches/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coach coachFromForm, List<int> selectedSpecialties)
        {
            if (id != coachFromForm.id) return NotFound();

            ModelState.Remove(nameof(coachFromForm.user));
            ModelState.Remove(nameof(coachFromForm.branch));
            //... (quita otras props de navegación si dan error)

            // --- Validación de duplicados (DNI y Email) en Edit ---
            var normalizedDni = coachFromForm.dni;
            var normalizedEmail = coachFromForm.email.ToLower();

            var conflictingCoach = await _context.Coach
                .FirstOrDefaultAsync(c => (c.dni == normalizedDni || c.email.ToLower() == normalizedEmail) && c.id != id);

            if (conflictingCoach != null)
            {
                if (conflictingCoach.dni == normalizedDni)
                    ModelState.AddModelError(nameof(coachFromForm.dni), "Ese DNI ya está en uso por otro profesor.");
                if (conflictingCoach.email.ToLower() == normalizedEmail)
                    ModelState.AddModelError(nameof(coachFromForm.email), "Ese email ya está en uso por otro profesor.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(coachFromForm.branchId, coachFromForm.userId, selectedSpecialties);
                return View(coachFromForm);
            }

            try
            {
                // 1. Obtener el coach original de la BD, INCLUYENDO sus especialidades
                var coachToUpdate = await _context.Coach
                    .Include(c => c.specialties)
                    .FirstOrDefaultAsync(c => c.id == id);
                    
                if (coachToUpdate == null) return NotFound();

                // 2. Actualizar datos simples (de Person y Coach)
                coachToUpdate.name = coachFromForm.name;
                coachToUpdate.lastName = coachFromForm.lastName;
                coachToUpdate.dni = coachFromForm.dni;
                coachToUpdate.phone = coachFromForm.phone;
                coachToUpdate.email = coachFromForm.email;
                coachToUpdate.branchId = coachFromForm.branchId;
                coachToUpdate.userId = coachFromForm.userId;
                coachToUpdate.state = coachFromForm.state;
                coachToUpdate.active = coachFromForm.active;

                // 3. Actualizar la relación M-M (Especialidades)
                coachToUpdate.specialties.Clear(); // Borra las antiguas
                if (selectedSpecialties != null)
                {
                    var specialties = await _context.Specialty
                        .Where(s => selectedSpecialties.Contains(s.id))
                        .ToListAsync();
                    coachToUpdate.specialties = specialties; // Asigna las nuevas
                }

                _context.Update(coachToUpdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoachExists(coachFromForm.id)) 
                    return NotFound();
                else
                    throw; 
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // MÉTODOS DE BORRADO LÓGICO (Delete / Reactivate)
        // (Son idénticos a los de UsersController y BranchesController)
        // ============================================================

        // GET: /Coaches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var coach = await _context.Coach.FirstOrDefaultAsync(m => m.id == id);
            if (coach == null) return NotFound();
            return View(coach);
        }

        // POST: /Coaches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coach = await _context.Coach.FindAsync(id);
            if (coach != null)
            {
                coach.active = false; 
                _context.Update(coach);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        // GET: /Coaches/ReactivateConfirmation/5
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();
            var coach = await _context.Coach.FindAsync(id);
            if (coach == null || coach.active)
                return NotFound();
            return View("ReactivateConfirmation", coach);
        }
        
        // POST: /Coaches/Reactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var coach = await _context.Coach.FindAsync(id);
            if (coach == null) return NotFound();
            
            coach.active = true;
            _context.Update(coach);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", new { id = coach.id });
        }

        // ============================================================
        // MÉTODOS AUXILIARES (privados)
        // ============================================================
        
        /// <summary>
        /// Carga los datos de Sedes, Usuarios (coach) y Especialidades
        /// para ser usados en dropdowns.
        /// </summary>
        private async Task LoadDropdownData(int? selectedBranchId, int? selectedUserId = null, List<int> selectedSpecialties = null)
        {
            // 1. Cargar Sedes (solo activas)
            var branches = await _context.Branch.Where(b => b.active).ToListAsync();
            ViewBag.BranchList = new SelectList(branches, "id", "name", selectedBranchId);

            // 2. Cargar Usuarios (solo activos, rol 'coach' y que NO tengan perfil)
            // Obtenemos los IDs de usuarios que YA están linkeados a un coach
            var linkedUserIds = await _context.Coach
                                    .Select(c => c.userId)
                                    .ToListAsync();

            var availableUsers = await _context.User
                .Where(u => u.active 
                            && u.Role == UserRole.coach 
                            && !linkedUserIds.Contains(u.id))
                .ToListAsync();
            
            // Si estamos editando, debemos añadir el usuario actual a la lista
            if (selectedUserId.HasValue)
            {
                var currentUser = await _context.User.FindAsync(selectedUserId.Value);
                if (currentUser != null && !availableUsers.Any(u => u.id == selectedUserId.Value))
                {
                    availableUsers.Add(currentUser);
                }
            }

            ViewBag.UserList = new SelectList(availableUsers, "id", "userName", selectedUserId);

            // 3. Cargar Especialidades (todas) para la lista M-M
            var specialties = await _context.Specialty.ToListAsync();
            ViewBag.SpecialtyList = new MultiSelectList(specialties, "id", "name", selectedSpecialties);
        }

        private bool CoachExists(int id) 
        {
            return _context.Coach.Any(e => e.id == id);
        }
    }
}