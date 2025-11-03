using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models; // Tu GymContext está aquí
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SuMejorPeso.Controllers
{
    // 1. Autorización: Solo estos roles pueden acceder a este controlador
    [Authorize(Roles = "SuperUser, Manager, Administrator, coach")]
    public class AttendanceController : Controller
    {
        private readonly GymContext _context;

        public AttendanceController(GymContext context)
        {
            _context = context;
        }

        // --- ViewModel para el formulario ---
        public class TakeAttendanceViewModel
        {
            [Display(Name = "Código de Barras")]
            [Required(ErrorMessage = "Debe ingresar un código de barras")]
            public string Barcode { get; set; } = string.Empty;

            [Display(Name = "Clase")]
            [Required(ErrorMessage = "Debe seleccionar una clase")]
            public int ClassroomId { get; set; }
        }


        // --- GET: /Attendance/Index ---
        // Muestra la página para tomar asistencia
        public async Task<IActionResult> Index()
        {
            await LoadClassroomsViewBag(); // Carga el dropdown de clases
            return View(new TakeAttendanceViewModel());
        }


        // --- POST: /Attendance/TakeAttendance ---
        // Procesa el formulario de asistencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeAttendance(TakeAttendanceViewModel model)
        {
            // Si el modelo no es válido (ej. campos vacíos), vuelve a mostrar el formulario
            if (!ModelState.IsValid)
            {
                await LoadClassroomsViewBag();
                return View("Index", model);
            }

            // 1. Buscar al miembro por su código de barras (usando la Licencia)
            //    CORRECCIÓN: Se usa _context.Member (singular)
            var member = await _context.Member
                                 .Include(m => m.license) // Incluimos la licencia para buscar
                                 .FirstOrDefaultAsync(m => m.license != null && m.license.barcode == model.Barcode);

            // 2. Validar que el miembro exista y esté activo
            if (member == null)
            {
                TempData["ErrorMessage"] = "Código de barras no encontrado. No existe ningún miembro con esa licencia.";
                await LoadClassroomsViewBag();
                return View("Index", model);
            }

            if (!member.active)
            {
                TempData["ErrorMessage"] = $"El miembro {member.name} {member.lastName} se encuentra INACTIVO.";
                await LoadClassroomsViewBag();
                return View("Index", model);
            }

            // 3. Validar que la clase exista y que el miembro esté inscripto
            //    (Este ya estaba bien, tu DbSet es Classrooms (plural))
            var classroom = await _context.Classrooms
                                    .Include(c => c.members) // Incluimos los miembros de la clase
                                    .FirstOrDefaultAsync(c => c.id == model.ClassroomId);

            if (classroom == null || !classroom.active)
            {
                TempData["ErrorMessage"] = "La clase seleccionada no existe o está inactiva.";
                await LoadClassroomsViewBag();
                return View("Index", model);
            }

            // Verificamos si el miembro está en la colección 'members' de la clase
            if (!classroom.members.Any(m => m.id == member.id))
            {
                TempData["ErrorMessage"] = $"El miembro {member.name} no está inscripto en la clase {classroom.name}.";
                await LoadClassroomsViewBag();
                return View("Index", model);
            }

            // 4. Validar que no tenga ya una asistencia para esta clase HOY
            var today = DateTime.UtcNow.Date;
            //    CORRECCIÓN: Se usa _context.Attendance (singular)
            var existingAttendance = await _context.Attendance
                .AnyAsync(a => a.classId == model.ClassroomId &&
                               a.memberId == member.id &&
                               a.date.Date == today);

            if (existingAttendance)
            {
                TempData["WarningMessage"] = $"El miembro {member.name} {member.lastName} ya tiene asistencia registrada para esta clase hoy.";
                await LoadClassroomsViewBag();
                return View("Index", model);
            }

            // 5. ¡Todo en orden! Creamos la asistencia
            var newAttendance = new Attendance
            {
                classId = model.ClassroomId,
                memberId = member.id,
                date = DateTime.UtcNow // Guardamos la fecha y hora actual
            };

            //    CORRECCIÓN: Se usa _context.Attendance (singular)
            _context.Attendance.Add(newAttendance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"¡Asistencia registrada para {member.name} {member.lastName} en la clase {classroom.name}!";

            // Redirigimos al Index (limpiando el formulario)
            return RedirectToAction("Index");
        }


        // --- Método privado para cargar el dropdown de clases ---
        private async Task LoadClassroomsViewBag()
        {
            // (Este ya estaba bien, tu DbSet es Classrooms (plural))
            ViewBag.Classrooms = new SelectList(
                await _context.Classrooms.Where(c => c.active).ToListAsync(), // Solo clases activas
                "id",
                "name"
            );
        }
    }
}