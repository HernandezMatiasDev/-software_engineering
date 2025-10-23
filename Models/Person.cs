namespace SuMejorPeso.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public abstract class Person
{


    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre")]
    public string name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [Display(Name = "Apellido")]
    public string lastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [Display(Name = "DNI")]
    public int dni { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Display(Name = "Teléfono")]
    public string phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [Display(Name = "Email")]
    public string email { get; set; } = string.Empty;

    //[Required(ErrorMessage = "Debe asignarse un usuario")] //por ahora no, temporalmente null
    [Display(Name = "Usuario")]
    public User? user { get; set; } = null!;

    //[Required] //por ahora no, temporalmente null
    public int? userId { get; set; }
}
