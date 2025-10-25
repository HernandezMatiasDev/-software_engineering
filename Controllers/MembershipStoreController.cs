using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SuMejorPeso.Helpers; // Para BarcodeHelper
using SuMejorPeso.ViewModels;
using Microsoft.AspNetCore.Authentication; // Necesario para SignOutAsync/SignInAsync
using Microsoft.AspNetCore.Authentication.Cookies; // Necesario para CookieAuthenticationDefaults

namespace SuMejorPeso.Controllers
{
    [Authorize(Roles = "defaults, member")] // Solo usuarios registrados pueden ver la tienda
    public class MembershipStoreController : Controller
    {
        private readonly GymContext _context;

        public MembershipStoreController(GymContext context)
        {
            _context = context;
        }

        // GET: /MembershipStore/Index
        // Muestra la lista de planes de membresía disponibles
        public async Task<IActionResult> Index()
        {
            var availablePlans = await _context.TypeMembreship
                .Where(t => t.active)
                .ToListAsync();
            return View(availablePlans);
        }

        // GET: /MembershipStore/ConfirmPurchase/{id}
        // Muestra una página de confirmación antes de ir a pedir datos
        public async Task<IActionResult> ConfirmPurchase(int? id)
        {
            if (id == null) return NotFound();

            var plan = await _context.TypeMembreship.FindAsync(id);
            if (plan == null || !plan.active) return NotFound();

            if (User.IsInRole(UserRole.member.ToString())) //
            {
                 ViewBag.ErrorMessage = "Ya eres un miembro activo.";
                 return View("PurchaseError");
            }

            // Pasa el objeto plan completo a la vista de confirmación
            return View(plan);
        }

        // GET: /MembershipStore/EnterMemberDetails?typeMembershipId=5
        // Muestra el formulario para ingresar los datos del miembro
        public async Task<IActionResult> EnterMemberDetails(int typeMembershipId)
        {
            // Verificar que el plan exista y esté activo
            var plan = await _context.TypeMembreship.FindAsync(typeMembershipId);
            if (plan == null || !plan.active)
            {
                return NotFound("Plan no válido.");
            }
             // Si ya es miembro, no permitir
            if (User.IsInRole(UserRole.member.ToString())) //
            {
                ViewBag.ErrorMessage = "Ya eres un miembro activo.";
                return View("PurchaseError");
            }

            // Creamos el ViewModel y le pasamos el ID del plan
            var model = new MemberDetailsViewModel
            {
                TypeMembershipId = typeMembershipId
            };

            ViewBag.PlanName = plan.name; // Pasa el nombre del plan a la vista
            return View(model);
        }

// ============================================================
        // POST: /MembershipStore/CompletePurchase
        // Acción final que recibe los datos del miembro y activa la cuenta
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePurchase(MemberDetailsViewModel memberDetails)
        {
             // Doble check por si acaso
             if (User.IsInRole(UserRole.member.ToString())) //
             {
                 ViewBag.ErrorMessage = "Ya eres un miembro activo.";
                 return View("PurchaseError");
             }

             // Validar el ViewModel que viene del formulario
             if (!ModelState.IsValid)
             {
                 var planValidation = await _context.TypeMembreship.FindAsync(memberDetails.TypeMembershipId);
                 ViewBag.PlanName = planValidation?.name ?? "Plan";
                 return View("EnterMemberDetails", memberDetails);
             }

            // --- Obtener datos del Usuario y Plan ---
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int currentUserId))
            {
                 return RedirectToAction("Logout", "Account");
            }
            var user = await _context.User.FindAsync(currentUserId);
            var selectedType = await _context.TypeMembreship.FindAsync(memberDetails.TypeMembershipId);
            if (user == null || selectedType == null || user.Role != UserRole.defaults) //
            {
                 ViewBag.ErrorMessage = "Error al procesar la compra. Usuario o plan no válido.";
                 return View("PurchaseError");
            }

            // --- Validación DNI Duplicado ---
            var existingMember = await _context.Member.FirstOrDefaultAsync(m => m.dni == memberDetails.Dni); //
            if (existingMember != null)
            {
                ModelState.AddModelError(nameof(memberDetails.Dni), "Ya existe un miembro (activo o inactivo) con ese DNI.");
                var planDni = await _context.TypeMembreship.FindAsync(memberDetails.TypeMembershipId);
                ViewBag.PlanName = planDni?.name ?? "Plan";
                return View("EnterMemberDetails", memberDetails);
            }
            // --- Fin Validación DNI ---

            // --- Lógica Principal: Crear Entidades y Actualizar Rol ---
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                 // 1. Crear el nuevo Member (sin guardarlo aún)
                 var newMember = new Member
                 {
                     name = user.name,
                     lastName = user.lastName,
                     email = user.email,
                     userId = user.id, //
                     user = user,
                     active = true, //
                     dni = memberDetails.Dni, //
                     birthdate = memberDetails.Birthdate, //
                     gender = memberDetails.Gender, //
                     phone = memberDetails.Phone, //
                     direction = memberDetails.Direction ?? "", //
                     note = "Creado desde compra online simulada", //
                 };

                // 2. Crear la License (sin guardarla aún)
                string barcodeString = BarcodeHelper.GenerateEAN13(newMember.dni.ToString()); //
                var newLicense = new License
                {
                    barcode = barcodeString, //
                    active = true, //
                    member = newMember, //
                    startDate = DateOnly.FromDateTime(DateTime.Now), //
                    endDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)) //
                };
                newMember.license = newLicense; //

                // 3. Crear la Membership (sin guardarla aún)
                var newMembership = new Membership
                {
                    type = selectedType, //
                    state = "Pagada (Simulado)", //
                    active = true, //
                    pricePaid = selectedType.price, //
                    debt = 0, //
                    discount = 0, //
                    member = newMember, //
                    startDate = DateOnly.FromDateTime(DateTime.Now), //
                    endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(selectedType.daysDuration)) //
                };
                newMember.membership = newMembership; //

                 // 4. Actualizar el Rol del User (preparar el cambio)
                 user.Role = UserRole.member; //

                 // ============================================
                 // --- PRIMER GUARDADO ---
                 // ============================================
                 // 5. Guardar Member (con License y Membership en cascada) y User
                 _context.Member.Add(newMember);
                 _context.User.Update(user);
                 await _context.SaveChangesAsync(); // <-- GUARDAMOS AQUÍ PARA OBTENER IDs

                 // ============================================
                 // --- CREACIÓN DE PAY (AHORA SÍ) ---
                 // ============================================
                 // 6. Crear el Pay usando los IDs generados
                 var newPay = new Pay
                 {
                     memberId = newMember.id,           // <-- ID OBTENIDO
                     membershipId = newMembership.id,   // <-- ID OBTENIDO
                     date = DateTime.UtcNow,            //
                     amount = (int)selectedType.price,  //
                     paymentMethod = "Simulado (Mercado Pago)" //
                 };
                 _context.Pay.Add(newPay); // Añadimos el Pay a la cola de guardado

                 // ============================================
                 // --- SEGUNDO GUARDADO ---
                 // ============================================
                 // 7. Guardar el Pay
                 await _context.SaveChangesAsync(); // <-- GUARDAMOS EL PAY

                 // 8. Confirmar la transacción
                 await transaction.CommitAsync();

                 // --- INICIO NUEVA LÓGICA: REFRESCAR AUTENTICACIÓN ---
                 // 9. Refrescar la cookie de autenticación con el nuevo rol
                 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                 var claims = new List<Claim>
                 {
                     new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                     new Claim(ClaimTypes.Name, user.userName),
                     new Claim(ClaimTypes.Role, user.Role.ToString()), // <-- ¡Ahora leerá 'member'!
                     new Claim("BranchId", user.branchId?.ToString() ?? "")
                 };
                 var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                 // --- FIN NUEVA LÓGICA ---

                 // 10. Redirigir al perfil
                 TempData["SuccessMessage"] = "¡Membresía activada con éxito! Ya eres miembro.";
                 return RedirectToAction("Index", "MyProfile");
            }
            catch (Exception ex)
            {
                 await transaction.RollbackAsync();
                 // logger.LogError(ex, "Error al completar la compra para User ID {UserId}", currentUserId);
                 ViewBag.ErrorMessage = $"Ocurrió un error inesperado al activar la membresía: {ex.Message}";
                 return View("PurchaseError");
            }
            // --- FIN DE LA LÓGICA DE CREACIÓN ---
        } // Fin de CompletePurchase


    } // Fin de MembershipStoreController
}