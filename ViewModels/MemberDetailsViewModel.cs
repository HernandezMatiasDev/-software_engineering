using System.ComponentModel.DataAnnotations;
using SuMejorPeso.Models;

namespace SuMejorPeso.ViewModels
{
    public class MemberDetailsViewModel
    {
        // Campos a pedir en el formulario
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [Display(Name = "DNI")]
        public int Dni { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateOnly Birthdate { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un género")]
        [Display(Name = "Género")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Dirección (Opcional)")]
        public string? Direction { get; set; }

        // Campo oculto para saber qué membresía se está comprando
        public int TypeMembershipId { get; set; }
    }
}