namespace SuMejorPeso.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Member : Person
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly birthdate { get; set; }

    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string direction { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un género")]
    [Display(Name = "Género")]
    public string gender { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool active { get; set; } = true;

    // Relación con licencia (opcional)
    [Display(Name = "Licencia")]
    public License? license { get; set; }
    public int? licenseId { get; set; }

    // Relación con membresía (opcional)
    [Display(Name = "Membresía")]
    public Membership? membership { get; set; }
    public int? membershipId { get; set; }

    // Relación con asistencias
    public ICollection<Attendance> attendance { get; set; } = new List<Attendance>();

    [Display(Name = "Notas")]
    public string note { get; set; } = string.Empty;

    // Relación con aulas
    public ICollection<Classroom> classrooms { get; set; } = new List<Classroom>();
}
