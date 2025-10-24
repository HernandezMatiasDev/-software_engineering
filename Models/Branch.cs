// En SuMejorPeso/Models/Branch.cs

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuMejorPeso.Models
{
    public class Branch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [StringLength(100)]
        public required string name { get; set; }

        public string? address { get; set; }
        public string? phone { get; set; }
        public required bool active { set; get; } = true;

        // --- Propiedades de Navegación (Relaciones 1:N) ---
        // Una sucursal tiene...

        /// <summary>
        /// El personal (Gerentes, Administradores) que trabaja EN esta sucursal.
        /// </summary>
        public ICollection<User> staff { get; set; }=  new List<User>();

        /// <summary>
        /// Los profesores que están asignados A esta sucursal.
        /// </summary>
        public ICollection<Coach> coaches { get; set; }=  new List<Coach>();

        /// <summary>
        /// Las clases (salones/actividades) que se imparten EN esta sucursal.
        /// </summary>
        public ICollection<Classroom> classrooms { get; set; } =  new List<Classroom>();

    }
}