using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SuMejorPeso.Controllers
{
    [Authorize(Roles = "SuperUser, Manager, Administrator, member")] // <-- Permisos adecuados
    public class ClassroomsController : Controller
    {
        private readonly GymContext _context;

        public ClassroomsController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Classrooms/
        // Muestra la lista de clases ACTIVAS (filtrada por Sede si no es SuperUser)
        // ============================================================
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Index()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userBranchIdClaim = User.Claims.FirstOrDefault(c => c.Type == "BranchId")?.Value;

            var query = _context.Classrooms
                .Include(c => c.branch)   // Carga Sede
                .Include(c => c.assignment) // Carga Actividad
                .Where(c => c.active);

            // --- Filtro por Sede ---
            if (userRole != "SuperUser" && int.TryParse(userBranchIdClaim, out int branchId))
            {
                query = query.Where(c => c.branchId == branchId);
            }
            else if (userRole != "SuperUser")
            {
                query = query.Where(c => false); // Error si no tiene sede
            }
            // --- Fin Filtro ---

            var classrooms = await query.ToListAsync();
            return View(classrooms);
        }

        // ============================================================
        // GET: /Classrooms/Inactive
        // ============================================================
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Inactive()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userBranchIdClaim = User.Claims.FirstOrDefault(c => c.Type == "BranchId")?.Value;

            var query = _context.Classrooms
                .Include(c => c.branch)
                .Include(c => c.assignment)
                .Where(c => !c.active);

            if (userRole != "SuperUser" && int.TryParse(userBranchIdClaim, out int branchId))
            {
                query = query.Where(c => c.branchId == branchId);
            }
            else if (userRole != "SuperUser")
            {
                query = query.Where(c => false);
            }

            var classrooms = await query.ToListAsync();
            return View(classrooms);
        }

        // ============================================================
        // GET: /Classrooms/Details/5
        // ============================================================
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var classroom = await _context.Classrooms
                .Include(c => c.branch)
                .Include(c => c.assignment)
                .Include(c => c.coaches) // Carga Profesores asignados
                .Include(c => c.members) // Carga Miembros inscritos
                .FirstOrDefaultAsync(c => c.id == id);

            // TODO: Añadir lógica de seguridad para que Manager/Admin solo vean detalles de su sede

            if (classroom == null) return NotFound();

            return View(classroom);
        }

        // ============================================================
        // GET: /Classrooms/Create
        // ============================================================
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData(null, null); // Carga Sedes, Actividades, Coaches, Miembros
            return View();
        }

        // ============================================================
        // POST: /Classrooms/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Create(Classroom classroom, List<int> selectedCoaches, List<int> selectedMembers)
        {
            // Remover props de navegación para evitar errores de validación
            ModelState.Remove(nameof(classroom.branch));
            ModelState.Remove(nameof(classroom.assignment));
            ModelState.Remove(nameof(classroom.coaches));
            ModelState.Remove(nameof(classroom.members));
            ModelState.Remove(nameof(classroom.schedule));

            // --- Lógica para Asignar Sede Automáticamente (si no es SuperUser) ---
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != "SuperUser")
            {
                var userBranchIdClaim = User.Claims.FirstOrDefault(c => c.Type == "BranchId")?.Value;
                if (int.TryParse(userBranchIdClaim, out int branchId))
                {
                    classroom.branchId = branchId; // Sobreescribe o asigna la sede del Manager/Admin
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No se pudo determinar la sede del usuario.");
                }
            }
            // --- Fin Lógica Sede ---

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(classroom.branchId, classroom.assignmentId, selectedCoaches, selectedMembers);
                return View(classroom);
            }

            // --- Validación de Duplicados (Nombre dentro de la misma Sede) ---
            var existingClassroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.name.ToLower() == classroom.name.ToLower()
                                       && c.branchId == classroom.branchId);

            if (existingClassroom != null)
            {
                if (existingClassroom.active)
                {
                    ModelState.AddModelError(nameof(classroom.name), "Ya existe una clase ACTIVA con ese nombre en esta sede.");
                    await LoadDropdownData(classroom.branchId, classroom.assignmentId, selectedCoaches, selectedMembers);
                    return View(classroom);
                }
                else
                {
                    return RedirectToAction("ReactivateConfirmation", new { id = existingClassroom.id });
                }
            }

            // --- Lógica de Relación M-M (Coaches y Members) ---
            if (selectedCoaches != null)
            {
                var coaches = await _context.Coach
                    .Where(c => selectedCoaches.Contains(c.id))
                    .ToListAsync();
                classroom.coaches = coaches;
            }
            if (selectedMembers != null)
            {
                var members = await _context.Member
                   .Where(m => selectedMembers.Contains(m.id))
                   .ToListAsync();
                classroom.members = members;
            }

            classroom.active = true;
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /Classrooms/Edit/5
        // ============================================================
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var classroom = await _context.Classrooms
                .Include(c => c.coaches) // Cargar coaches actuales
                .Include(c => c.members) // Cargar members actuales
                .FirstOrDefaultAsync(c => c.id == id);
                
            if (classroom == null) return NotFound();

            // TODO: Lógica de seguridad para Manager/Admin
            
            await LoadDropdownData(classroom.branchId, classroom.assignmentId,
                                   classroom.coaches.Select(c => c.id).ToList(),
                                   classroom.members.Select(m => m.id).ToList());
            return View(classroom);
        }

        // ============================================================
        // POST: /Classrooms/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Edit(int id, Classroom classroomFromForm, List<int> selectedCoaches, List<int> selectedMembers)
        {
            if (id != classroomFromForm.id) return NotFound();

            // TODO: Lógica de seguridad para Manager/Admin

            ModelState.Remove(nameof(classroomFromForm.branch));
            ModelState.Remove(nameof(classroomFromForm.assignment));
            //... (quita otras props de navegación si dan error)

            // --- Lógica para Mantener Sede si no es SuperUser ---
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != "SuperUser")
            {
                var userBranchIdClaim = User.Claims.FirstOrDefault(c => c.Type == "BranchId")?.Value;
                if (int.TryParse(userBranchIdClaim, out int branchId))
                {
                    // Nos aseguramos que no puedan cambiar la clase a otra sede
                    classroomFromForm.branchId = branchId;
                }
            }
            // --- Fin Lógica Sede ---

            // --- Validación de duplicados (Nombre dentro de la misma Sede) en Edit ---
            var conflictingClassroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.name.ToLower() == classroomFromForm.name.ToLower()
                                       && c.branchId == classroomFromForm.branchId
                                       && c.id != id); // Excluirse a sí mismo

            if (conflictingClassroom != null)
            {
                ModelState.AddModelError(nameof(classroomFromForm.name), "Ese nombre ya está en uso por otra clase en esta sede.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(classroomFromForm.branchId, classroomFromForm.assignmentId, selectedCoaches, selectedMembers);
                return View(classroomFromForm);
            }

            try
            {
                var classroomToUpdate = await _context.Classrooms
                    .Include(c => c.coaches)
                    .Include(c => c.members)
                    .FirstOrDefaultAsync(c => c.id == id);

                if (classroomToUpdate == null) return NotFound();

                // Actualizar datos simples
                classroomToUpdate.name = classroomFromForm.name;
                classroomToUpdate.description = classroomFromForm.description;
                classroomToUpdate.limitedPlace = classroomFromForm.limitedPlace;
                classroomToUpdate.assignmentId = classroomFromForm.assignmentId;
                classroomToUpdate.branchId = classroomFromForm.branchId; // Ya validado arriba
                classroomToUpdate.active = classroomFromForm.active;

                // Actualizar M-M Coaches
                classroomToUpdate.coaches.Clear();
                if (selectedCoaches != null)
                {
                    var coaches = await _context.Coach.Where(c => selectedCoaches.Contains(c.id)).ToListAsync();
                    classroomToUpdate.coaches = coaches;
                }
                // Actualizar M-M Members
                classroomToUpdate.members.Clear();
                if (selectedMembers != null)
                {
                    var members = await _context.Member.Where(m => selectedMembers.Contains(m.id)).ToListAsync();
                    classroomToUpdate.members = members;
                }

                _context.Update(classroomToUpdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassroomExists(classroomFromForm.id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // MÉTODOS DE BORRADO LÓGICO (Delete / Reactivate)
        // (Son idénticos a los de otros controladores)
        // ============================================================
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var classroom = await _context.Classrooms
                .Include(c => c.branch)       
                .Include(c => c.assignment)   
                .FirstOrDefaultAsync(m => m.id == id);             
            if (classroom == null) return NotFound();
            return View(classroom);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
             // TODO: Seguridad Manager/Admin
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom != null)
            {
                classroom.active = false; 
                _context.Update(classroom);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();
            var classroom = await _context.Classrooms.FindAsync(id);
             // TODO: Seguridad Manager/Admin
            if (classroom == null || classroom.active)
                return NotFound();
            return View("ReactivateConfirmation", classroom);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUser, Manager, Administrator")]
        public async Task<IActionResult> Reactivate(int id)
        {
             // TODO: Seguridad Manager/Admin
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null) return NotFound();
            
            classroom.active = true;
            _context.Update(classroom);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", new { id = classroom.id });
        }

        // ============================================================
        // MÉTODOS AUXILIARES (privados)
        // ============================================================
        
        /// <summary>
        /// Carga Sedes, Actividades, Coaches y Miembros para los dropdowns/listas.
        /// Filtra por Sede si el usuario no es SuperUser.
        /// </summary>
        private async Task LoadDropdownData(int? selectedBranchId, int? selectedActivityId, 
                                            List<int> selectedCoaches = null, List<int> selectedMembers = null)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userBranchIdClaim = User.Claims.FirstOrDefault(c => c.Type == "BranchId")?.Value;
            int? currentBranchId = null;
            if (int.TryParse(userBranchIdClaim, out int branchId))
            {
                currentBranchId = branchId;
            }

            // 1. Cargar Sedes (solo la del usuario si no es SuperUser)
            IQueryable<Branch> branchesQuery = _context.Branch.Where(b => b.active);
            if (userRole != "SuperUser" && currentBranchId.HasValue)
            {
                branchesQuery = branchesQuery.Where(b => b.id == currentBranchId.Value);
            }
            var branches = await branchesQuery.ToListAsync();
            ViewBag.BranchList = new SelectList(branches, "id", "name", selectedBranchId ?? currentBranchId); // Preselecciona la del usuario

            // 2. Cargar Actividades (solo activas)
            var activities = await _context.Assignment.Where(a => a.active).ToListAsync();
            ViewBag.ActivityList = new SelectList(activities, "id", "name", selectedActivityId);

            // 3. Cargar Coaches (solo activos y de la sede correspondiente si no es SuperUser)
            IQueryable<Coach> coachesQuery = _context.Coach.Where(c => c.active);
            if (userRole != "SuperUser" && currentBranchId.HasValue)
            {
                 coachesQuery = coachesQuery.Where(c => c.branchId == currentBranchId.Value);
            }
             else if (userRole != "SuperUser") // Si no tiene sede asignada, no muestra coaches
            {
                coachesQuery = coachesQuery.Where(c => false);
            }
            var coaches = await coachesQuery.ToListAsync();
            ViewBag.CoachList = new MultiSelectList(coaches, "id", "name", selectedCoaches);

             // 4. Cargar Miembros (solo activos) - Asumimos que miembros pueden ir a cualquier sede
             var members = await _context.Member.Where(m => m.active).ToListAsync();
             ViewBag.MemberList = new MultiSelectList(members, "id", "name", selectedMembers); // Podrías querer mostrar Apellido también
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "member")] // <-- ONLY members can enroll
        public async Task<IActionResult> Enroll(int classroomId)
        {
            // 1. Get Logged-in User's ID
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int currentUserId))
            {
                // Should not happen if logged in, but good practice
                return RedirectToAction("Logout", "Account");
            }

            // 2. Find the Member profile linked to this User
            var member = await _context.Member.FirstOrDefaultAsync(m => m.userId == currentUserId && m.active);
            if (member == null)
            {
                // User is 'member' role but has no active Member profile? Error.
                TempData["ErrorMessage"] = "No se encontró tu perfil de miembro activo.";
                return RedirectToAction("Index", "Home"); // Or MyProfile/NoProfile
            }

            // 3. Find the Classroom, including current members
            var classroom = await _context.Classrooms
                .Include(c => c.members) // NEED to load current members
                .FirstOrDefaultAsync(c => c.id == classroomId && c.active);

            if (classroom == null)
            {
                TempData["ErrorMessage"] = "La clase seleccionada no existe o no está activa.";
                // Redirect back to where they came from (e.g., the class list for that branch)
                // You might need to pass the branchId back if you have it
                return RedirectToAction("Branches", "Browse"); 
            }

            // --- VALIDATIONS ---

            // 4. Check if class is full
            if (classroom.members.Count >= classroom.limitedPlace)
            {
                TempData["ErrorMessage"] = $"La clase '{classroom.name}' está llena (Cupo: {classroom.limitedPlace}).";
                return RedirectToAction("Classes", "Browse", new { id = classroom.branchId }); // Go back to class list
            }

            // 5. Check if member is already enrolled
            if (classroom.members.Any(m => m.id == member.id))
            {
                TempData["SuccessMessage"] = $"Ya estás inscrito en la clase '{classroom.name}'."; // Use SuccessMessage for info
                return RedirectToAction("Classes", "Browse", new { id = classroom.branchId }); // Go back to class list
            }
            
            // TODO: Add Schedule Conflict Validation here later

            // --- END VALIDATIONS ---


            // 6. Enroll the member
            classroom.members.Add(member);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"¡Te has inscrito exitosamente en '{classroom.name}'!";
            // Redirect to a "My Classes" page (which we need to create) or back to the list
            // For now, let's go back to the browse list for that branch
             return RedirectToAction("Classes", "Browse", new { id = classroom.branchId });
        }
        private bool ClassroomExists(int id) 
        {
            return _context.Classrooms.Any(e => e.id == id);
        }
    }
}