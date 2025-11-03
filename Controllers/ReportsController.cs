using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuMejorPeso.Models;
using System.ComponentModel.DataAnnotations;
using System.IO; // Necesario para MemoryStream
using ClosedXML.Excel; // Para Excel
using QuestPDF.Fluent; // Para PDF
using QuestPDF.Helpers; // Para PDF
using QuestPDF.Infrastructure; // Para PDF

namespace SuMejorPeso.Controllers
{
    [Authorize(Roles = "SuperUser, Manager, Administrator")]
    public class ReportsController : Controller
    {
        private readonly GymContext _context;

        public ReportsController(GymContext context)
        {
            _context = context;
        }

        // --- PÁGINA PRINCIPAL DE REPORTES ---
        // GET: /Reports/
        public IActionResult Index()
        {
            return View();
        }

        #region ViewModels
        // --- VIEWMODELS (Modelos para las vistas) ---
        public class AttendanceReportViewModel
        {
            [Display(Name = "Fecha de Inicio")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

            [Display(Name = "Fecha de Fin")]
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; } = DateTime.UtcNow.Date;
            
            public List<Attendance> Results { get; set; } = new List<Attendance>();
        }

        public class DebtReportViewModel
        {
            public List<Member> MembersWithDebt { get; set; } = new List<Member>();
            public float TotalDebt { get; set; } = 0;
        }

        // --- NUEVO VIEWMODEL ---
        public class EarningsReportViewModel
        {
            [Display(Name = "Fecha de Inicio")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

            [Display(Name = "Fecha de Fin")]
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; } = DateTime.UtcNow.Date;

            public List<Pay> Results { get; set; } = new List<Pay>();
            public float TotalEarnings { get; set; } = 0;
        }
        #endregion

        #region Reporte Asistencias
        // ===============================================
        // REPORTE 1: ASISTENCIAS
        // ===============================================

        [HttpGet]
        public IActionResult AttendanceReport()
        {
            var vm = new AttendanceReportViewModel
            {
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendanceReport(AttendanceReportViewModel vm)
        {
            DateTime endDateAdjusted = vm.EndDate.AddDays(1).AddTicks(-1);

            var results = await _context.Attendance
                .Include(a => a.member)
                .Include(a => a.classroom)
                .Where(a => a.date >= vm.StartDate && a.date <= endDateAdjusted)
                .OrderByDescending(a => a.date)
                .ToListAsync();

            vm.Results = results;
            return View(vm);
        }

        // --- EXPORTACIÓN ASISTENCIAS ---
        [HttpGet]
        public async Task<IActionResult> ExportAttendanceToExcel(DateTime startDate, DateTime endDate)
        {
            DateTime endDateAdjusted = endDate.AddDays(1).AddTicks(-1);
            var results = await _context.Attendance
                .Include(a => a.member)
                .Include(a => a.classroom)
                .Where(a => a.date >= startDate && a.date <= endDateAdjusted)
                .OrderByDescending(a => a.date)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Asistencias");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Fecha y Hora";
                worksheet.Cell(currentRow, 2).Value = "Miembro";
                worksheet.Cell(currentRow, 3).Value = "Clase";

                foreach (var item in results)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.date;
                    worksheet.Cell(currentRow, 2).Value = $"{item.member?.name} {item.member?.lastName}";
                    worksheet.Cell(currentRow, 3).Value = item.classroom?.name;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteAsistencias.xlsx");
                }
            }
        }
        #endregion

        #region Reporte Deudores
        // ===============================================
        // REPORTE 2: DEUDORES
        // ===============================================

        [HttpGet]
        public async Task<IActionResult> DebtReport()
        {
            var membersWithDebt = await _context.Member
                .Include(m => m.membership)
                .Where(m => m.membership != null && m.membership.debt > 0)
                .OrderByDescending(m => m.membership.debt)
                .ToListAsync();

            var vm = new DebtReportViewModel
            {
                MembersWithDebt = membersWithDebt,
                TotalDebt = membersWithDebt.Sum(m => m.membership.debt)
            };

            return View(vm);
        }

        // --- EXPORTACIÓN DEUDORES ---
        [HttpGet]
        public async Task<IActionResult> ExportDebtToExcel()
        {
            var membersWithDebt = await _context.Member
                .Include(m => m.membership)
                .Where(m => m.membership != null && m.membership.debt > 0)
                .OrderByDescending(m => m.membership.debt)
                .ToListAsync();
            
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Deudores");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Miembro";
                worksheet.Cell(currentRow, 2).Value = "Email";
                worksheet.Cell(currentRow, 3).Value = "Deuda";

                foreach (var member in membersWithDebt)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = $"{member.name} {member.lastName}";
                    worksheet.Cell(currentRow, 2).Value = member.email;
                    worksheet.Cell(currentRow, 3).Value = member.membership?.debt;
                }
                
                worksheet.Cell(currentRow + 2, 2).Value = "Deuda Total";
                worksheet.Cell(currentRow + 2, 3).Value = membersWithDebt.Sum(m => m.membership.debt);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteDeudores.xlsx");
                }
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> ExportDebtToPdf()
        {
            var membersWithDebt = await _context.Member
                .Include(m => m.membership)
                .Where(m => m.membership != null && m.membership.debt > 0)
                .OrderByDescending(m => m.membership.debt)
                .ToListAsync();

            var totalDebt = membersWithDebt.Sum(m => m.membership.debt);

            // Usamos la clase helper de PDF definida abajo
            var document = new DebtReportDocument(membersWithDebt, totalDebt);
            var pdfData = document.GeneratePdf(); // Genera el PDF en memoria
            
            return File(pdfData, "application/pdf", "ReporteDeudores.pdf");
        }
        #endregion

        #region Reporte Ganancias (NUEVO)
        // ===============================================
        // REPORTE 3: GANANCIAS (NUEVO)
        // ===============================================

        [HttpGet]
        public IActionResult EarningsReport()
        {
            var vm = new EarningsReportViewModel
            {
                StartDate = DateTime.UtcNow.Date.AddDays(-30),
                EndDate = DateTime.UtcNow.Date
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EarningsReport(EarningsReportViewModel vm)
        {
            DateTime endDateAdjusted = vm.EndDate.AddDays(1).AddTicks(-1);

            var results = await _context.Pay // Usamos el DbSet de Pay
                .Include(p => p.member) // Incluimos al miembro (gracias al Paso 2)
                .Where(p => p.date >= vm.StartDate && p.date <= endDateAdjusted)
                .OrderByDescending(p => p.date)
                .ToListAsync();

            vm.Results = results;
            vm.TotalEarnings = results.Sum(p => p.amount); // Sumamos el total
            return View(vm);
        }

        // --- EXPORTACIÓN GANANCIAS ---
        [HttpGet]
        public async Task<IActionResult> ExportEarningsToExcel(DateTime startDate, DateTime endDate)
        {
            DateTime endDateAdjusted = endDate.AddDays(1).AddTicks(-1);
            var results = await _context.Pay
                .Include(p => p.member)
                .Where(p => p.date >= startDate && p.date <= endDateAdjusted)
                .OrderByDescending(p => p.date)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ganancias");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Fecha";
                worksheet.Cell(currentRow, 2).Value = "Miembro";
                worksheet.Cell(currentRow, 3).Value = "Método de Pago";
                worksheet.Cell(currentRow, 4).Value = "Monto";

                foreach (var item in results)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.date;
                    worksheet.Cell(currentRow, 2).Value = $"{item.member?.name} {item.member?.lastName}";
                    worksheet.Cell(currentRow, 3).Value = item.paymentMethod;
                    worksheet.Cell(currentRow, 4).Value = item.amount;
                }

                worksheet.Cell(currentRow + 2, 3).Value = "Total Ganancias";
                worksheet.Cell(currentRow + 2, 4).Value = results.Sum(p => p.amount);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteGanancias.xlsx");
                }
            }
        }
        #endregion
    }

    #region Clases Helper para PDF (QuestPDF)
    // --- CLASE HELPER PARA PDF DE DEUDORES ---
    // (Pon esta clase en el mismo archivo, pero FUERA de la clase ReportsController)
    public class DebtReportDocument : IDocument
    {
        private readonly List<Member> _members;
        private readonly float _totalDebt;

        public DebtReportDocument(List<Member> members, float totalDebt)
        {
            _members = members;
            _totalDebt = totalDebt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    // --- Encabezado ---
                    page.Header()
                        .Text("Reporte de Deudores")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    // --- Contenido (Tabla) ---
                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(10);
                            
                            // --- La Tabla ---
                            col.Item().Table(table =>
                            {
                                // Definir columnas
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Nombre
                                    columns.RelativeColumn(3); // Email
                                    columns.RelativeColumn(1); // Deuda
                                });

                                // Encabezados de tabla
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Text("Miembro");
                                    header.Cell().Background(Colors.Grey.Lighten3).Text("Email");
                                    header.Cell().Background(Colors.Grey.Lighten3).Text("Deuda").AlignRight();
                                });

                                // Datos de la tabla
                                foreach (var member in _members)
                                {
                                    table.Cell().Text($"{member.name} {member.lastName}");
                                    table.Cell().Text(member.email);
                                    table.Cell().Text($"${member.membership?.debt:N2}").AlignRight();
                                }
                            });
                            
                            // --- Total ---
                            col.Item().AlignRight()
                                .Text($"Deuda Total: ${_totalDebt:N2}")
                                .SemiBold().FontSize(14).FontColor(Colors.Red.Medium);
                        });

                    // --- Pie de página ---
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                        });
                });
        }
    }
    
    // (Puedes crear clases similares para los reportes de Asistencia y Ganancias)
    #endregion
}