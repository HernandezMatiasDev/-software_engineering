using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;

namespace SuMejorPeso.Controllers
{
    /// <summary>
    /// Controlador encargado de las operaciones CRUD (Alta, Baja y Modificación)
    /// de los miembros del gimnasio.
    /// </summary>
    public class MembersController : Controller
    {
        private readonly GymContext _context;

        // Inyecta el contexto (para acceder a la base de datos)
        public MembersController(GymContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Members/
        // Muestra la lista completa de miembros
        // ============================================================
        public async Task<IActionResult> Index()
        {
            // Incluimos las relaciones opcionales para mostrar más info
            var members = await _context.Member
                .Include(m => m.license)
                .Include(m => m.membership)
                .ToListAsync();

            // Devuelve la vista Index.cshtml y le pasa la lista de miembros
            return View(members);
        }

        // ============================================================
        // GET: /Members/Details/5
        // Muestra información detallada de un miembro
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Member
                .Include(m => m.license)
                .Include(m => m.membership)
                .FirstOrDefaultAsync(m => m.id == id);

            if (member == null) return NotFound();

            return View(member);
        }

        // ============================================================
        // GET: /Members/Create
        // Muestra el formulario para crear un nuevo miembro
        // ============================================================
        public IActionResult Create()
        {
            // Solo devuelve el formulario vacío
            return View();
        }

        // ============================================================
        // POST: /Members/Create
        // Recibe los datos del formulario y crea el miembro en la BD
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {
            // 1. Verificamos que el modelo cumpla las validaciones
            if (!ModelState.IsValid)
            {
                foreach (var e in ModelState)
                    Console.WriteLine($"{e.Key}: {string.Join(", ", e.Value.Errors.Select(err => err.ErrorMessage))}");

                // Si algo falla (campos vacíos o inválidos), se devuelve el form con errores
                return View(member);
            }

            // 2. Validación extra: evitar DNIs duplicados
            bool dniExiste = await _context.Member.AnyAsync(m => m.dni == member.dni);
            if (dniExiste)
            {
                ModelState.AddModelError(nameof(member.dni), "Ya existe un miembro con ese DNI.");
                return View(member);
            }

            // 3. Guardamos en la base de datos
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            // 4. Redirigimos a la lista
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /Members/Edit/5
        // Muestra el formulario para editar un miembro existente
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Member.FindAsync(id);
            if (member == null) return NotFound();

            return View(member);
        }

        // ============================================================
        // POST: /Members/Edit/5
        // Recibe los cambios del formulario y actualiza el miembro
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            if (id != member.id) return NotFound();

            // 1. Verificamos que pase las validaciones del modelo
            if (!ModelState.IsValid)

                return View(member);

            try
            {
                // 2. Actualizamos la entidad
                _context.Update(member);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si otro usuario lo borró mientras lo editábamos
                if (!MemberExists(member.id))
                    return NotFound();
                else
                    throw; // Re-lanzamos si fue otro tipo de error
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: /Members/Delete/5
        // Muestra confirmación antes de eliminar un miembro
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.id == id);

            if (member == null) return NotFound();

            return View(member);
        }

        // ============================================================
        // POST: /Members/Delete/5
        // Ejecuta la eliminación después de confirmar
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Member.FindAsync(id);
            if (member != null)
            {
                _context.Member.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // MÉTODO AUXILIAR (privado)
        // ============================================================
        private bool MemberExists(int id)
        {
            return _context.Member.Any(e => e.id == id);
        }
    }
}
