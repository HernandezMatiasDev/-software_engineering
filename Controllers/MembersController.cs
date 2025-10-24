using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using SuMejorPeso.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace SuMejorPeso.Controllers
{
    /// <summary>
    /// Controlador encargado de las operaciones CRUD (Alta, Baja y Modificación)
    /// de los miembros del gimnasio.
    /// </summary>
    [Authorize(Roles = "SuperUser")]
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
            // Filtrar solo a los miembros activos (active == true)
            var members = await _context.Member
                .Where(m => m.active) // <-- Nuevo filtro: trae solo donde active es true
                .Include(m => m.license)
                .Include(m => m.membership)
                .ToListAsync();

            return View(members);
        }

        // ============================================================
        // GET: /Members/Inactive
        // Muestra la lista de miembros INACTIVOS
        // ============================================================
        public async Task<IActionResult> Inactive()
        {
            // Filtrar solo a los miembros inactivos (active == false)
            var members = await _context.Member
                .Where(m => !m.active) // <-- Filtro: trae solo donde active es false
                .Include(m => m.license)
                .Include(m => m.membership)
                .ToListAsync();

            // Devolvemos una nueva vista Inactive.cshtml
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
        public async Task<IActionResult> Create() // <-- CAMBIO: Ahora es 'async Task'
        {
            // --- CAMBIO: Cargar los tipos de membresía activos ---
            var activeTypes = await _context.TypeMembreship
                .Where(t => t.active)
                .ToListAsync();

            ViewBag.TypeMembreshipList = new SelectList(activeTypes, "id", "name");
            // --- FIN DEL CAMBIO ---

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
            // 1. Validación de Modelo
            if (!ModelState.IsValid)
            {
                // Si falla, volvemos a cargar el dropdown
                var activeTypes = await _context.TypeMembreship.Where(t => t.active).ToListAsync();
                ViewBag.TypeMembreshipList = new SelectList(activeTypes, "id", "name");
                return View(member);
            }

            // 2. Validación de DNI Duplicado
            var existingMember = await _context.Member
                .FirstOrDefaultAsync(m => m.dni == member.dni);

            if (existingMember != null)
            {
                // ... (tu lógica de error y reactivación)
                // (Recargar el dropdown si hay error)
                var activeTypes = await _context.TypeMembreship.Where(t => t.active).ToListAsync();
                ViewBag.TypeMembreshipList = new SelectList(activeTypes, "id", "name");
                return View(member);
            }

            // --- LÓGICA DE LICENCIA (ya la tienes) ---
            string barcodeString = BarcodeHelper.GenerateEAN13(member.dni.ToString());
            var newLicense = new License
            {
                barcode = barcodeString,
                active = true,
                member = member,
                startDate = DateOnly.FromDateTime(DateTime.Now),
                endDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            };
            member.license = newLicense;

            // ============================================================
            // --- INICIO DE LA NUEVA LÓGICA DE MEMBERSHIP ---
            // ============================================================

            // 3. Obtener el TypeMembreship seleccionado del dropdown
            // (Necesitamos el 'membershipId' que viene del formulario)
            if (member.membershipId != null) //
            {
                var selectedType = await _context.TypeMembreship.FindAsync(member.membershipId);

                if (selectedType != null)
                {
                    // 4. Crear el objeto Membership
                    var newMembership = new Membership
                    {
                        type = selectedType,
                        state = "Pagada", // O "Pendiente de Pago"
                        active = true,
                        pricePaid = selectedType.price,
                        debt = 0,
                        discount = 0,

                        // Propiedades de BaseRecord
                        member = member,
                        startDate = DateOnly.FromDateTime(DateTime.Now),
                        endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(selectedType.daysDuration))
                    };

                    // 5. Asignar la membresía al miembro
                    member.membership = newMembership; //
                }
            }
            // ============================================================
            // --- FIN DE LA NUEVA LÓGICA ---
            // ============================================================

            // 6. Guardamos en la base de datos
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            // 7. Redirigimos a la lista
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
        // Ejecuta la eliminación LÓGICA (Soft Delete)
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Member.FindAsync(id);

            // Solo procedemos si el miembro existe
            if (member != null)
            {
                // **ESTE ES EL CAMBIO CLAVE:** // 1. Cambiamos el estado a inactivo (Eliminación Lógica)
                member.active = false;

                // 2. Le decimos a EF Core que el objeto ha sido modificado
                _context.Update(member);

                // 3. Guardamos el cambio en la BD
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        // ============================================================
        // GET: /Members/ReactivateConfirmation/5
        // Muestra el formulario para confirmar reactivación
        // ============================================================
        public async Task<IActionResult> ReactivateConfirmation(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Member.FindAsync(id);

            if (member == null || member.active) // No debe ser activo ni nulo
                return NotFound();

            // Usamos la misma vista Delete.cshtml (o una nueva, si quieres más info)
            // Pero la modificaremos para que su formulario apunte a Reactivate
            return View("ReactivateConfirmation", member);
        }
        // ============================================================
        // POST: /Members/Reactivate
        // Ejecuta la reactivación del miembro
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var member = await _context.Member.FindAsync(id);

            if (member == null) return NotFound();

            // **Reactivar:** Establecer el estado en true
            member.active = true;
            _context.Update(member);
            await _context.SaveChangesAsync();

            // Redirigir a la vista de detalles para que vea que se activó
            return RedirectToAction("Details", new { id = member.id });
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
