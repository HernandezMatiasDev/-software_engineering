// En SuMejorPeso/Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using SuMejorPeso.Helpers; // --- CAMBIO: Añadido el using para el PasswordHelper
using Microsoft.AspNetCore.Authorization;
namespace SuMejorPeso.Controllers
{
    /// <summary>
    /// Controlador encargado de las operaciones CRUD (Alta, Baja y Modificación)
    /// de los Usuarios (personal) del sistema.
    /// </summary>
    [Authorize(Roles = "SuperUser")]
    public class UsersController : Controller
    {
        private readonly GymContext _context;

        public UsersController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Users/
        // Muestra la lista completa de usuarios ACTIVOS
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var users = await _context.User
                .Include(u => u.branch) 
                .Where(u => u.active) 
                .ToListAsync();

            return View(users);
        }

        // ============================================================
        // GET: /Users/Inactive
        // Muestra la lista de usuarios INACTIVOS
        // ============================================================
        public async Task<IActionResult> Inactive()
        {
            var users = await _context.User
                .Include(u => u.branch) 
                .Where(u => !u.active) 
                .ToListAsync();
            
            return View(users); 
        }

        // ============================================================
        // GET: /Users/Details/5
        // Muestra información detallada de un usuario
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User
                .Include(u => u.branch) 
                .FirstOrDefaultAsync(u => u.id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // ============================================================
        // GET: /Users/Create
        // Muestra el formulario para crear un nuevo usuario
        // ============================================================
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // ============================================================
        // POST: /Users/Create
        // Recibe los datos del formulario y crea el usuario en la BD
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // --- CAMBIO: Implementación real de Hashing ---
            
            // 1. Capturar la contraseña del formulario
            string plainTextPassword = Request.Form["Password"].ToString();

            // 2. Validar que la contraseña no esté vacía
            if (string.IsNullOrEmpty(plainTextPassword))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");
            }
            else
            {
                // 3. Generar el hash y salt reales
                var (hash, salt) = PasswordHelper.HashPassword(plainTextPassword);
                user.passwordHash = hash;
                user.salt = salt;
            }
            
            // 4. Asignar 'lastAccess' (sigue siendo requerido)
            user.lastAccess = DateTime.UtcNow;
            
            // --- FIN DEL CAMBIO ---

            // Limpiamos la validación de estos campos porque los asignamos manualmente
            ModelState.Remove(nameof(user.passwordHash));
            ModelState.Remove(nameof(user.salt));
            ModelState.Remove(nameof(user.lastAccess));
            ModelState.Remove(nameof(user.branch)); 

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(user.branchId);
                return View(user);
            }

            // --- CAMBIO: Validación de duplicados (UserName Y Email) ---
            var normalizedUserName = user.userName.ToLower();
            var normalizedEmail = user.email.ToLower();

            var existingUser = await _context.User
                .FirstOrDefaultAsync(u => 
                    u.userName.ToLower() == normalizedUserName || 
                    u.email.ToLower() == normalizedEmail); 

            if (existingUser != null)
            {
                if (existingUser.active)
                {
                    // Comprobamos qué campo falló
                    if (existingUser.userName.ToLower() == normalizedUserName)
                    {
                        ModelState.AddModelError(nameof(user.userName), "Ya existe un usuario ACTIVO con ese nombre de usuario.");
                    }
                    if (existingUser.email.ToLower() == normalizedEmail)
                    {
                        ModelState.AddModelError(nameof(user.email), "Ya existe un usuario ACTIVO con ese email.");
                    }
                    await LoadDropdownData(user.branchId);
                    return View(user);
                }
                else
                {
                    // Si está inactivo, ofrecemos reactivar (la lógica es la misma)
                    return RedirectToAction("ReactivateConfirmation", new { id = existingUser.id });
                }
            }
            // --- FIN DEL CAMBIO ---
            
            user.active = true;
            _context.User.Add(user);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /Users/Edit/5
        // Muestra el formulario para editar un usuario existente
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound();

            await LoadDropdownData(user.branchId);
            return View(user);
        }

        // ============================================================
        // POST: /Users/Edit/5
        // Recibe los cambios del formulario y actualiza el usuario
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User userFromForm)
        {
            if (id != userFromForm.id) return NotFound();

            // Evitar validación en propiedades que no queremos cambiar
            ModelState.Remove(nameof(userFromForm.passwordHash));
            ModelState.Remove(nameof(userFromForm.salt));
            ModelState.Remove(nameof(userFromForm.branch));

            // --- CAMBIO: Añadida validación de duplicados en Edit ---
            var normalizedUserName = userFromForm.userName.ToLower();
            var normalizedEmail = userFromForm.email.ToLower();

            var conflictingUser = await _context.User
                .FirstOrDefaultAsync(u => 
                    (u.userName.ToLower() == normalizedUserName || u.email.ToLower() == normalizedEmail) 
                    && u.id != userFromForm.id); // <-- ¡Importante! Excluir al usuario actual

            if (conflictingUser != null)
            {
                if (conflictingUser.userName.ToLower() == normalizedUserName)
                {
                    ModelState.AddModelError(nameof(userFromForm.userName), "Ese nombre de usuario ya está en uso por otra cuenta.");
                }
                if (conflictingUser.email.ToLower() == normalizedEmail)
                {
                    ModelState.AddModelError(nameof(userFromForm.email), "Ese email ya está en uso por otra cuenta.");
                }
            }
            // --- FIN DEL CAMBIO ---

            if (!ModelState.IsValid)
            {
                await LoadDropdownData(userFromForm.branchId);
                return View(userFromForm);
            }

            try
            {
                var userToUpdate = await _context.User.FindAsync(id);
                if (userToUpdate == null) return NotFound();

                // Actualizar solo los campos del formulario
                userToUpdate.userName = userFromForm.userName;
                userToUpdate.name = userFromForm.name;
                userToUpdate.lastName = userFromForm.lastName;
                userToUpdate.email = userFromForm.email;
                userToUpdate.Role = userFromForm.Role;
                userToUpdate.branchId = userFromForm.branchId;
                userToUpdate.active = userFromForm.active; 

                _context.Update(userToUpdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(userFromForm.id)) 
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
        // GET: /Users/Delete/5
        // Muestra confirmación antes de eliminar (desactivar) un usuario
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // ============================================================
        // POST: /Users/Delete/5
        // Ejecuta la eliminación LÓGICA (Soft Delete)
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            
            if (user != null)
            {
                user.active = false; 
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        // ============================================================
        // GET: /Users/ReactivateConfirmation/5
        // Muestra el formulario para confirmar reactivación
        // ============================================================
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User.FindAsync(id);

            if (user == null || user.active) // No debe ser activo ni nulo
                return NotFound();

            return View("ReactivateConfirmation", user);
        }
        
        // ============================================================
        // POST: /Users/Reactivate
        // Ejecuta la reactivación del usuario
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var user = await _context.User.FindAsync(id);
            
            if (user == null) return NotFound();

            user.active = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = user.id });
        }

        // ============================================================
        // MÉTODOS AUXILIARES (privados)
        // ============================================================
        
        /// <summary>
        /// Carga los datos de Roles y Sedes (activas) en el ViewBag
        /// para ser usados en dropdowns.
        /// </summary>
        private async Task LoadDropdownData(int? selectedBranchId = null)
        {
            // Cargar Roles desde el Enum
            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)));

            // Cargar Sedes (solo activas) desde la BD
            var branches = await _context.Branch.Where(b => b.active).ToListAsync();
            ViewBag.Branches = new SelectList(branches, "id", "name", selectedBranchId);
        }

        private bool UserExists(int id) 
        {
            return _context.User.Any(e => e.id == id);
        }
    }
}